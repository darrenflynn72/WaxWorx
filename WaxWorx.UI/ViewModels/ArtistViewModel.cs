namespace WaxWorx.UI.ViewModels
{
    public class ArtistViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<AlbumViewModel>? Albums { get; set; } // Navigation property
    }
}
