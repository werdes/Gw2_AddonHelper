using Gw2_AddonHelper.Model.AddonList;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Model.GameState
{
    public class AddonInstallation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public AddonInstallation(Addon addon)
        {
            _addon = addon;
        }

        private string _installationDirectory;
        [JsonProperty("installation_directory")]
        public string InstallationDirectory
        {
            get => _installationDirectory;
            set
            {
                _installationDirectory = value;
                Notify();
            }
        }

        private Addon _addon;
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

        private InstallState _installState;

        public InstallState InstallState
        {
            get => _installState;
            set
            {
                _installState = value;
                Notify();
            }
        }


    }
}
