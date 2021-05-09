using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Model.UserConfig
{
    public class UserConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public UserConfig()
        {
            GameLocation = new Uri(@"C:\Program Files\Guild Wars 2\gw2-64.exe");
            Language = "en-EN";
            LastGithubCheck = DateTime.MinValue;
        }

        private Uri _gameLocation;
        [JsonProperty("game_location")]
        public Uri GameLocation { 
            get => _gameLocation;
            set
            {
                _gameLocation = value;
                Notify();
            }
        }

        private string _lang;
        [JsonProperty("language")]
        public string Language
        {
            get => _lang;
            set
            {
                _lang = value;
                Notify();
            }
        }

        private DateTime _lastGithubCheck;
        [JsonProperty("last_github_check")]
        public DateTime LastGithubCheck
        {
            get => _lastGithubCheck;
            set
            {
                _lastGithubCheck = value;
                Notify();
            }
        }
    }
}
