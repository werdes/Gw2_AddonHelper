using Gw2_AddonHelper.AddonLib.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Model.UserConfig
{
    public class UserConfig : PropertyLoader<UserConfig>, IPropertyLoader, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private Uri _gameLocation;
        private CultureInfo _lang;
        private DateTime _lastGithubCheck;
        private DateTime _lastSelfUpdateCheck;

        public UserConfig()
        {
            IConfiguration config = App.ServiceProvider.GetService<IConfiguration>();

            _gameLocation = new Uri(config.GetValue<string>("defaultValues:gamePath"));
            _lang = CultureInfo.GetCultureInfo(config.GetValue<string>("defaultValues:language"));
            _lastGithubCheck = DateTime.MinValue;
        }

        [JsonProperty("game_location")]
        public Uri GameLocation
        {
            get => _gameLocation;
            set
            {
                _gameLocation = value;
                Notify();
            }
        }

        [JsonProperty("language")]
        public CultureInfo Language
        {
            get => _lang;
            set
            {
                _lang = value;
                Notify();
                LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(_lang));
            }
        }

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

        [JsonProperty("last_self_update_check")]
        public DateTime LastSelfUpdateCheck
        {
            get => _lastSelfUpdateCheck;
            set
            {
                _lastSelfUpdateCheck = value;
                Notify();
            }
        }
    }
}
