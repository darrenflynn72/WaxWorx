using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WaxWorx.Controllers;
using WaxWorx.CoverArtApi;
using WaxWorx.Data;
using WaxWorx.Data.Entities;
using WaxWorx.Models;
using WaxWorx.MusicBrainzApi;
using WaxWorx.Shared.Configurations;
using WaxWorx.UI.ViewModels;

namespace WaxWorx.UI.Controllers
{
    public class MusicBrainzController : Controller
    {
        private readonly ILogger<MusicBrainzController> _logger;
        private readonly InventoryDbContext _context;

        // configs
        private readonly MusicBrainzApiConfig _configMusicBrainzApi;

        // external APIs
        private readonly MusicBrainzApiClient _musicBrainzApiClient;

        public MusicBrainzController(ILogger<MusicBrainzController> logger, InventoryDbContext context,
                        IOptions<MusicBrainzApiConfig> configMusicBrainzApi, MusicBrainzApiClient musicBrainzApiClient)
        {
            _logger = logger;
            _context = context;
            _configMusicBrainzApi = configMusicBrainzApi.Value;
            _musicBrainzApiClient = musicBrainzApiClient;
        }

        public async Task<IActionResult> TestMusicBrainzApi()
        {
            // Release: Thriller by Michael Jackson
            var mbid = "61bf0388-b8a9-48f4-81d1-7eb02706dfb0";

            var data = await _musicBrainzApiClient.TestAsync(mbid);

            return Ok(data);

            //return Ok("TEST");
        }

        public async Task<IActionResult> GetTracksByMbId(string mbid)
        {
            // use: https://localhost:7076/MusicBrainz/GetTracksByMbId?mbid=d07202e3-c6c5-4724-8f4b-842ee44e2184

            if (string.IsNullOrWhiteSpace(mbid))
            {
                return BadRequest("MbId is required.");
            }

            var tracks = await _musicBrainzApiClient.GetTracksByMbIdAsync(mbid);

            if (tracks == null || !tracks.Any())
            {
                return NotFound($"No tracklist found for MbId: {mbid}");
            }

            // Optional: resolve album/artist metadata via separate call
            //var metadata = await _musicBrainzApiClient.GetAlbumMetadataAsync(mbid);

            var viewModel = new TracklistViewModel
            {
                //AlbumTitle = metadata?.AlbumTitle ?? "Unknown Album",
                //ArtistName = metadata?.ArtistName ?? "Unknown Artist",
                MbId = mbid,
                RetrievedAtUtc = DateTime.UtcNow,
                Tracks = tracks.Select(t => new TrackDto
                {
                    Title = t.Title,
                    Duration = t.Duration,
                    Position = t.Position
                }).ToList()
            };

            return PartialView("_TracklistPartial", new List<TracklistViewModel> { viewModel });
        }
    }
}
