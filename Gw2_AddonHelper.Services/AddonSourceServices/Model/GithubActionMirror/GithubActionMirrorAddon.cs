using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AddonSourceServices.Model.GithubActionMirror
{
    public class GithubActionMirrorAddon
    {
        public GithubActionMirrorAddon()
        {

        }

        public Addon GetCommonAddon()
        {
            Addon addon = new Addon();
            addon.AddonId = Nickname;
            addon.AddonName = AddonName;
            addon.Description = Description;
            addon.Developer = Developer;
            addon.Tooltip = Tooltip;

            addon.InstallMode = InstallMode;
            addon.HostType = HostType;
            addon.DownloadType = DownloadType;

            addon.RequiredAddons.AddRangeIfNotNull(Requires);
            addon.Conflicts.AddRangeIfNotNull(Conflicts);

            if (SelfUpdate)
            {
                addon.AdditionalFlags.Add(AddonFlag.SelfUpdating);
            }
            if(!string.IsNullOrEmpty(PluginName))
            {
                addon.PluginName = PluginName;
                if(!addon.PluginName.EndsWith(".dll"))
                {
                    addon.PluginName += ".dll";
                }
            }
            if (!string.IsNullOrEmpty(PluginNamePattern))
            {
                addon.AdditionalFlags.Add(AddonFlag.ObscuredFilename);
            }
            if (!string.IsNullOrEmpty(VersionUrl))
            {
                addon.VersionUrl = new Uri(VersionUrl);
            }
            if (!string.IsNullOrEmpty(Website))
            {
                addon.Website = new Uri(Website);
            }
            if (!string.IsNullOrEmpty(HostUrl))
            {
                addon.HostUrl = new Uri(HostUrl);
            }

            return addon;
        }

        [JsonProperty("developer")]
        public string Developer { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("addon_name")]
        public string AddonName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("tooltip")]
        public string Tooltip { get; set; }

        [JsonProperty("host_type")]
        public HostType HostType { get; set; }

        [JsonProperty("host_url")]
        public string HostUrl { get; set; }

        [JsonProperty("version_url")]
        public string VersionUrl { get; set; }

        [JsonProperty("download_type")]
        public DownloadType DownloadType { get; set; }

        [JsonProperty("install_mode")]
        public InstallMode InstallMode { get; set; }

        [JsonProperty("plugin_name_pattern")]
        public string PluginNamePattern { get; set; }

        [JsonProperty("self_update")]
        public bool SelfUpdate { get; set; }

        [JsonProperty("requires")]
        public string[] Requires { get; set; }

        [JsonProperty("conflicts")]
        public string[] Conflicts { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("plugin_name")]
        public string PluginName { get; set; }

        [JsonProperty("fetch_time")]
        public long FetchTime { get; set; }

        [JsonProperty("version_id")]
        public string VersionId { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }
    }
}
