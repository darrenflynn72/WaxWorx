using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using WaxWorx.Data;
using WaxWorx.Data.Entities;

namespace WaxWorx.Core.Import
{
    public class CsvImporter
    {
        private readonly ILogger<CsvImporter> _logger;
        private readonly InventoryDbContext _context;

        public CsvImporter(ILogger<CsvImporter> logger, InventoryDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Import(Stream csvStream)
        {
            try
            {
                using var reader = new StreamReader(csvStream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim,
                    BadDataFound = null
                });

                var records = csv.GetRecords<VinylImportDto>().ToList();

                var artistCache = new Dictionary<string, Artist>(StringComparer.OrdinalIgnoreCase);
                var genreCache = new Dictionary<string, Genre>(StringComparer.OrdinalIgnoreCase);

                int successCount = 0;
                int failureCount = 0;

                foreach (var record in records)
                {
                    try
                    {
                        // Deduplicate Artist
                        if (!artistCache.TryGetValue(record.Artist, out var artist))
                        {
                            artist = GetOrCreateArtist(record.Artist);
                            artistCache[record.Artist] = artist;
                        }

                        // Deduplicate Genre
                        if (!genreCache.TryGetValue(record.Genre, out var genre))
                        {
                            genre = GetOrCreateGenre(record.Genre);
                            genreCache[record.Genre] = genre;
                        }

                        var newAlbum = new Album
                        {
                            Title = record.Album,
                            ArtistId = artist.Id,
                            GenreId = genre.Id,
                            Color = record.VinylColor,
                            Condition = record.Condition,
                            Country = record.Country,
                            NoOfDiscs = ParseNullableInt(record.DiscCount),
                            DatePurchased = ParseDatePurchased(record.DatePurchased),
                            ReleaseYear = ParseYear(record.DatePurchased),
                            LimitedEditioNo = record.LimitedEditionNumber,
                            CopiesPressed = ParseNullableInt(record.CopiesPressed),
                            Boxset = ParseBool(record.BoxSet),
                            LimitedEdition = !string.IsNullOrWhiteSpace(record.LimitedEditionNumber),
                            PictureDisc = InferPictureDisc(record.VinylColor),
                            Notes = record.Notes,
                            CreatedDate = DateTime.Now,
                            CreatedBy = "admin"
                        };

                        newAlbum.IsActive = true;
                        newAlbum.CreatedDate = DateTime.Now;
                        newAlbum.CreatedBy = "admin";

                        _context.Albums.Add(newAlbum);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex, "Failed to import album record: {@Record}", record);
                    }
                }

                _context.SaveChanges();
                _logger.LogInformation("Import completed. Success: {SuccessCount}, Failures: {FailureCount}", successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Fatal error during CSV import.");
                throw;
            }
        }

        private Artist GetOrCreateArtist(string name)
        {
            var existing = _context.Artists.FirstOrDefault(a => a.Name == name);
            if (existing != null)
            {
                return existing;
            }

            var newArtist = new Artist { Name = name };

            newArtist.IsActive = true;
            newArtist.CreatedDate = DateTime.Now;
            newArtist.CreatedBy = "admin";

            _context.Artists.Add(newArtist);
            _context.SaveChanges();

            return newArtist;
        }

        private Genre GetOrCreateGenre(string name)
        {
            var existing = _context.Genres.FirstOrDefault(g => g.Name == name);
            if (existing != null)
            {
                return existing;
            }

            var newGenre = new Genre { Name = name };

            newGenre.IsActive = true;
            newGenre.CreatedDate = DateTime.Now;
            newGenre.CreatedBy = "admin";

            _context.Genres.Add(newGenre);
            _context.SaveChanges();

            return newGenre;
        }

        private int? ParseNullableInt(string input)
        {
            return int.TryParse(input, out var value) ? value : null;
        }

        private string ParseYear(string date)
        {
            if (DateTime.TryParse(date, out var dt))
            {
                return dt.Year.ToString();
            }

            return null;
        }

        private bool? ParseBool(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            return input.Trim().ToLower() switch
            {
                // true values
                "y" => true,
                "yes" => true,
                "true" => true,
                "1" => true,

                // false values
                "n" => false,
                "no" => false,
                "false" => false,
                "0" => false,
                _ => null
            };
        }

        public static DateTime? ParseDatePurchased(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    return null;

                if (DateTime.TryParseExact(
                    input.Trim(),
                    "M/d/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
                {
                    return parsed;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Optional: log ex.Message or tag the row for review
                return null;
            }
        }

        private bool? InferPictureDisc(string vinylColor)
        {
            if (string.IsNullOrWhiteSpace(vinylColor))
            {
                return null;
            }

            var normalized = vinylColor.ToLower();

            return normalized.Contains("picture") || normalized.Contains("image");
        }
    }
}
