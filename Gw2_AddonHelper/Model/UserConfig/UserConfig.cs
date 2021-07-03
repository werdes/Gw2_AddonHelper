using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.Model.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private Version _latestVersion;
        private ObservableCollection<Enums.UiFlag> _uiFlags;

        public UserConfig()
        {
            IConfiguration config = App.ServiceProvider.GetService<IConfiguration>();

            _gameLocation = new Uri(config.GetValue<string>("defaultValues:gamePath"));
            _lang = CultureInfo.GetCultureInfo(config.GetValue<string>("defaultValues:language"));
            _lastGithubCheck = DateTime.MinValue;
            _latestVersion = new Version();
            _uiFlags = new ObservableCollection<Enums.UiFlag>();
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

        [JsonProperty("latest_version")]
        public Version LatestVersion
        {
            get => _latestVersion;
            set
            {
                _latestVersion = value;
                Notify();
            }
        }

        [JsonProperty("ui_flags", ItemConverterType = typeof(StringEnumConverter))]
        public ObservableCollection<Enums.UiFlag> UiFlags
        {
            get => _uiFlags;
            set
            {
                _uiFlags = value;
                Notify();
            }
        }
    }
}
