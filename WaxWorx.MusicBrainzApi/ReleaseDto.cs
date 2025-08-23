using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

using System.Text.Json.Serialization;

namespace WaxWorx.MusicBrainzApi
{
    public class ReleaseDetailDto
    {
        [JsonPropertyName("country")]
        public string Country { get; set; } = default!;

        [JsonPropertyName("label-info")]
        public List<LabelInfoDto> LabelInfo { get; set; } = new();

        [JsonPropertyName("release-group")]
        public ReleaseGroupDto ReleaseGroup { get; set; } = new();
    }

    public class LabelInfoDto
    {
        [JsonPropertyName("catalog-number")]
        public string CatalogNumber { get; set; } = default!;

        [JsonPropertyName("label")]
        public LabelDto Label { get; set; } = new();
    }

    public class LabelDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("sort-name")]
        public string SortName { get; set; } = default!;

        [JsonPropertyName("disambiguation")]
        public string? Disambiguation { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("type-id")]
        public string? TypeId { get; set; }

        [JsonPropertyName("label-code")]
        public int LabelCode { get; set; }

        [JsonPropertyName("genres")]
        public List<GenreDto> Genres { get; set; } = new();
    }

    public class ReleaseGroupDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("title")]
        public string Title { get; set; } = default!;

        [JsonPropertyName("primary-type")]
        public string PrimaryType { get; set; } = default!;

        [JsonPropertyName("primary-type-id")]
        public string? PrimaryTypeId { get; set; }

        [JsonPropertyName("first-release-date")]
        public string? FirstReleaseDate { get; set; }

        [JsonPropertyName("disambiguation")]
        public string? Disambiguation { get; set; }

        [JsonPropertyName("secondary-type-ids")]
        public List<string> SecondaryTypeIds { get; set; } = new();

        [JsonPropertyName("genres")]
        public List<GenreDto> Genres { get; set; } = new();

        [JsonPropertyName("artist-credit")]
        public List<ArtistCreditDto> ArtistCredit { get; set; } = new();
    }

    public class GenreDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("disambiguation")]
        public string? Disambiguation { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class ArtistCreditDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("artist")]
        public ArtistDto Artist { get; set; } = new();
    }

    public class ArtistDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }
}

