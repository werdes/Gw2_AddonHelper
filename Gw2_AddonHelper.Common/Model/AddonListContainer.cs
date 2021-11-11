using Gw2_AddonHelper.Common.Model.AddonList;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Common.Model
{
    public class AddonListContainer
    {
        public AddonListContainer()
        {
            Addons = new List<Addon>();
            CrawlTime = DateTime.MinValue;
        }

        [JsonProperty("addons")]
        public List<Addon> Addons { get; set; }

        [JsonProperty("time")]
        public DateTime CrawlTime { get; set; }

        [JsonProperty("repository_version")]
        public string RepositoryVersion { get; set; }
    }
}
