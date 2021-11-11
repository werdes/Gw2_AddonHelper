using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model.GameState
{
    public class AddonContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private InstallState _installState;
        private Addon _addon;
        private string _installationEntrypointFile;
        private bool _quickUpdateAvailable;
        private bool _checked;


        public AddonContainer(Addon addon)
        {
            _addon = addon;
            _checked = true;
        }

        [JsonProperty("addon")]
        public Addon Addon
        {
            get => _addon;
            set
            {
                _addon = value;
                Notify();
            }
        }

        [JsonProperty("installation_entrypoint_file")]
        public string InstallationEntrypointFile
        {
            get => _installationEntrypointFile;
            set
            {
                _installationEntrypointFile = value;
                Notify();
            }
        }

        [JsonProperty("install_state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InstallState InstallState
        {
            get => _installState;
            set
            {
                _installState = value;
                Notify();
                Notify(nameof(UiShowInstall));
                Notify(nameof(UiShowRemove));
                Notify(nameof(UiShowDisable));
                Notify(nameof(UiShowEnable));
            }
        }

        [JsonProperty("quick_update_available")]
        public bool QuickUpdateAvailable
        {
            get => _quickUpdateAvailable;
            set
            {
                _quickUpdateAvailable = value;
                Notify();
            }
        }


        public bool UiShowInstall
        {
            get => _installState == InstallState.NotInstalled;
        }
        public bool UiShowRemove
        {
            get => _installState == InstallState.InstalledEnabled ||
                   _installState == InstallState.InstalledDisabled;
        }
        public bool UiShowDisable
        {
            get => _installState == InstallState.InstalledEnabled;
        }
        public bool UiShowEnable
        {
            get => _installState == InstallState.InstalledDisabled;
        }
        public bool UiShowOpen
        {
            get => _installState == InstallState.InstalledEnabled ||
                   _installState == InstallState.InstalledDisabled;
        }

        public bool Checked
        {
            get => _checked;
            set
            {
                _checked = value;
                Notify();
            }
        }

        public int SortKey
        {
            get => QuickUpdateAvailable ? int.MinValue + (int)InstallState : (int)InstallState;
        }
    }
}
