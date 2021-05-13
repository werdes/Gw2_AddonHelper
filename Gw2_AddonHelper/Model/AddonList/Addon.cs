using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Gw2_AddonHelper.Model.AddonList
{
    public class Addon : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Addon()
        {
        }

        private string _addonId;
        [JsonProperty("addon_id")]
        public string AddonId
        {
            get { return _addonId; }
            set { _addonId = value; }
        }

        private string _developer;
        [YamlMember(Alias = "developer")]
        [JsonProperty("developer")]
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
        [JsonConverter(typeof(StringEnumConverter))]
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
        [JsonConverter(typeof(StringEnumConverter))]
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
        [JsonConverter(typeof(StringEnumConverter))]
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
        public ObservableCollection<string> RequiredAddons
        {
            get => _requiredAddons;
            set
            {
                _requiredAddons = value;
                Notify();
            }
        }

        private ObservableCollection<string> _conflicts;
        [YamlMember(Alias = "conflicts")]
        [JsonProperty("conflicts")]
        public ObservableCollection<string> Conflicts
        {
            get => _conflicts;
            set
            {
                _conflicts = value;
                Notify();
            }
        }

        private ObservableCollection<string> _additionalFlags;
        [YamlMember(Alias = "additional_flags")]
        [JsonProperty("additional_flags")]
        public ObservableCollection<string> AdditionalFlags
        {
            get => _additionalFlags; set
            {
                _additionalFlags = value;
                Notify();
            }
        }
    }
}
