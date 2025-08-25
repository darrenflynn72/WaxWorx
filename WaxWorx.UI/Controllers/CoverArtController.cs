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
    public class CoverArtController : Controller
    {
        private readonly ILogger<CoverArtController> _logger;
        private readonly InventoryDbContext _context;

        // configs
        private readonly CoverArtApiConfig _configCoverArtApi;

        // external APIs
        private readonly CoverArtApiClient _coverArtApiClient;

        public CoverArtController(ILogger<CoverArtController> logger, InventoryDbContext context,
                        IOptions<CoverArtApiConfig> configCoverArtApi, CoverArtApiClient coverArtApiClient)
        {
            _logger = logger;
            _context = context;
            _configCoverArtApi = configCoverArtApi.Value;
             _coverArtApiClient = coverArtApiClient;
        }


        // Main page with options
        public IActionResult Index()
        {
            var tempView = new DashboardViewModel
            {
                RecentAlbums = new List<AlbumSummary>() // Safe default
            };

            return View(tempView);
        }

        public async Task<IActionResult> TestCoverArtApi()
        {
            // Release: Thriller by Michael Jackson
            var mbid = "61bf0388-b8a9-48f4-81d1-7eb02706dfb0";

            //var data = await _coverArtApiClient.TestAsync(mbid);

            // Get CDN Image path
            var data = await _coverArtApiClient.GetFrontCoverArtUrlAsync(mbid);

            // Get Image byte array
            //var data = await _coverArtApiClient.GetFrontCoverArtAsync(mbid);


            return Ok("TEST");
        }      
    }
}
