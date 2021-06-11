using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.AddonLib.Model.UserConfig;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Gw2_AddonHelper.Model.UI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private ObservableCollection<AddonContainer> _addonContainers;
        private ObservableCollection<AddonInstallAction> _addonInstallActions;
        private UiState _uiState;
        private string _errorMessageText;
        private string _errorTitleText;
        private UserConfig _userConfig;
        private ObservableCollection<CultureInfo> _availableCultures;
        private ObservableCollection<AddonConflict> _addonConflicts;
        private ObservableCollection<AddonInstallProgress> _addonInstallProgresses;


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


        public UiState UiState
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
            }
        }

        public bool ShowLoading => _uiState == UiState.Loading;
        public bool ShowAddonList => _uiState == UiState.AddonList;
        public bool ShowError => _uiState == UiState.Error;
        public bool ShowInstaller => _uiState == UiState.Installer;
        public bool ShowInstallerProgress => _uiState == UiState.InstallerProgress;
        public bool ShowSettings => _uiState == UiState.Settings;
        public bool ShowConflicts => _uiState == UiState.Conflicts;


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

    }
}
