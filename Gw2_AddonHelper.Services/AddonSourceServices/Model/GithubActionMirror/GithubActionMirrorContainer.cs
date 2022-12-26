using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AddonSourceServices.Model.GithubActionMirror
{
    public class GithubActionMirrorContainer
    {
        public GithubActionMirrorContainer()
        {
            Addons = new Dictionary<string, GithubActionMirrorAddon>();
        }

        [JsonProperty("addons")]
        public Dictionary<string, GithubActionMirrorAddon> Addons { get; set; }

        [JsonProperty("loader")]
        public GithubActionMirrorAddon Loader { get; set; }
    }
}
