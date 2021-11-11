using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Gw2_AddonHelper.Common.Model.AddonList
{
    public class Addon : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _loaderKey;
        private ObservableCollection<AddonFlag> _additionalFlags;
        private ObservableCollection<string> _conflicts;
        private ObservableCollection<string> _requiredAddons;
        private string _pluginName;
        private InstallMode _installMode;
        private DownloadType _downloadType;
        private Uri _versionUrl;
        private Uri _hostUrl;
        private HostType _hostType;
        private string _tooltip;
        private string _description;
        private string _addonName;
        private Uri _website;
        private string _developer;
        private string _addonId;

        public Addon()
        {
            _additionalFlags = new ObservableCollection<AddonFlag>();
            _requiredAddons = new ObservableCollection<string>();
            _conflicts = new ObservableCollection<string>();
        }

        [JsonProperty("addon_id")]
        [JsonPropertyName("addon_id")]
        public string AddonId
        {
            get { return _addonId; }
            set { _addonId = value; }
        }

        [YamlMember(Alias = "developer")]
        [JsonProperty("developer")]
        [JsonPropertyName("developer")]
        public string Developer
        {
            get => _developer;
            set
            {
                _developer = value;
                Notify();
            }
        }

        [YamlMember(Alias = "website")]
        [JsonProperty("website")]
        [JsonPropertyName("website")]
        public Uri Website
        {
            get => _website;
            set
            {
                _website = value;
                Notify();
            }
        }

        [YamlMember(Alias = "addon_name")]
        [JsonProperty("addon_name")]
        [JsonPropertyName("addon_name")]
        public string AddonName
        {
            get => _addonName;
            set
            {
                _addonName = value;
                Notify();
            }
        }

        [YamlMember(Alias = "description")]
        [JsonProperty("description")]
        [JsonPropertyName("description")]
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                Notify();
            }
        }

        [YamlMember(Alias = "tooltip")]
        [JsonProperty("tooltip")]
        [JsonPropertyName("tooltip")]
        public string Tooltip
        {
            get => _tooltip;
            set
            {
                _tooltip = value;
                Notify();
            }
        }

        [YamlMember(Alias = "host_type")]
        [JsonProperty("host_type")]
        [JsonPropertyName("host_type")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public HostType HostType
        {
            get => _hostType;
            set
            {
                _hostType = value;
                Notify();
                Notify(nameof(VersioningType));
            }
        }

        [YamlMember(Alias = "host_url")]
        [JsonProperty("host_url")]
        [JsonPropertyName("host_url")]
        public Uri HostUrl
        {
            get => _hostUrl;
            set
            {
                _hostUrl = value;
                Notify();
            }
        }

        [YamlMember(Alias = "version_url")]
        [JsonProperty("version_url")]
        [JsonPropertyName("version_url")]
        public Uri VersionUrl
        {
            get => _versionUrl;
            set
            {
                _versionUrl = value;
                Notify();
                Notify(nameof(VersioningType));
            }
        }

        [YamlMember(Alias = "download_type")]
        [JsonProperty("download_type")]
        [JsonPropertyName("download_type")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public DownloadType DownloadType
        {
            get => _downloadType;
            set
            {
                _downloadType = value;
                Notify();
            }
        }

        [YamlMember(Alias = "install_mode")]
        [JsonProperty("install_mode")]
        [JsonPropertyName("install_mode")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public InstallMode InstallMode
        {
            get => _installMode;
            set
            {
                _installMode = value;
                Notify();
            }
        }

        [YamlMember(Alias = "plugin_name")]
        [JsonProperty("plugin_name")]
        [JsonPropertyName("plugin_name")]
        public string PluginName
        {
            get => _pluginName;
            set
            {
                _pluginName = value;
                Notify();
            }
        }

        [YamlMember(Alias = "requires")]
        [JsonProperty("required_addons")]
        [JsonPropertyName("required_addons")]
        public ObservableCollection<string> RequiredAddons
        {
            get => _requiredAddons;
            set
            {
                _requiredAddons = value ?? new ObservableCollection<string>();
                Notify();
            }
        }

        [YamlMember(Alias = "conflicts")]
        [JsonProperty("conflicts")]
        [JsonPropertyName("conflicts")]
        public ObservableCollection<string> Conflicts
        {
            get => _conflicts;
            set
            {
                _conflicts = value ?? new ObservableCollection<string>();
                Notify();
            }
        }

        [YamlMember(Alias = "additional_flags")]
        [JsonProperty("additional_flags", ItemConverterType = typeof(StringEnumConverter))]
        [JsonPropertyName("additional_flags")]
        public ObservableCollection<AddonFlag> AdditionalFlags
        {
            get => _additionalFlags; set
            {
                _additionalFlags = value ?? new ObservableCollection<AddonFlag>(); ;
                Notify();
                Notify(nameof(VersioningType));
            }
        }

        [JsonProperty("versioning_type")]
        [JsonPropertyName("versioning_type")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public VersioningType VersioningType
        {
            get
            {
                if (HostType == HostType.Github) return VersioningType.GithubCommitSha;
                if (HostType == HostType.Standalone && VersionUrl != null) return VersioningType.HostFileMd5;
                if (HostType == HostType.Standalone && AdditionalFlags.Contains(AddonFlag.SelfUpdating)) return VersioningType.SelfUpdating;
                return VersioningType.Unknown;
            }
        }

        [JsonProperty("loader_key")]
        [JsonPropertyName("loader_key")]
        public string LoaderKey
        {
            get => _loaderKey;
            set
            {
                _loaderKey = value;
                Notify();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Addon)
            {
                return ((Addon)obj).AddonId == AddonId;
            }
            return false;
        }
    }
}
