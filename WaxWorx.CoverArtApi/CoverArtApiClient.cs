using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaxWorx.Shared.Configurations;
using System.Net;

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

        public async Task<byte[]> GetFrontCoverArtAsync(string mbid)
        {
            // build target endpoint url from settings
            var baseUrl = _config.BaseUrl;

            var endpointUrl = $"{baseUrl}/release/{mbid}/front";

            try
            {              
                var response = await _httpClient.GetAsync(endpointUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsByteArrayAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching test for {mbid}", mbid);
                return null;
            }
        }

        public async Task<string?> GetFrontCoverArtUrlAsync(string mbid)
        {
            // build target endpoint url from settings
            var baseUrl = _config.BaseUrl;

            var endpointUrl = $"{baseUrl}/release/{mbid}/front";

            try
            {
                // Clone handler for this call so you don’t break other endpoints
                var handler = new HttpClientHandler { AllowAutoRedirect = false };
                var client = new HttpClient(handler);
                
                var response = await client.GetAsync(endpointUrl);

                if (response.StatusCode == HttpStatusCode.MovedPermanently || 
                        response.StatusCode == HttpStatusCode.Found || 
                        response.StatusCode == HttpStatusCode.SeeOther || 
                        response.StatusCode == HttpStatusCode.TemporaryRedirect || 
                        (int)response.StatusCode == 308) // Permanent Redirect
                {
                    var result = response.Headers.Location?.ToString();
                    return result;
                }

                response.EnsureSuccessStatusCode();
                return null;  // no redirect header found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching test for {mbid}", mbid);
                return null;
            }
        }

        public async Task<string> TestAsync(string mbid)
        {
            // build target endpoint url from settings
            var baseUrl = _config.BaseUrl;

            var endpointUrl = $"{baseUrl}/release/{mbid}/front";

            try
            {
                var data = await _httpClient.GetStringAsync(endpointUrl);

                return data;
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
