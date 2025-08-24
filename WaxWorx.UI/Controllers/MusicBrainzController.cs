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
    }
}
