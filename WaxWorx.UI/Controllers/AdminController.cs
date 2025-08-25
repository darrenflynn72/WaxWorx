using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WaxWorx.Controllers;
using WaxWorx.Core;
using WaxWorx.Core.Import;
using WaxWorx.CoverArtApi;
using WaxWorx.Data;
using WaxWorx.Data.Entities;
using WaxWorx.MusicBrainzApi;
using WaxWorx.Shared.Configurations;
using WaxWorx.UI.ViewModels;

namespace WaxWorx.UI.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly InventoryDbContext _context;

        // configs
        private readonly CoverArtApiConfig _configCoverArtApi;
        private readonly MusicBrainzApiConfig _configMusicBrainzApi;
        private readonly AdminSettingsConfig _configAdminSettings;

        // external APIs
        private readonly CoverArtApiClient _coverArtApiClient;
        private readonly MusicBrainzApiClient _musicBrainzApiClient;

        // CSV Importer
        private readonly CsvImporter _importer;

        public AdminController(ILogger<AdminController> logger, InventoryDbContext context,
                                IOptions<CoverArtApiConfig> configCoverArtApi, IOptions<MusicBrainzApiConfig> configMusicBrainzApi, IOptions<AdminSettingsConfig> configAdminSettings,
                                CoverArtApiClient coverArtApiClient, MusicBrainzApiClient musicBrainzApiClient, CsvImporter importer)
        {
            _logger = logger;
            _context = context;
            _configCoverArtApi = configCoverArtApi.Value;
            _configMusicBrainzApi = configMusicBrainzApi.Value;
            _configAdminSettings = configAdminSettings.Value;
            _coverArtApiClient = coverArtApiClient;
            _musicBrainzApiClient = musicBrainzApiClient;
            _importer = importer;
        }

        // Admin page with options
        public IActionResult Index()
        {
            // Admin Main

            return View();
        }

        public IActionResult Login()
        {
            // Login

            return View();
        }

        public IActionResult Settings()
        {
            // Settings

            return View();
        }

        public IActionResult Other()
        {
            // Other

            return Ok("Other");
        }

        public IActionResult RunImport(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                //TempData["ImportError"] = "No file selected.";
                TempData["AlertMessage"] = "No file selected.";

                return RedirectToAction("Settings"); // Or wherever your form lives
            }

            using var stream = csvFile.OpenReadStream();
            _importer.Import(stream);

            //TempData["ImportSuccess"] = "Import triggered.";
            TempData["AlertMessage"] = "Import Complete.";

            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAlbumTrackListing()
        {
            var albums = _context.Albums
                .Include(a => a.Tracks)
                .Where(a => !string.IsNullOrWhiteSpace(a.MbId) && (a.Tracks == null || !a.Tracks.Any()))
                .ToList();

            const int delayMs = 1000; // throttle delay between requests

            foreach (var album in albums)
            {
                try
                {
                    var mbid = album.MbId;
                    var tracks = await _musicBrainzApiClient.GetTracksByMbIdAsync(mbid);

                    if (tracks != null && tracks.Any())
                    {
                        var now = DateTime.UtcNow;
                        var newTracks = tracks.Select(track => new Track
                        {
                            Name = track.Title,
                            Duration = (int)track.Duration.TotalSeconds,
                            TrackNumber = track.Position,
                            AlbumId = album.Id,
                            CreatedDate = now,
                            CreatedBy = "admin",
                            ModifiedDate = now,
                            ModifiedBy = "admin"
                        }).ToList();

                        // Attach album if not tracked
                        if (!_context.Albums.Local.Any(a => a.Id == album.Id))
                        {
                            _context.Albums.Attach(album);
                        }

                        _context.Tracks.AddRange(newTracks);

                        album.ModifiedDate = now;
                        album.ModifiedBy = "admin";
                        _context.Albums.Update(album);

                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear(); // prevent memory bloat
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, $"Failed to update tracklist for album ID {album.Id}: {album.Title}");
                }

                await Task.Delay(delayMs); // throttle to avoid API rate limits
            }

            return Ok("Tracklist update attempted.");
        }

        public static class DurationConverter
        {
            public static int ToSeconds(TimeSpan duration) => (int)duration.TotalSeconds;
            public static TimeSpan ToTimeSpan(int seconds) => TimeSpan.FromSeconds(seconds);
        }

        public async Task<IActionResult> UpdateAlbumCdnUrls()
        {
            // only pull albums that don't have an Image url
            // no point polling API for 300+ albums
            var albums = _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Image)
                .Where(a => a.Image == null || string.IsNullOrEmpty(a.Image.CoverUrl))
                .ToList();

            const int delayMs = 1000; // throttle delay between requests

            foreach (var album in albums)
            {
                try
                {
                    var artistName = NormalizeArtistName(album.Artist?.Name);
                    var albumTitle = album.Title;

                    var mbid = await _musicBrainzApiClient.GetMBIDAsync(albumTitle, artistName);

                    if (!string.IsNullOrWhiteSpace(mbid))
                    {
                        album.MbId = mbid; // Persist MBID regardless of CDN outcome

                        var cdnUrl = await _coverArtApiClient.GetFrontCoverArtUrlAsync(mbid);

                        if (!string.IsNullOrWhiteSpace(cdnUrl))
                        {
                            var now = DateTime.UtcNow;

                            if (album.Image == null)
                            {
                                album.Image = new Image
                                {
                                    CoverUrl = cdnUrl,
                                    CreatedDate = now,
                                    CreatedBy = "admin",
                                    ModifiedDate = now,
                                    ModifiedBy = "admin"
                                };
                            }
                            else
                            {
                                album.Image.CoverUrl = cdnUrl;
                                album.Image.ModifiedDate = now;
                                album.Image.ModifiedBy = "admin";
                            }

                            album.ModifiedDate = now;
                            album.ModifiedBy = "admin";
                        }

                        _context.Update(album);
                    }
                }
                catch (Exception ex)
                {
                    // Optional: log or tag the failed album for review
                    _logger?.LogWarning(ex, $"Failed to update CDN for album ID {album.Id}: {album.Title}");
                }

                await Task.Delay(delayMs); // throttle to avoid API rate limits
            }

            // Uncomment when ready to persist
            await _context.SaveChangesAsync();

            return Ok("CDN URLs update attempted.");
        }

        public string NormalizeArtistName(string rawName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rawName))
                {
                    return "";
                }

                var parts = rawName.Split(',');

                if (parts.Length == 2)
                {
                    var first = parts[1].Trim();
                    var last = parts[0].Trim();

                    return $"{first} {last}";
                }

                return rawName.Trim();
            }
            catch (Exception ex)
            {
                // Optional: log exception for audit trace
                _logger?.LogWarning(ex, $"NormalizeArtistName failed for input: '{rawName}'");

                return rawName ?? "";
            }
        }
    }
}
