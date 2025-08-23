using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaxWorx.Shared.Configurations;

namespace WaxWorx.MusicBrainzApi
{
    // MusicBrainz Api Documentation
    // https://musicbrainz.org/doc/MusicBrainz_API

    public class MusicBrainzApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MusicBrainzApiClient> _logger;
        private readonly MusicBrainzApiConfig _config;
        public MusicBrainzApiClient(HttpClient httpClient, ILogger<MusicBrainzApiClient> logger, IOptions<MusicBrainzApiConfig> configMusicBrainzApi)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WaxWorx/1.0");
            _logger = logger;
            _config = configMusicBrainzApi.Value;
        }

        public async Task<string> TestAsync(string mbid)
        {
            // build target endpoint url from settings
            var baseUrl = _config.BaseUrl;

            var endpointUrl = $"{baseUrl}/release/{mbid}";

            try
            {
                return await _httpClient.GetStringAsync(endpointUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Test for {mbid}", mbid);
                return null;
            }
        }

        public async Task<string> GetAlbumInfoAsync(string mbid)
        {
            // build target endpoint url from settings
            var baseUrl = _config.BaseUrl;

            var endpointUrl = $"{baseUrl}/release/{mbid}?fmt=json";

            try
            {
                return await _httpClient.GetStringAsync(endpointUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Album Info for {mbid}", mbid);
                return null;
            }
        }

        public async Task<string?> GetMBIDAsync(string albumTitle, string artistName)
        {
            // build target endpoint url from settings
            var baseUrl = _config.BaseUrl;

            //var endpointUrl = $"{baseUrl}/release/{mbid}";

            // Construct the query URL
            var endpointUrl = $"{baseUrl}/release/?query=release:{Uri.EscapeDataString(albumTitle)}%20artist:{Uri.EscapeDataString(artistName)}&fmt=json";

            try
            {
                // Construct the query URL
                //var query = $"https://musicbrainz.org/ws/2/release/?query=release:{Uri.EscapeDataString(albumTitle)}%20artist:{Uri.EscapeDataString(artistName)}&fmt=json";
                
                // Make the API call
                var response = await _httpClient.GetAsync(endpointUrl);

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parse the JSON response to find the MBID
                var json = JObject.Parse(content);
                var release = json["releases"]?.FirstOrDefault();
                var mbid = release?["id"]?.ToString();

                return mbid; // Return the MBID, or null if not found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching MBID for {albumTitle} {artistName}", albumTitle, artistName);
                return null;
            }
        }
    }
}
