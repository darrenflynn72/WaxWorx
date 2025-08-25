using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace WaxWorx.Data.Entities
{
    public class Image : AuditBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public string CoverUrl { get; set; } // Name of the file
        public byte[]? Data { get; set; } // Image as byte array
        public string ContentType { get; set; } // MIME type (e.g., "image/jpeg")
        public ICollection<Album>? Albums { get; set; } // Navigation property
    }
}
