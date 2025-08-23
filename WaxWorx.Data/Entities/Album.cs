using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaxWorx.Data.Entities
{
    public class Album : AuditBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public int ArtistId { get; set; } // Foreign key for Artist
        public int GenreId { get; set; } // Foreign key for Genre
        public int? ImageId { get; set; } // Nullable foreign key
        public int? NoOfDiscs { get; set; }
        public string? Color { get; set; }
        public bool? PictureDisc { get; set; }
        public bool? LimitedEdition { get; set; }
        public string? LimitedEditioNo { get; set; }
        public bool? Boxset { get; set; }
        public string? Notes { get; set; }

        public Artist? Artist { get; set; } // Navigation property for Artist
        public Genre? Genre { get; set; }   // Navigation property for Genre
        public Image? Image { get; set; } // Nullable navigation property
    }
}
