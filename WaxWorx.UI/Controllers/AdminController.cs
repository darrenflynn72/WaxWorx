using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WaxWorx.Controllers;
using WaxWorx.Core;
using WaxWorx.Core.Import;
using WaxWorx.CoverArtApi;
using WaxWorx.Data;
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
        private readonly AdminSettingsConfig _adminSettingsConfig;

        // external APIs
        //private readonly CoverArtApiClient _coverArtApiClient;

        // CSV Importer
        private readonly CsvImporter _importer;

        public AdminController(ILogger<AdminController> logger, InventoryDbContext context,
                                IOptions<CoverArtApiConfig> configCoverArtApi, IOptions<AdminSettingsConfig> adminSettingsConfig,
                                CsvImporter importer)
        {
            _logger = logger;
            _context = context;
            _adminSettingsConfig = adminSettingsConfig.Value;
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

                //return BadRequest("No file selected.");
            }

            using var stream = csvFile.OpenReadStream();
            _importer.Import(stream);

            //TempData["ImportSuccess"] = "Import triggered.";
            TempData["AlertMessage"] = "Import Complete.";

            return RedirectToAction("Settings");
            //return Ok("Import triggered.");
        }
    }
}
