using WaxWorx.MusicBrainzApi;

namespace WaxWorx.UI.ViewModels
{
    public class TracklistViewModel
    {
        public string AlbumTitle { get; set; }
        public string ArtistName { get; set; }
        public string MbId { get; set; }
        public DateTime RetrievedAtUtc { get; set; }
        public List<TrackDto> Tracks { get; set; }
    }
}
