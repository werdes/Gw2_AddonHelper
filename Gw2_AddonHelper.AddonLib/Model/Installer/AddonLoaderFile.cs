using Gw2_AddonHelper.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model.Installer
{
    public class AddonLoaderFile
    {
        [JsonProperty("key")]
        public AddonLoaderFileKey Key { get; set; }

        [JsonProperty("directory")]
        public string Directory { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }
    }
}
