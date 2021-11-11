using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Common.Model.AddonList
{
    public class VersionContainer
    {
        public VersionContainer()
        {
            Versions = new Dictionary<string, string>();
            CrawlTime = DateTime.MinValue;
        }

        [JsonProperty("versions")]
        public Dictionary<string, string> Versions { get; set; }

        [JsonProperty("time")]
        public DateTime CrawlTime { get; set; }
    }
}
