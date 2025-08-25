using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaxWorx.MusicBrainzApi
{
    public class TrackDto
    {
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public int Position { get; set; }
    }
}
