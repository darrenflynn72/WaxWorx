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
                //ConditionScore = CalculateConditionScore(), // or set a static value
                RecentAlbums = new List<AlbumSummary>() // Safe default
            };

            return View(tempView); 
        }

        public async Task<IActionResult> Test()
        {
            return Ok("TEST");
        }


        //public IActionResult GetAlbumCount()
        //{
        //    var count = _context.Albums.Count();

        //    return Content(count.ToString());
        //}

        //public IActionResult GetArtistCount()
        //{
        //    var count = _context.Albums.Count();

        //    return Content(count.ToString());
        //}

        //public IActionResult GetGenreCount()
        //{
        //    var count = _context.Albums.Count();

        //    return Content(count.ToString());
        //}

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
                    Artist = a.Artist != null ? new ArtistViewModel
                    {
                        Id = a.Artist.Id,
                        Name = a.Artist.Name
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
                    Name = a.Name
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
                            FileName = albumViewModel.ImageFile.FileName,
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
                Artist = album.Artist != null ? new ArtistViewModel
                {
                    Id = album.Artist.Id,
                    Name = album.Artist.Name
                } : null, // Map ArtistViewModel or set to null if Artist is not found

                Genre = album.Genre != null ? new GenreViewModel
                {
                    Id = album.Genre.Id,
                    Name = album.Genre.Name
                } : null, // Map GenreViewModel or set to null if Genre is not found

                ImageUrl = album.Image != null
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
                            FileName = albumViewModel.ImageFile.FileName,
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
                    Name = a.Name
                })
                .ToList();

            return View(artistViewModels);
        }

        [HttpGet]
        public IActionResult AddArtist()
        {
            // Setting ViewData["Title"] for the view
            ViewData["Title"] = "Add Artist";

            return View(new ArtistViewModel());
        }

        [HttpPost]
        public IActionResult AddArtist(ArtistViewModel artistViewModel)
        {
            if (ModelState.IsValid)
            {
                var artist = new Artist
                {
                    Name = artistViewModel.Name,
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

            return View(new GenreViewModel());
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
