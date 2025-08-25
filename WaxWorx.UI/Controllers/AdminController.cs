using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WaxWorx.Controllers;
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

        public AdminController(ILogger<AdminController> logger, InventoryDbContext context,
                                IOptions<CoverArtApiConfig> configCoverArtApi, IOptions<AdminSettingsConfig> adminSettingsConfig)
        {
            _logger = logger;
            _context = context;
            _adminSettingsConfig = adminSettingsConfig.Value;
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

        public IActionResult RunImport()
        {
            // Run Import

            return Ok("Run Import");
        }
    }
}
