using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model.SelfUpdate
{
    public class UpdatePath
    {
        [JsonProperty("zipPath")]
        [JsonPropertyName("zipPath")]
        public string ZipPath { get; set; }

        [JsonProperty("fileSystemPath")]
        [JsonPropertyName("fileSystemPath")]
        public string FileSystemPath { get; set; }

    }
}
