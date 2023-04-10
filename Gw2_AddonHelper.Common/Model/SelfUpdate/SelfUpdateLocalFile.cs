using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Common.Model.SelfUpdate
{
    public class SelfUpdateLocalFile : INotifyPropertyChanged
    {
        private Uri _uri;
        private string _version;
        private DateTime _timeStamp;
        private string _notes;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [JsonProperty("uri")]
        [JsonPropertyName("uri")]
        public Uri Uri
        {
            get => _uri; 
            set
            {
                _uri = value;
                Notify();
            }
        }

        [JsonProperty("version")]
        [JsonPropertyName("version")]
        public string Version
        {
            get => _version; 
            set
            {
                _version = value;
                Notify();
            }
        }

        [JsonProperty("timestamp")]
        [JsonPropertyName("timestamp")]
        public DateTime TimeStamp
        {
            get => _timeStamp; 
            set
            {
                _timeStamp = value;
                Notify();
            }
        }

        [JsonProperty("notes")]
        [JsonPropertyName("notes")]
        public string Notes
        {
            get => _notes; 
            set
            {
                _notes = value;
                Notify();
            }
        }
    }
}
