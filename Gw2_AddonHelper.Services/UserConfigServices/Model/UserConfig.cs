using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Services;
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

namespace Gw2_AddonHelper.Services.UserConfigServices.Model
{
    public class UserConfig : PropertyLoader<UserConfig>, IPropertyLoader, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private Uri _gameLocation;
        private CultureInfo _lang;
        private ObservableCollection<UiFlag> _uiFlags;
        private ObservableCollection<string> _versionSkipFlags;


        public UserConfig()
        {
            IConfiguration config = Lib.ServiceProvider.GetService<IConfiguration>();

            _gameLocation = new Uri(config.GetValue<string>("defaultValues:gamePath"));
            _lang = CultureInfo.GetCultureInfo(config.GetValue<string>("defaultValues:language"));
            _uiFlags = new ObservableCollection<UiFlag>();
            _versionSkipFlags = new ObservableCollection<string>();
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

        [JsonProperty("ui_flags", ItemConverterType = typeof(StringEnumConverter))]
        public ObservableCollection<UiFlag> UiFlags
        {
            get => _uiFlags;
            set
            {
                _uiFlags = value;
                Notify();
            }
        }

        [JsonProperty("version_skip_flags")]
        public ObservableCollection<string> VersionSkipFlags
        {
            get => _versionSkipFlags;
            set
            {
                _versionSkipFlags = value;
                Notify();
            }
        }
    }
}
