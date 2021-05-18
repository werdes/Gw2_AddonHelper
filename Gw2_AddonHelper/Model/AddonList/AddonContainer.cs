using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Model.AddonList
{
    public class AddonContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private InstallState _installState;
        private Addon _addon;
        private string _installationEntrypointFile;


        /// <summary>
        /// 
        /// </summary>
        public AddonContainer(Addon addon)
        {
            _addon = addon;
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
            }
        }
    }
}
