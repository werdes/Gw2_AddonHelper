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
            FileHashes = new Dictionary<string, Dictionary<string, string>>();
            CrawlTime = DateTime.MinValue;
        }

        [JsonProperty("versions")]
        public Dictionary<string, string> Versions { get; set; }

        [JsonProperty("file_hashes")]
        public Dictionary<string, Dictionary<string, string>> FileHashes { get; set; }

        [JsonProperty("time")]
        public DateTime CrawlTime { get; set; }
    }
}
