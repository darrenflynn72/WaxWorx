using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace WaxWorx.UI.ViewModels
{
    public class AlbumViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public string MbId { get; set; }
        public string ReleaseYear { get; set; }
        public int ArtistId { get; set; } // Foreign key for Artist
        public int GenreId { get; set; } // Foreign key for Genre
        public IFormFile? ImageFile { get; set; } // For file upload
        [BindNever] // Prevent this property from being included in model binding
        public string? CoverUrl { get; set; } // For displaying the image 
        public int? NoOfDiscs { get; set; }
        public DateTime? DatePurchased { get; set; }
        public string? Color { get; set; }
        public bool? PictureDisc { get; set; }
        public bool? LimitedEdition { get; set; }
        public string? LimitedEditioNo { get; set; }
        public bool? Boxset { get; set; }
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Artist is required")]
        public ArtistViewModel Artist { get; set; } // Nested ViewModel

        [Required(ErrorMessage = "Genre is required")]
        public GenreViewModel Genre { get; set; } // Nested ViewModel
    }
}
