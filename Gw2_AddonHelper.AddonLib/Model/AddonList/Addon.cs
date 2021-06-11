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

namespace Gw2_AddonHelper.AddonLib.Model.AddonList
{
    public class Addon : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Addon()
        {
            _additionalFlags = new ObservableCollection<AddonFlag>();
            _requiredAddons = new ObservableCollection<string>();
            _conflicts = new ObservableCollection<string>();
        }

        private string _addonId;
        [JsonProperty("addon_id")]
        [JsonPropertyName("addon_id")]
        public string AddonId
        {
            get { return _addonId; }
            set { _addonId = value; }
        }

        private string _developer;
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

        private Uri _website;
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

        private string _addonName;
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

        private string _description;
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

        private string _tooltip;
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

        private HostType _hostType;
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
            }
        }

        private Uri _hostUrl;
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

        private Uri _versionUrl;
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
            }
        }

        private DownloadType _downloadType;
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

        private InstallMode _installMode;
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

        private string _pluginName;
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

        private ObservableCollection<string> _requiredAddons;
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

        private ObservableCollection<string> _conflicts;
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

        private ObservableCollection<AddonFlag> _additionalFlags;
        [YamlMember(Alias = "additional_flags")]
        [JsonProperty("additional_flags", ItemConverterType = typeof(StringEnumConverter))]
        [JsonPropertyName("additional_flags")]
        public ObservableCollection<AddonFlag> AdditionalFlags
        {
            get => _additionalFlags; set
            {
                _additionalFlags = value ?? new ObservableCollection<AddonFlag>(); ;
                Notify();
            }
        }

        private VersioningType _versioningType;
        [JsonProperty("versioning_type")]
        [JsonPropertyName("versioning_type")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public VersioningType VersioningType
        {
            get => _versioningType;
            set
            {
                _versioningType = value;
                Notify();
            }
        }

        private string _loaderKey;
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
            if(obj is Addon)
            {
                return ((Addon)obj).AddonId == AddonId;
            }
            return false;
        }
    }
}
