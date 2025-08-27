using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WaxWorx.UI.ViewModels
{
    public class AddAlbumViewModel
    {
        public string Title { get; set; }
        public int ReleaseYear { get; set; }
        //public string Artist { get; set; }
        //public string Genre { get; set; }
        //public string Condition { get; set; }


        [Required(ErrorMessage = "Artist is required")]
        public ArtistViewModel Artist { get; set; } // Nested ViewModel

        [Required(ErrorMessage = "Genre is required")]
        public GenreViewModel Genre { get; set; } // Nested ViewModel

        [Required(ErrorMessage = "Condition is required")]
        public ConditionViewModel Condition { get; set; } // Nested ViewModel


        // for dropdowns
        public IEnumerable<SelectListItem> ArtistOptions { get; set; } = new List<SelectListItem>(); //TODO: populate
        public IEnumerable<SelectListItem> GenreOptions { get; set; } = new List<SelectListItem>(); //TODO: populate
        public IEnumerable<SelectListItem> ConditionOptions { get; set; } = new List<SelectListItem>(); //TODO: populate
    }
}
