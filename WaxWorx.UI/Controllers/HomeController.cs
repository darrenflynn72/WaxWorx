using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WaxWorx.CoverArtApi;
using WaxWorx.Data;
using WaxWorx.Data.Entities;
using WaxWorx.Models;
using WaxWorx.MusicBrainzApi;
using WaxWorx.Shared.Configurations;
using WaxWorx.UI.ViewModels;

namespace WaxWorx.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InventoryDbContext _context;

        // configs
        private readonly CoverArtApiConfig _configCoverArtApi;
        private readonly MusicBrainzApiConfig _configMusicBrainzApi;

        // external APIs
        private readonly CoverArtApiClient _coverArtApiClient;
        private readonly MusicBrainzApiClient _musicBrainzApiClient;

        public HomeController(ILogger<HomeController> logger, InventoryDbContext context,
                                IOptions<CoverArtApiConfig> configCoverArtApi, IOptions<MusicBrainzApiConfig> configMusicBrainzApi,
                                CoverArtApiClient coverArtApiClient, MusicBrainzApiClient musicBrainzApiClient)
        {
            _logger = logger;
            _context = context;
            _configCoverArtApi = configCoverArtApi.Value;
            _configMusicBrainzApi = configMusicBrainzApi.Value;
            _coverArtApiClient = coverArtApiClient;
            _musicBrainzApiClient = musicBrainzApiClient;
        }

        // Main page with options
        public IActionResult Index()
        {
            // Get Summery Counts
            var tempView = new DashboardViewModel
            {
                TotalRecords = _context.Albums.Count(),
                TotalArtists = _context.Artists.Count(),
                TotalGenres = _context.Genres.Count(),
                ConditionScore = CalculateConditionScore(), // or set a static value
                RecentAlbums = DummyRecentAlbums() // Safe default
                //RecentAlbums = GetRecentAlbums()
            };

            return View(tempView);
        }

        public IActionResult Template()
        {
            // Template

            var data = new TemplateViewModel();

            return View(data);
        }

        private List<AlbumSummary> DummyRecentAlbums()
        {
            return new List<AlbumSummary>
            {
                new AlbumSummary
                {
                    MbId = "",
                    Title = "Lateralus",
                    Artist = "Tool",
                    ReleaseYear = "2001",
                    CoverUrl = "https://upload.wikimedia.org/wikipedia/en/6/63/Tool_-_Lateralus.jpg"
                },
                new AlbumSummary
                {
                    MbId = "",
                    Title = "Keep Me Fed",
                    Artist = "The Warning",
                    ReleaseYear = "2024",
                    CoverUrl = "https://upload.wikimedia.org/wikipedia/en/c/cb/The_Warning_-_Keep_Me_Fed.png"
                },
               new AlbumSummary
                {
                   MbId = "",
                    Title = "The Dark Side of the Moon",
                    Artist = "Pink Floyd",
                    ReleaseYear = "1973",
                    CoverUrl = "https://upload.wikimedia.org/wikipedia/en/3/3b/Dark_Side_of_the_Moon.png"
                },
                new AlbumSummary
                {
                    MbId = "",
                    Title = "Abbey Road",
                    Artist = "The Beatles",
                    ReleaseYear     = "1969",
                    CoverUrl = "https://upload.wikimedia.org/wikipedia/en/4/42/Beatles_-_Abbey_Road.jpg"
                },
                  new AlbumSummary
                {
                    MbId = "",
                    Title = "Back in Black",
                    Artist = "AC/DC",
                    ReleaseYear = "1980",
                    CoverUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/ACDC_Back_in_Black.png/1024px-ACDC_Back_in_Black.png"
                }
            };
        }

        private List<AlbumSummary> GetRecentAlbums()
        {
            int topRecentAlbumCount = 5;

            return _context.Albums
                .Include(a => a.Artist) // if you need Artist.Name
                .OrderByDescending(a => a.CreatedDate)
                .Take(topRecentAlbumCount)
                .Select(a => new AlbumSummary
                {
                    MbId = a.MbId,
                    Title = a.Title,
                    Artist = a.Artist.Name,
                    ReleaseYear = a.ReleaseYear, // or a.ReleaseYear if that’s your field
                    CoverUrl = a.Image.CoverUrl
                })
                .ToList();
        }

        [HttpGet]
        public IActionResult GetGenreStats()
        {
            var genreCounts = _context.Albums
                .Include(a => a.Genre)
                .Where(a => a.Genre != null)
                .GroupBy(a => a.Genre.Name)
                .Select(g => new
                {
                    Genre = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            var result = Json(genreCounts);
            return result;
        }


        //public async Task<IActionResult> ViewConditions()
        //{
        //    // View Conditions

        //    var data = new List<ConditionViewModel>();

        //    return View(data);
        //}


        public async Task<IActionResult> Test()
        {
            return Ok("TEST");
        }

        // Artist Endpoints

        public IActionResult ViewAlbums()
        {
            // Fetch and order by Artist Name and then Album Title
            var albumQuery = _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Genre)
                .OrderBy(a => a.Artist.Name) // Order by Artist Name first
                .ThenBy(a => a.Title);       // Then order by Album Title

            // Map to AlbumViewModel AFTER ordering
            var albumViewModels = albumQuery
                .Select(a => new AlbumViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    MbId = a.MbId,
                    Artist = a.Artist != null ? new ArtistViewModel
                    {
                        Id = a.Artist.Id,
                        Name = a.Artist.Name,
                        MbId = a.MbId
                    } : null,
                    Genre = a.Genre != null ? new GenreViewModel
                    {
                        Id = a.Genre.Id,
                        Name = a.Genre.Name
                    } : null,
                    NoOfDiscs = a.NoOfDiscs,
                    Color = a.Color,
                    PictureDisc = a.PictureDisc,
                    LimitedEdition = a.LimitedEdition,
                    LimitedEditioNo = a.LimitedEditioNo,
                    Boxset = a.Boxset,
                    Notes = a.Notes
                })
                .ToList();

            // Return the mapped View Models to the view
            return View(albumViewModels);
        }

        // Add new album page
        public IActionResult AddAlbum()
        {
            // Setting ViewData["Title"]
            ViewData["Title"] = "Add Album";

            var artists = _context.Artists
                .OrderBy(a => a.Name) // Order by Artist Name
                .Select(a => new ArtistViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    MbId = a.MbId
                })
                .ToList();
            // Store the lookup data in ViewBag
            ViewBag.ArtistLookup = artists;

            var genres = _context.Genres
                .OrderBy(a => a.Name) // Order by Genre Name
                .Select(a => new GenreViewModel
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .ToList();
            // Store the lookup data in ViewBag
            ViewBag.GenreLookup = genres;

            // create the new model for the view
            var model = new AlbumViewModel
            {
                Artist = new ArtistViewModel(),
                Genre = new GenreViewModel()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddAlbum(AlbumViewModel albumViewModel)
        {
            if (ModelState.IsValid) // Validate the form input
            {
                WaxWorx.Data.Entities.Image image = null;

                // Check if an image file is uploaded
                if (albumViewModel.ImageFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        albumViewModel.ImageFile.CopyTo(ms);
                        image = new WaxWorx.Data.Entities.Image
                        {
                            CoverUrl = albumViewModel.ImageFile.FileName,
                            Data = ms.ToArray(),
                            ContentType = albumViewModel.ImageFile.ContentType
                        };
                        _context.Images.Add(image);
                    }
                }

                // ADD NEW ARTIST IF CREATED DURING ADD ALBUM

                // ADD NEW GENRE IF CREATED DURING ADD ALBUM

                // Map AlbumViewModel to Album DbContext model
                var album = new Album
                {
                    Title = albumViewModel.Title,
                    MbId = albumViewModel.MbId,
                    ArtistId = albumViewModel.Artist.Id, // Use the nested ArtistViewModel's Id
                    GenreId = albumViewModel.Genre.Id,  // Use the nested GenreViewModel's Id
                    Image = image, // Associate the uploaded image
                    NoOfDiscs = albumViewModel.NoOfDiscs,
                    Color = albumViewModel.Color,
                    PictureDisc = albumViewModel.PictureDisc,
                    LimitedEdition = albumViewModel.LimitedEdition,
                    LimitedEditioNo = albumViewModel.LimitedEditioNo,
                    Boxset = albumViewModel.Boxset,
                    Notes = albumViewModel.Notes,
                    // Set audit fields
                    CreatedBy = "MyName",
                    CreatedDate = DateTime.Now,
                    IsActive = true // Set default status to active
                };

                // Save the album to the database
                _context.Albums.Add(album); // Add the mapped album object
                _context.SaveChanges(); // Commit changes

                return RedirectToAction("ViewAlbums"); // Redirect to the albums list page
            }

            // If validation fails, return the ViewModel to the form
            return View(albumViewModel);
        }

        public IActionResult EditAlbum(int id)
        {
            // Fetch the album details by ID from the database, including related entities
            var album = _context.Albums
                .Include(a => a.Artist) // Include the Artist details
                .Include(a => a.Genre) // Include the Genre details
                .Include(a => a.Image) // Include the Image for URL generation
                .FirstOrDefault(a => a.Id == id);

            if (album == null) // Handle the case where the album is not found
            {
                ViewBag.ErrorMessage = "The album you are trying to edit does not exist.";
                return View("AlbumError"); // Redirect to a custom error view
            }

            // ADD NEW ARTIST IF CREATED DURING EDIT ALBUM

            // ADD NEW GENRE IF CREATED DURING EDIT ALBUM

            // Map the Album DbContext model to the AlbumViewModel
            var albumViewModel = new AlbumViewModel
            {
                Id = album.Id,
                Title = album.Title,
                MbId = album.MbId,
                Artist = album.Artist != null ? new ArtistViewModel
                {
                    Id = album.Artist.Id,
                    Name = album.Artist.Name,
                    MbId = album.Artist.MbId,
                } : null, // Map ArtistViewModel or set to null if Artist is not found

                Genre = album.Genre != null ? new GenreViewModel
                {
                    Id = album.Genre.Id,
                    Name = album.Genre.Name
                } : null, // Map GenreViewModel or set to null if Genre is not found

                CoverUrl = album.Image != null
                    ? Url.Action("GetImage", "Home", new { id = album.Image.Id })
                    : null, // Generate ImageUrl or set to null if no image exists

                NoOfDiscs = album.NoOfDiscs,
                Color = album.Color,
                PictureDisc = album.PictureDisc,
                LimitedEdition = album.LimitedEdition,
                LimitedEditioNo = album.LimitedEditioNo,
                Boxset = album.Boxset,
                Notes = album.Notes,

                // Manually set ArtistId and GenreId from nested ViewModels
                ArtistId = album.Artist?.Id ?? 0,
                GenreId = album.Genre?.Id ?? 0
            };

            // Setting ViewData["Title"] for the view
            ViewData["Title"] = "Edit Album";

            // Return the AlbumViewModel to the view
            return View("EditAlbum", albumViewModel);
        }

        [HttpPost]
        public IActionResult EditAlbum(AlbumViewModel albumViewModel)
        {
            if (ModelState.IsValid) // Validate the form input
            {
                // Find the existing Album in the database by its ID
                var existingAlbum = _context.Albums
                    .Include(a => a.Artist) // Ensure Artist is included
                    .Include(a => a.Genre) // Ensure Genre is included
                    .Include(a => a.Image) // Include Image for potential updates
                    .FirstOrDefault(a => a.Id == albumViewModel.Id);

                if (existingAlbum == null) // Handle case where album is not found
                {
                    ViewBag.ErrorMessage = "The album you are trying to edit does not exist.";
                    return View("CustomError"); // Redirect to a custom error view
                }

                // Map properties from AlbumViewModel to the existing Album DbContext model
                existingAlbum.Title = albumViewModel.Title;
                existingAlbum.MbId = albumViewModel.MbId;
                existingAlbum.ArtistId = albumViewModel.Artist?.Id ?? existingAlbum.ArtistId; // Handle null ArtistViewModel
                existingAlbum.GenreId = albumViewModel.Genre?.Id ?? existingAlbum.GenreId;   // Handle null GenreViewModel
                existingAlbum.NoOfDiscs = albumViewModel.NoOfDiscs;
                existingAlbum.Color = albumViewModel.Color;
                existingAlbum.PictureDisc = albumViewModel.PictureDisc;
                existingAlbum.LimitedEdition = albumViewModel.LimitedEdition;
                existingAlbum.LimitedEditioNo = albumViewModel.LimitedEditioNo;
                existingAlbum.Boxset = albumViewModel.Boxset;
                existingAlbum.Notes = albumViewModel.Notes;

                // Check if a new image file is uploaded
                if (albumViewModel.ImageFile != null)
                {
                    // Remove the old image if it exists
                    if (existingAlbum.Image != null)
                    {
                        _context.Images.Remove(existingAlbum.Image);
                    }

                    using (var ms = new MemoryStream())
                    {
                        albumViewModel.ImageFile.CopyTo(ms);
                        var newImage = new WaxWorx.Data.Entities.Image
                        {
                            CoverUrl = albumViewModel.ImageFile.FileName,
                            Data = ms.ToArray(),
                            ContentType = albumViewModel.ImageFile.ContentType
                        };
                        _context.Images.Add(newImage);
                        existingAlbum.Image = newImage; // Associate the new image
                    }
                }

                // Update audit fields
                existingAlbum.ModifiedBy = "MyName"; // Replace with actual user info
                existingAlbum.ModifiedDate = DateTime.Now;

                // Save changes to the database
                _context.SaveChanges();

                // Redirect to the album list page
                return RedirectToAction("ViewAlbums");
            }

            // If validation fails, redisplay the form with the submitted data
            return View(albumViewModel);
        }

        public IActionResult DeleteAlbum(int id) //DeleteAlbumConfirmed(int id)
        {
            var album = _context.Albums.FirstOrDefault(a => a.Id == id);

            if (album == null)
            {
                //return NotFound(); // Return 404 if album not found

                // Pass an error message to the NotFound view
                ViewBag.ErrorMessage = "The album you are trying to delete does not exist.";
                return View("AlbumError"); // Return a custom NotFound view
            }

            _context.Albums.Remove(album); // Remove the album from DbSet
            _context.SaveChanges(); // Persist changes to the database

            return RedirectToAction("ViewAlbums"); // Redirect to the album list page
        }

        // Artist Endpoints
        public IActionResult ViewArtists()
        {
            var artistViewModels = _context.Artists
                .OrderBy(a => a.Name) // Order by Artist Name
                .Select(a => new ArtistViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    MbId = a.MbId
                })
                .ToList();

            return View(artistViewModels);
        }

        [HttpGet]
        public IActionResult AddArtist()
        {
            // Setting ViewData["Title"] for the view
            ViewData["Title"] = "Add Artist";

            // create the new model for the view
            var model = new ArtistViewModel
            {
                //Name = "",
                //MbId = ""
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddArtist(ArtistViewModel artistViewModel)
        {
            if (ModelState.IsValid)
            {
                var artist = new Artist
                {
                    Name = artistViewModel.Name,
                    MbId = artistViewModel.MbId,
                    CreatedBy = "MyName", // Replace with actual user name
                    CreatedDate = DateTime.Now,
                    ModifiedBy = "MyName", // Replace with actual user name
                    ModifiedDate = DateTime.Now
                };

                _context.Artists.Add(artist);
                _context.SaveChanges();

                return RedirectToAction("ViewArtists");
            }

            return View(artistViewModel);
        }

        [HttpGet]
        public IActionResult EditArtist(int id)
        {
            var artist = _context.Artists.FirstOrDefault(a => a.Id == id);

            if (artist == null)
            {
                ViewBag.ErrorMessage = "The artist you are trying to edit does not exist.";
                return View("ArtistError");
            }

            var artistViewModel = new ArtistViewModel
            {
                Id = artist.Id,
                Name = artist.Name
            };

            return View(artistViewModel);
        }

        [HttpPost]
        public IActionResult EditArtist(ArtistViewModel artistViewModel)
        {
            if (ModelState.IsValid)
            {
                var artist = _context.Artists.FirstOrDefault(a => a.Id == artistViewModel.Id);

                if (artist == null)
                {
                    ViewBag.ErrorMessage = "The artist you are trying to edit does not exist.";
                    return View("ArtistError");
                }

                artist.Name = artistViewModel.Name;
                artist.MbId = artistViewModel.MbId;
                artist.ModifiedBy = "MyName"; // Replace with actual user name
                artist.ModifiedDate = DateTime.Now;

                _context.SaveChanges();

                return RedirectToAction("ViewArtists");
            }

            return View(artistViewModel);
        }

        // Genre Endpoints
        public IActionResult ViewGenres()
        {
            var genreViewModels = _context.Genres
                .OrderBy(g => g.Name) // Order by Genre Name
                .Select(g => new GenreViewModel
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToList();

            return View(genreViewModels);
        }

        [HttpGet]
        public IActionResult AddGenre()
        {
            // Setting ViewData["Title"] for the view
            ViewData["Title"] = "Add Genre";

            // create the new model for the view
            var model = new GenreViewModel
            {
                //Name = "",
                //MbId = ""
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddGenre(GenreViewModel genreViewModel)
        {
            if (ModelState.IsValid)
            {
                var genre = new Genre
                {
                    Name = genreViewModel.Name,
                    CreatedBy = "MyName", // Replace with actual user name
                    CreatedDate = DateTime.Now,
                    ModifiedBy = "MyName", // Replace with actual user name
                    ModifiedDate = DateTime.Now
                };

                _context.Genres.Add(genre);
                _context.SaveChanges();

                return RedirectToAction("ViewGenres");
            }

            return View(genreViewModel);
        }

        [HttpGet]
        public IActionResult EditGenre(int id)
        {
            var genre = _context.Genres.FirstOrDefault(g => g.Id == id);

            if (genre == null)
            {
                ViewBag.ErrorMessage = "The genre you are trying to edit does not exist.";
                return View("GenreError");
            }

            var genreViewModel = new GenreViewModel
            {
                Id = genre.Id,
                Name = genre.Name
            };

            return View(genreViewModel);
        }

        [HttpPost]
        public IActionResult EditGenre(GenreViewModel genreViewModel)
        {
            if (ModelState.IsValid)
            {
                var genre = _context.Genres.FirstOrDefault(g => g.Id == genreViewModel.Id);

                if (genre == null)
                {
                    ViewBag.ErrorMessage = "The genre you are trying to edit does not exist.";
                    return View("GenreError");
                }

                genre.Name = genreViewModel.Name;
                genre.ModifiedBy = "MyName"; // Replace with actual user name
                genre.ModifiedDate = DateTime.Now;

                _context.SaveChanges();

                return RedirectToAction("ViewGenres");
            }

            return View(genreViewModel);
        }

        public IActionResult GetImage(int id)
        {
            var image = _context.Images.FirstOrDefault(i => i.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            return File(image.Data, image.ContentType);
        }

        public int CalculateConditionScore()
        {
            try
            {
                var gradeMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["M"] = 100,
                    ["NM"] = 90,
                    ["VG+"] = 80,
                    ["VG"] = 70,
                    ["G+"] = 60,
                    ["G"] = 50,
                    ["F"] = 30,
                    ["P"] = 10
                };

                var conditions = _context.Albums
                    .Select(a => a.Condition)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                if (conditions.Count == 0) return 0;

                var totalScore = conditions.Sum(c =>
                {
                    var albumCode = c.Split('/')[0].Trim();
                    return gradeMap.GetValueOrDefault(albumCode, 0);
                });

                return (int)Math.Round((double)totalScore / conditions.Count, MidpointRounding.AwayFromZero);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public IActionResult ViewCollectionCondition()
        {
            try
            {
                var totalAlbums = _context.Albums.Count();

                var gradeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["M"] = "Mint",
                    ["Mint"] = "Mint",
                    ["NM"] = "Near Mint",
                    ["Near Mint"] = "Near Mint",
                    ["VG+"] = "Very Good+",
                    ["VG"] = "Very Good",
                    ["G"] = "Good",
                    ["F"] = "Fair",
                    ["P"] = "Poor"
                };

                var knownGrades = new List<string>
        {
            "Mint", "Near Mint", "Very Good+", "Very Good", "Good", "Fair", "Poor", "U"
        };

                var canonicalCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Mint"] = "M",
                    ["Near Mint"] = "NM",
                    ["Very Good+"] = "VG+",
                    ["Very Good"] = "VG",
                    ["Good"] = "G",
                    ["Fair"] = "F",
                    ["Poor"] = "P",
                    ["U"] = "U"
                };

                var displayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Mint"] = "Mint",
                    ["Near Mint"] = "Near Mint",
                    ["Very Good+"] = "Very Good+",
                    ["Very Good"] = "Very Good",
                    ["Good"] = "Good",
                    ["Fair"] = "Fair",
                    ["Poor"] = "Poor",
                    ["U"] = "Unknown"
                };

                var rawConditions = _context.Albums
                    .Select(a => new { a.Condition })
                    .AsEnumerable()
                    .Select(a =>
                    {
                        var rawCode = string.IsNullOrWhiteSpace(a.Condition)
                            ? "U"
                            : a.Condition.Split('/')[0].Trim();

                        var normalized = gradeMap.TryGetValue(rawCode, out var mapped)
                            ? mapped
                            : "U";

                        return new { RawCode = rawCode, Normalized = normalized };
                    })
                    .GroupBy(x => x.Normalized)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            Count = g.Count(),
                            RawCodes = g.Select(x => x.RawCode).Distinct().ToList()
                        });

                var viewModel = knownGrades
                    .Select(grade => new ConditionViewModel
                    {
                        ConditionName = displayNames.ContainsKey(grade) ? displayNames[grade] : grade,
                        ConditionGroup = canonicalCodeMap.ContainsKey(grade)
                            ? canonicalCodeMap[grade]
                            : "U",
                        AlbumCount = rawConditions.ContainsKey(grade) ? rawConditions[grade].Count : 0,
                        PercentageOfTotal = totalAlbums == 0
                            ? 0
                            : Math.Round((double)(rawConditions.ContainsKey(grade) ? rawConditions[grade].Count : 0) / totalAlbums * 100, 2)
                    })
                    .OrderByDescending(vm => vm.AlbumCount)
                    .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate condition breakdown");

                return View(new List<ConditionViewModel>
        {
            new ConditionViewModel
            {
                ConditionGroup = "ERR",
                ConditionName = "Error",
                AlbumCount = 0,
                PercentageOfTotal = 0
            }
        });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
