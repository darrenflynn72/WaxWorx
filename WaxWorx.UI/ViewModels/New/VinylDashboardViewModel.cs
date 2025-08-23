namespace WaxWorx.UI.ViewModels
{
    public class VinylDashboardViewModel
    {
        public int TotalRecords { get; set; }
        public int TotalArtists { get; set; }
        public int TotalGenres { get; set; }
        public int ConditionScore { get; set; } // e.g. average condition %
        public List<string> GenreLabels { get; set; }
        public List<int> GenreCounts { get; set; }
        public List<AlbumSummary> RecentAlbums { get; set; }
    }

    public class AlbumSummary
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public int Year { get; set; }
        public string CoverUrl { get; set; }
    }
}
