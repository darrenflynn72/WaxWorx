using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaxWorx.Shared.Configurations
{
    public class MusicBrainzApiConfig
    {
        public string ApiKey { get; set; }
        public string AccessToken { get; set; }
        public string BaseUrl { get; set; }
        public string CallbackUrl { get; set; }
    }
}
