using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaxWorx.Data.Entities
{
    public class Track : AuditBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int AlbumId { get; set; } // FK to Album

        public string Name { get; set; }

        public int? TrackNumber { get; set; } // MusicBrainz Position

        public int? Duration { get; set; }

        public string? MbId { get; set; }

        public Album Album { get; set; } // Navigation: each track belongs to one album
    }
}
