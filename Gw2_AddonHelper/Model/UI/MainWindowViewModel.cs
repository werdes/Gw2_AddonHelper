using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.Model.UserConfig;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Gw2_AddonHelper.Model.UI;
using System;

namespace Gw2_AddonHelper.Model.UI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private ObservableCollection<AddonContainer> _addonContainers;
        private ObservableCollection<AddonInstallAction> _addonInstallActions;
        private Enums.UiState _uiState;
        private string _errorMessageText;
        private string _errorTitleText;
        private UserConfig.UserConfig _userConfig;
        private ObservableCollection<CultureInfo> _availableCultures;
        private ObservableCollection<AddonConflict> _addonConflicts;
        private ObservableCollection<AddonInstallProgress> _addonInstallProgresses;
        private bool _appUpdateAvailable;
        private Version _version;

        public MainWindowViewModel()
        {
            _addonContainers = new ObservableCollection<AddonContainer>();
            _addonInstallActions = new ObservableCollection<AddonInstallAction>();
            _addonConflicts = new ObservableCollection<AddonConflict>();
            _addonInstallProgresses = new ObservableCollection<AddonInstallProgress>();
        }

        public ObservableCollection<AddonContainer> AddonContainers
        {
            get => _addonContainers;
            set
            {
                _addonContainers = value;
                Notify();
            }
        }

        public ObservableCollection<AddonInstallAction> AddonInstallActions
        {
            get => _addonInstallActions;
            set
            {
                _addonInstallActions = value;
                Notify();
            }
        }
        public ObservableCollection<AddonInstallProgress> AddonInstallProgresses
        {
            get => _addonInstallProgresses;
            set
            {
                _addonInstallProgresses = value;
                Notify();
            }
        }


        public Enums.UiState UiState
        {
            get => _uiState;
            set
            {
                _uiState = value;
                Notify();
                Notify(nameof(ShowLoading));
                Notify(nameof(ShowAddonList));
                Notify(nameof(ShowError));
                Notify(nameof(ShowInstaller));
                Notify(nameof(ShowInstallerProgress));
                Notify(nameof(ShowSettings));
                Notify(nameof(ShowConflicts));
                Notify(nameof(ShowAbout));
            }
        }

        public bool ShowLoading => _uiState == Enums.UiState.Loading;
        public bool ShowAddonList => _uiState == Enums.UiState.AddonList;
        public bool ShowError => _uiState == Enums.UiState.Error;
        public bool ShowInstaller => _uiState == Enums.UiState.Installer;
        public bool ShowInstallerProgress => _uiState == Enums.UiState.InstallerProgress;
        public bool ShowSettings => _uiState == Enums.UiState.Settings;
        public bool ShowConflicts => _uiState == Enums.UiState.Conflicts;
        public bool ShowAbout => _uiState == Enums.UiState.About;



        public string ErrorTitleText
        {
            get => _errorTitleText;
            set
            {
                _errorTitleText = value;
                Notify();
            }
        }

        public string ErrorMessageText
        {
            get => _errorMessageText;
            set
            {
                _errorMessageText = value;
                Notify();
            }
        }

        public UserConfig.UserConfig UserConfig
        {
            get => _userConfig;
            set
            {
                _userConfig = value;
                Notify();
            }
        }

        public ObservableCollection<CultureInfo> AvailableCultures
        {
            get => _availableCultures;
            set
            {
                _availableCultures = value;
                Notify();
            }
        }

        public ObservableCollection<AddonConflict> AddonConflicts
        {
            get => _addonConflicts;
            set
            {
                _addonConflicts = value;
                Notify();
            }
        }

        public bool AppUpdateAvailable
        {
            get => _appUpdateAvailable;
            set
            {
                _appUpdateAvailable = value;
                Notify();
            }
        }
        public Version Version
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
