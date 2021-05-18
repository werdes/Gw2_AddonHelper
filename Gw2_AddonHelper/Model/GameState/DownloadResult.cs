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
    public class DownloadResult : INotifyPropertyChanged
    {
        private string _fileName;
        private byte[] _fileContent;
        private string _version;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [JsonProperty("file_name")]
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                Notify();
            }
        }

        [JsonIgnore]
        public byte[] FileContent
        {
            get => _fileContent;
            set
            {
                _fileContent = value;
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
    }
}
