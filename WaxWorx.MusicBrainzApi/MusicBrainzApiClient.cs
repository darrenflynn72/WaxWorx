using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaxWorx.Shared.Configurations;
using System.Text.Json;

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

        public async Task<ReleaseDetailDto> TestAsync(string mbid)
        {
            var baseUrl = _config.BaseUrl.TrimEnd('/');
            //var endpointUrl = $"{baseUrl}/release/{mbid}?inc=artist-credits&fmt=json"; // include artist

            // include artist and full detail
            var endpointUrl = $"{baseUrl}/release/{mbid}?inc=artist-credits+media+recordings+release-groups+labels+genres&fmt=json";

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WaxWorx/1.0.0 ( https://waxworx.com )");

            try
            {
                var response = await _httpClient.GetAsync(endpointUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("MusicBrainz returned {StatusCode} for release {mbid}", response.StatusCode, mbid);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // options to ignore case
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                var result = JsonSerializer.Deserialize<ReleaseDetailDto>(json);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching release for {mbid}", mbid);
                return null;
            }
        }

        public async Task<ReleaseDetailDto> GetAlbumInfoAsync(string mbid)
        {
            var baseUrl = _config.BaseUrl.TrimEnd('/');
            //var endpointUrl = $"{baseUrl}/release/{mbid}?inc=artist-credits&fmt=json"; // include artist

            // include artist and full detail
            var endpointUrl = $"{baseUrl}/release/{mbid}?inc=artist-credits+media+recordings+release-groups+labels+genres&fmt=json";

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WaxWorx/1.0.0 ( https://waxworx.com )");

            try
            {
                var response = await _httpClient.GetAsync(endpointUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("MusicBrainz returned {StatusCode} for release {mbid}", response.StatusCode, mbid);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // options to ignore case
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                var result = JsonSerializer.Deserialize<ReleaseDetailDto>(json);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching release for {mbid}", mbid);
                return null;
            }
        }

        public async Task<string?> GetMBIDAsync(string albumTitle, string artistName)
        {
            var baseUrl = _config.BaseUrl;
            var query = $"release:{albumTitle} AND artist:{artistName} AND status:official";
            var endpointUrl = $"{baseUrl}/release/?query={Uri.EscapeDataString(query)}&fmt=json";

            try
            {
                var response = await _httpClient.GetAsync(endpointUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("MusicBrainz returned {StatusCode} for query {Query}", response.StatusCode, query);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var release = json["releases"]?.FirstOrDefault();
                if (release == null)
                {
                    _logger.LogInformation("No releases found for album '{Album}' by artist '{Artist}'", albumTitle, artistName);
                    return null;
                }

                var mbid = release["id"]?.ToString();
                return mbid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching MBID for album '{Album}' by artist '{Artist}'", albumTitle, artistName);
                return null;
            }
        }
    }
}
