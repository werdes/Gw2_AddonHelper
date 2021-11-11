using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gw2_AddonHelper.AddonLib.Model.GameState
{
    public class ExtractionResult : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _version;
        private string _loaderKey;
        private DateTime _installationTime;
        private List<ExtractionResultFile> _addonFiles;

        public ExtractionResult()
        {
            _addonFiles = new List<ExtractionResultFile>();
            _loaderKey = string.Empty;
        }


        [JsonProperty("addon_files")]
        public List<ExtractionResultFile> AddonFiles
        {
            get => _addonFiles;
            set
            {
                _addonFiles = value;
                Notify();
            }
        }

        [JsonProperty("version")]
        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                Notify();
            }
        }

        [JsonProperty("installation_time")]
        public DateTime InstallationTime
        {
            get => _installationTime;
            set
            {
                _installationTime = value;
                Notify();
            }
        }

        [JsonProperty("loader_key")]
        public string LoaderKey
        {
            get => _loaderKey;
            set
            {
                _loaderKey = value;
                Notify();
            }
        }
    }
}
