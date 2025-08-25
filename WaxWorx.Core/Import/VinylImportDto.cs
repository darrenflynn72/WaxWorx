using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaxWorx.Core.Import
{
    public class VinylImportDto
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public string VinylColor { get; set; }
        public string Condition { get; set; }
        public string Country { get; set; }
        public string DiscCount { get; set; }
        public string DatePurchased { get; set; }
        public string LimitedEditionNumber { get; set; }
        public string CopiesPressed { get; set; }
        public string BoxSet { get; set; }
        public string Notes { get; set; }
    }
}
