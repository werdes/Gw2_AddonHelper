using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Gw2_AddonHelper.Model.UI;
using System;
using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Services.UserConfigServices.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using Gw2_AddonHelper.Common.Model.SelfUpdate;

namespace Gw2_AddonHelper.Model.UI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private ObservableCollection<AddonContainer> _addonContainers;
        private ObservableCollection<AddonInstallAction> _addonInstallActions;
        private ObservableCollection<CultureInfo> _availableCultures;
        private ObservableCollection<AddonConflict> _addonConflicts;
        private ObservableCollection<AddonInstallProgress> _addonInstallProgresses;
        private UserConfig _userConfig;
        private Version _version;
        private Enums.UiState _uiState;
        private string _errorMessageText;
        private string _errorTitleText;
        private bool _appUpdateAvailable;
        private bool _installerProgressCancelEnabled;
        private AddonListSource _addonListSource;
        private DateTime _addonListCrawlTime;
        private string _addonSourceServiceName;
        private string _filterText;
        private SelfUpdateLocalFile _selfUpdateLocalFile;
        private double _appUpdateDownloadProgress;
        private bool _appUpdateDownloadWaiting;


        public MainWindowViewModel()
        {
            _addonContainers = new ObservableCollection<AddonContainer>();
            _addonInstallActions = new ObservableCollection<AddonInstallAction>();
            _addonConflicts = new ObservableCollection<AddonConflict>();
            _addonInstallProgresses = new ObservableCollection<AddonInstallProgress>();

            _appUpdateDownloadWaiting = true;
            _appUpdateDownloadProgress = 0;
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
            }
        }

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

        public UserConfig UserConfig
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

        public bool InstallerProgressCancelEnabled
        {
            get => _installerProgressCancelEnabled;
            set
            {
                _installerProgressCancelEnabled = value;
                Notify();
            }
        }

        public AddonListSource AddonListSource
        {
            get => _addonListSource;
            set
            {
                _addonListSource = value;
                Notify();
            }
        }

        public string AddonSourceServiceName
        {
            get => _addonSourceServiceName;
            set
            {
                _addonSourceServiceName = value;
                Notify();
            }
        }

        public DateTime AddonVersionsCrawlTime
        {
            get => _addonListCrawlTime;
            set
            {
                _addonListCrawlTime = value;
                Notify();
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                Notify();
            }
        }

        public SelfUpdateLocalFile SelfUpdateLocalFile
        {
            get => _selfUpdateLocalFile; 
            set
            {
                _selfUpdateLocalFile = value;
                Notify();
            }
        }

        public double AppUpdateDownloadProgress
        {
            get => _appUpdateDownloadProgress;
            set
            {
                _appUpdateDownloadProgress = value;
                Notify();
                Notify(nameof(AppUpdateDownloadProgressText));
            }
        }
        public string AppUpdateDownloadProgressText
        {
            get
            {
                if (_appUpdateDownloadWaiting) return "Waiting...";
                return $"{(AppUpdateDownloadProgress * 100):n2} %";
            }
        }

        public bool AppUpdateDownloadWaiting
        {
            get => _appUpdateDownloadWaiting;
            set
            {
                _appUpdateDownloadWaiting = value;
                Notify();
                Notify(nameof(AppUpdateDownloadProgressText));
            }
        }

    }
}
