using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaxWorx.Shared.Configurations;

namespace WaxWorx.CoverArtApi
{
    // CoverArt Api Documentation
    // https://musicbrainz.org/doc/Cover_Art_Archive/API

    public class CoverArtApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoverArtApiClient> _logger;
        private readonly CoverArtApiConfig _config;
        public CoverArtApiClient(HttpClient httpClient, ILogger<CoverArtApiClient> logger, IOptions<CoverArtApiConfig> configCoverArtApi)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = configCoverArtApi.Value;
        }

        public async Task<string> TestAsync(string mbid)
        {
            // build target endpoint url from settings
            var baseUrl = _config.BaseUrl;

            var endpointUrl = $"{baseUrl}/release/{mbid}/front";

            try
            {
                return await _httpClient.GetStringAsync(endpointUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching test for {mbid}", mbid);
                return null;
            }
        }

        public async Task<string?> GetAlbumCoverUrlAsync(string mbid)
        {
            // build target endpoint url from settings
            var baseUrl = _config.BaseUrl;

            var endpointUrl = $"{baseUrl}/release/{mbid}/front";

            try
            {
                var response = await _httpClient.GetAsync(endpointUrl);
                return response.IsSuccessStatusCode ? endpointUrl : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Album Cover for {mbid}", mbid);
                return null;
            }
        }
    }
}
