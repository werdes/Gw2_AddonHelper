using Gw2_AddonHelper.AddonLib.Extensions;
using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.AddonList;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.Model.UserConfig;
using Gw2_AddonHelper.Extensions;
using Gw2_AddonHelper.Model.UI;
using Gw2_AddonHelper.Services.Interfaces;
using Gw2_AddonHelper.UI.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Gw2_AddonHelper.AddonLib.Utility.Github;

namespace Gw2_AddonHelper.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel _viewModel = null;
        private IConfiguration _config;
        private IUserConfigService _userConfigService;
        private ILogger<MainWindow> _log;

        private bool _initializationFinished = false;

        public MainWindow(IConfiguration config, ILogger<MainWindow> log, IAddonListService addonListManager, IUserConfigService userConfigService)
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();
            _viewModel.UiState = Enums.UiState.Loading;
            _viewModel.UserConfig = userConfigService.GetConfig();
            _viewModel.UserConfig.LanguageChanged += OnUserConfigLanguageChanged;
            _viewModel.Version = Assembly.GetExecutingAssembly().GetName().Version;

            _log = log;
            _config = config;
            _userConfigService = userConfigService;

            //Tell the localization provider to refresh the object data provider
            LocalizationProvider.ChangeCulture(_viewModel.UserConfig.Language);
            DataContext = _viewModel;
        }

        /// <summary>
        /// Window loaded event
        ///  > Initialize UI
        ///  > Check Game Path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.AvailableCultures = new ObservableCollection<CultureInfo>(await LoadAvailableLanguages());
                _viewModel.AppUpdateAvailable = await CheckAppUpdateAvailable();

                if (_userConfigService.GetConfig().UiFlags.Contains(Enums.UiFlag.WelcomeScreenDismissed))
                {
                    await InitializeUi();
                }
                else
                {
                    _viewModel.UiState = Enums.UiState.Welcome;
                }

                _initializationFinished = true;
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, nameof(OnWindowLoaded));
                SetUiError(ex, Localization.Localization.UiErrorWarning);
            }
        }

        /// <summary>
        /// Checks, if an update for this application is available
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckAppUpdateAvailable()
        {
            IAppUpdaterService appUpdaterService = App.ServiceProvider.GetService<IAppUpdaterService>();
            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            DateTime lastCheck = _userConfigService.GetConfig().LastSelfUpdateCheck;
            DateTime checkThreshold = DateTime.UtcNow.Add(-1 * _config.GetValue<TimeSpan>("selfUpdate:refreshCooldown"));

            _log.LogInformation($"Checking for app updates with last_check [{lastCheck}], checkThreshold [{checkThreshold}]");

            //Check Refresh cooldown, Ratelimit 
            if (lastCheck <= checkThreshold && GithubRatelimitService.Instance.CanCall())
            {
                Version latestVersion = await appUpdaterService.GetLatestVersion();
                _log.LogInformation($"Latest available version is [{latestVersion}]");

                if (latestVersion != null)
                {
                    _userConfigService.GetConfig().LastSelfUpdateCheck = DateTime.UtcNow;
                    _userConfigService.GetConfig().LatestVersion = latestVersion;
                    _userConfigService.Store();

                    return latestVersion != currentVersion;
                }
            }

            Version storedVersion = _userConfigService.GetConfig().LatestVersion;
            _log.LogInformation($"Checking against stored version [{storedVersion}]");
            return storedVersion.Build != 0 && storedVersion.Revision != 0 && storedVersion != currentVersion;
        }

        /// <summary>
        /// Checks if the game is selected correctly
        /// </summary>
        /// <returns></returns>
        private bool CheckGamePath()
        {
            Uri gamePath = _userConfigService.GetConfig().GameLocation;
            return new FileInfo(gamePath.LocalPath).Exists;
        }

        /// <summary>
        /// Returns the available Languages from the resource file
        /// </summary>
        /// <returns></returns>
        private async Task<List<CultureInfo>> LoadAvailableLanguages()
        {
            List<CultureInfo> availableCultures = new List<CultureInfo>();
            bool showInvariantCulture = _config.GetValue<bool>("showInvariantCulture");

            await Task.Run(() =>
            {
                availableCultures = LocalizationProvider.GetAvailableCultures(showInvariantCulture);
            });

            return availableCultures;
        }

        /// <summary>
        /// Builds the UI
        ///  > Also refreshes the addon list
        /// </summary>
        /// <returns></returns>
        private async Task InitializeUi()
        {
            try
            {
                _viewModel.UiState = Enums.UiState.Loading;

                if (CheckGamePath())
                {
                    IAddonListService addonListService = App.ServiceProvider.GetService<IAddonListService>();
                    IAddonGameStateService addonGameStateService = App.ServiceProvider.GetService<IAddonGameStateService>();

                    List<Addon> lstAddons = await addonListService.GetAddonsAsync();
                    addonListService.Store();

                    List<AddonContainer> containers = (await addonGameStateService.GetAddonContainers(lstAddons)).OrderByDescending(x => x.SortKey).ToList();
                    _viewModel.AddonContainers = new ObservableCollection<AddonContainer>(containers);
                    _viewModel.UiState = Enums.UiState.AddonList;
                }
                else
                {
                    SetUiError(new FileNotFoundException(_userConfigService.GetConfig().GameLocation.LocalPath), Localization.Localization.GameNotFoundError);
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.UiErrorWarning);
                _log.LogCritical(ex, $"Initializing UI");
            }
        }

        /// <summary>
        /// Sets the UI "Error" Panel to visible
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="errorTitle"></param>
        private void SetUiError(Exception ex, string errorTitle)
        {
            _viewModel.UiState = Enums.UiState.Error;
            _viewModel.ErrorMessageText = ex.Message;
            _viewModel.ErrorTitleText = errorTitle;
        }

        /// <summary>
        /// Resets the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonRetryClick(object sender, RoutedEventArgs e)
        {
            await InitializeUi();
        }

        /// <summary>
        /// Show UserConfig settings page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonSettingsClick(object sender, RoutedEventArgs e)
        {
            _viewModel.UiState = Enums.UiState.Settings;
        }

        /// <summary>
        /// Show about page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonAboutClick(object sender, RoutedEventArgs e)
        {
            _viewModel.UiState = Enums.UiState.About;
        }


        /// <summary>
        /// Saves the Userconfig and continues with UI Init
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonWelcomeContinueClick(object sender, RoutedEventArgs e)
        {
            try
            {
                IUserConfigService userConfigService = App.ServiceProvider.GetService<IUserConfigService>();
                userConfigService.GetConfig().UiFlags.Add(Enums.UiFlag.WelcomeScreenDismissed);
                userConfigService.Store();

                await InitializeUi();
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.SettingsError);
                _log.LogCritical(ex, nameof(OnButtonSettingsSaveClick));
            }
        }


        /// <summary>
        /// Save Userconfig
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonSettingsSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {

                IUserConfigService userConfigService = App.ServiceProvider.GetService<IUserConfigService>();
                userConfigService.Store();

                await InitializeUi();
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.SettingsError);
                _log.LogCritical(ex, nameof(OnButtonSettingsSaveClick));
            }
        }

        /// <summary>
        /// Cancels the Settings dialog and resets the userconfig
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonSettingsCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                IUserConfigService userConfigService = App.ServiceProvider.GetService<IUserConfigService>();
                userConfigService.Load();

                await InitializeUi();
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.SettingsError);
                _log.LogCritical(ex, nameof(OnButtonSettingsCancelClick));
            }
        }

        /// <summary>
        /// Opens the addon directory if possible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddonListItemOpen(object sender, AddonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(e.AddonContainer.InstallationEntrypointFile))
                {
                    string directory = Path.GetDirectoryName(e.AddonContainer.InstallationEntrypointFile);
                    if (Directory.Exists(directory))
                    {
                        Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", directory);
                    }
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.OpenAddonError + e.AddonContainer?.Addon?.AddonName);
                _log.LogCritical(ex, $"Opening directory for [{e.AddonContainer?.Addon?.AddonId}]");
            }
        }

        /// <summary>
        /// Refresh data provider on language change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserConfigLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            if (e.Culture != null)
            {
                LocalizationProvider.ChangeCulture(e.Culture);
            }
        }

        /// <summary>
        /// Button click handler for installing addons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAddonListItemInstall(object sender, AddonEventArgs e)
        {
            try
            {
                _log.LogDebug($"Installing [{e.AddonContainer.Addon.AddonId}]");
                IAddonGameStateService addonGameStateService = App.ServiceProvider.GetService<IAddonGameStateService>();

                HashSet<AddonContainer> requiredAddons = new HashSet<AddonContainer>();
                await addonGameStateService.GetParentAddons(_viewModel.AddonContainers.ToList(), e.AddonContainer, requiredAddons);

                HashSet<AddonContainer> installableAddons = requiredAddons.Where(x => x.InstallState != InstallState.InstalledEnabled && x.InstallState != InstallState.InstalledDisabled).ToHashSet();
                installableAddons.Add(e.AddonContainer);

                HashSet<AddonContainer> enableableAddons = requiredAddons.Where(x => x.InstallState == InstallState.InstalledDisabled).ToHashSet();


                List<AddonContainer> installedAddons = installableAddons.ToList();
                installedAddons.AddRange(enableableAddons);


                IEnumerable<AddonConflict> addonConflicts = await addonGameStateService.CheckConflicts(installedAddons, installableAddons);
                if (addonConflicts.Count() == 0)
                {
                    _log.LogInformation($"Required addons for [{e.AddonContainer.Addon.AddonId}]:");
                    _log.LogObject(installableAddons);

                    _viewModel.AddonInstallActions.Clear();
                    _viewModel.AddonInstallActions.AddRange(installableAddons.Select(x => new AddonInstallAction(x, InstallerActionType.Install)));
                    _viewModel.AddonInstallActions.AddRange(enableableAddons.Select(x => new AddonInstallAction(x, InstallerActionType.Enable)));

                    _viewModel.UiState = Enums.UiState.Installer;
                }
                else
                {
                    _viewModel.AddonConflicts.Clear();
                    _viewModel.AddonConflicts.AddRange(addonConflicts);
                    _viewModel.UiState = Enums.UiState.Conflicts;
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError + e.AddonContainer?.Addon?.AddonName);
                _log.LogCritical(ex, $"Installing [{e.AddonContainer?.Addon?.AddonId}]");
            }
        }

        /// <summary>
        /// Click handler for removing a given addon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAddonListItemRemove(object sender, AddonEventArgs e)
        {
            try
            {
                _log.LogDebug($"Removing [{e.AddonContainer.Addon.AddonId}]");
                IAddonGameStateService addonGameStateService = App.ServiceProvider.GetService<IAddonGameStateService>();

                HashSet<AddonContainer> childAddons = new HashSet<AddonContainer>();
                await addonGameStateService.GetChildAddons(_viewModel.AddonContainers.ToList(), e.AddonContainer, childAddons);

                HashSet<AddonContainer> removableAddons = childAddons.Where(x => x.InstallState == InstallState.InstalledEnabled || x.InstallState == InstallState.InstalledDisabled).ToHashSet();
                removableAddons.Add(e.AddonContainer);

                _log.LogInformation($"Removable addons for [{e.AddonContainer.Addon.AddonId}]:");
                _log.LogObject(removableAddons);

                _viewModel.AddonInstallActions.Clear();
                _viewModel.AddonInstallActions.AddRange(removableAddons.Select(x => new AddonInstallAction(x, InstallerActionType.Remove)));

                _viewModel.UiState = Enums.UiState.Installer;

            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError + e.AddonContainer?.Addon?.AddonName);
                _log.LogCritical(ex, $"Removing [{e.AddonContainer?.Addon?.AddonId}]");
            }
        }

        /// <summary>
        /// Click handler for disabling a given addon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAddonListItemDisable(object sender, AddonEventArgs e)
        {
            try
            {
                _log.LogDebug($"Disabling [{e.AddonContainer.Addon.AddonId}]");
                IAddonGameStateService addonGameStateService = App.ServiceProvider.GetService<IAddonGameStateService>();

                HashSet<AddonContainer> childAddons = new HashSet<AddonContainer>();
                await addonGameStateService.GetChildAddons(_viewModel.AddonContainers.ToList(), e.AddonContainer, childAddons);

                HashSet<AddonContainer> disableableAddons = childAddons.Where(x => x.InstallState == InstallState.InstalledEnabled).ToHashSet();
                disableableAddons.Add(e.AddonContainer);
                HashSet<AddonContainer> installedAddons = _viewModel.AddonContainers.Where(x => x.InstallState == InstallState.InstalledEnabled).ToHashSet();

                _log.LogInformation($"Disableable addons for [{e.AddonContainer.Addon.AddonId}]:");
                _log.LogObject(disableableAddons);

                _viewModel.AddonInstallActions.Clear();
                _viewModel.AddonInstallActions.AddRange(disableableAddons.Select(x => new AddonInstallAction(x, InstallerActionType.Disable)));

                _viewModel.UiState = Enums.UiState.Installer;
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError + e.AddonContainer?.Addon?.AddonName);
                _log.LogCritical(ex, $"Disabling [{e.AddonContainer?.Addon?.AddonId}]");
            }
        }

        /// <summary>
        /// Click handler for enabling a given addon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAddonListItemEnable(object sender, AddonEventArgs e)
        {
            try
            {
                _log.LogDebug($"Enabling [{e.AddonContainer.Addon.AddonId}]");
                IAddonGameStateService addonGameStateService = App.ServiceProvider.GetService<IAddonGameStateService>();

                HashSet<AddonContainer> requiredAddons = new HashSet<AddonContainer>();
                await addonGameStateService.GetParentAddons(_viewModel.AddonContainers.ToList(), e.AddonContainer, requiredAddons);

                HashSet<AddonContainer> installableAddons = requiredAddons.Where(x => x.InstallState != InstallState.InstalledEnabled && x.InstallState != InstallState.InstalledDisabled).ToHashSet();

                HashSet<AddonContainer> enableableAddons = requiredAddons.Where(x => x.InstallState == InstallState.InstalledDisabled).ToHashSet();
                enableableAddons.Add(e.AddonContainer);

                List<AddonContainer> installedAddons = installableAddons.ToList();
                installedAddons.AddRange(enableableAddons);

                IEnumerable<AddonConflict> addonConflicts = await addonGameStateService.CheckConflicts(installedAddons, installableAddons);
                if (addonConflicts.Count() == 0)
                {
                    _log.LogInformation($"Required addons for [{e.AddonContainer.Addon.AddonId}]:");
                    _log.LogObject(installableAddons);

                    _viewModel.AddonInstallActions.Clear();
                    _viewModel.AddonInstallActions.AddRange(installableAddons.Select(x => new AddonInstallAction(x, InstallerActionType.Install)));
                    _viewModel.AddonInstallActions.AddRange(enableableAddons.Select(x => new AddonInstallAction(x, InstallerActionType.Enable)));

                    _viewModel.UiState = Enums.UiState.Installer;
                }
                else
                {
                    _viewModel.AddonConflicts.Clear();
                    _viewModel.AddonConflicts.AddRange(addonConflicts);
                    _viewModel.UiState = Enums.UiState.Conflicts;
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError + e.AddonContainer?.Addon?.AddonName);
                _log.LogCritical(ex, $"Enabling [{e.AddonContainer?.Addon?.AddonId}]");
            }
        }

        /// <summary>
        /// Display a open file dialog and set the corresponding userconfig
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonFindGameExecutableClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog;

            try
            {
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Guild Wars 2 (gw2-64.exe)|gw2-64.exe";
                openFileDialog.Multiselect = false;

                bool? dialogResult = openFileDialog.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    _viewModel.UserConfig.GameLocation = new Uri(openFileDialog.FileName);
                    _log.LogDebug($"Set game location to [{_viewModel.UserConfig.GameLocation}]");
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.SetGamePathError);
                _log.LogCritical(ex, $"Setting game path");
            }
        }

        /// <summary>
        /// Return to default UI panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonInstallerCancelClick(object sender, RoutedEventArgs e)
        {
            await InitializeUi();
        }

        /// <summary>
        /// Return to default UI panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonConflictsBackClick(object sender, RoutedEventArgs e)
        {
            await InitializeUi();
        }

        /// <summary>
        /// Return to default UI panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonAboutBackClick(object sender, RoutedEventArgs e)
        {
            _viewModel.UiState = Enums.UiState.AddonList;
        }

        /// <summary>
        /// Performs the requested action on the list of selected addons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonInstallerInstallClick(object sender, RoutedEventArgs e)
        {
            bool chainSuccess = true;
            try
            {
                _viewModel.InstallerProgressCancelEnabled = false;
                _viewModel.AddonInstallProgresses.Clear();
                _viewModel.AddonInstallProgresses.AddRange(_viewModel.AddonInstallActions.Select(x => new AddonInstallProgress(x)));

                _viewModel.UiState = Enums.UiState.InstallerProgress;

                foreach (AddonInstallProgress addonInstallProgress in _viewModel.AddonInstallProgresses)
                {
                    if (!await PerformAddonAction(addonInstallProgress))
                    {
                        chainSuccess = false;
                    }
                }

                _viewModel.InstallerProgressCancelEnabled = !chainSuccess;
                if (chainSuccess)
                {
                    //Delay to allow display of final status
                    await Task.Delay(1000);
                    await InitializeUi();
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError);
                _log.LogCritical(ex, $"Installing");
            }
        }

        /// <summary>
        /// Performs the requested Action on an addon
        /// </summary>
        /// <param name="addonGameStateService"></param>
        /// <param name="addonInstallProgress"></param>
        private async Task<bool> PerformAddonAction(AddonInstallProgress addonInstallProgress)
        {
            IAddonGameStateService addonGameStateService = App.ServiceProvider.GetService<IAddonGameStateService>();

            AddonInstallAction addonInstallAction = addonInstallProgress.AddonInstallAction;
            AddonContainer addonContainer = addonInstallAction.AddonContainer;

            try
            {
                if (addonContainer.Checked)
                {
                    bool actionResult = false;
                    addonInstallProgress.InstallProgress = InstallProgress.InProgress;

                    switch (addonInstallAction.InstallActionType)
                    {
                        case InstallerActionType.Install:
                            actionResult = await addonGameStateService.InstallAddon(addonContainer);
                            break;
                        case InstallerActionType.Enable:
                            actionResult = await addonGameStateService.EnableAddon(addonContainer);
                            break;
                        case InstallerActionType.Disable:
                            actionResult = await addonGameStateService.DisableAddon(addonContainer);
                            break;
                        case InstallerActionType.Remove:
                            actionResult = await addonGameStateService.RemoveAddon(addonContainer);
                            break;
                        case InstallerActionType.Update:
                            actionResult = await addonGameStateService.UpdateAddon(addonContainer);
                            break;

                    }

                    addonInstallProgress.InstallProgress = actionResult ? InstallProgress.Completed : InstallProgress.Error;
                    return actionResult;
                }

            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Installing {addonContainer.Addon.AddonId}");
                addonInstallProgress.InstallProgress = InstallProgress.Error;
            }
            return false;
        }

        /// <summary>
        /// Checks the installed (active/inactive) addons for updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonUpdateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _log.LogDebug($"Checking for updates");
                _viewModel.UiState = Enums.UiState.Loading;
                IAddonGameStateService addonGameStateService = App.ServiceProvider.GetService<IAddonGameStateService>();

                List<AddonContainer> installedAddons = _viewModel.AddonContainers.Where(x => x.InstallState == InstallState.InstalledEnabled ||
                                                                                             x.InstallState == InstallState.InstalledDisabled).ToList();

                IEnumerable<AddonContainer> updateableAddons = await addonGameStateService.GetUpdateableAddons(installedAddons);

                _viewModel.AddonInstallActions.Clear();
                _viewModel.AddonInstallActions.AddRange(updateableAddons.Select(x => new AddonInstallAction(x, InstallerActionType.Update)));

                if (_viewModel.AddonInstallActions.Count > 0)
                {
                    _viewModel.UiState = Enums.UiState.Installer;
                }
                else
                {
                    _viewModel.UiState = Enums.UiState.AddonList;
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError);
                _log.LogCritical(ex, $"Updating");
            }
        }


        /// <summary>
        /// Opens the icon link
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextBlockLegalNoticeIconsMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = _config.GetValue<string>("about:iconsLinkUrl");
            try
            {
                Uri uri = new Uri(url);
                uri.OpenWeb();
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.UncategorizedError);
                _log.LogCritical(ex, nameof(OnTextBlockLegalNoticeIconsMouseLeftButtonDown));
            }
        }

        private async void OnButtonAppUpdateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                IAppUpdaterService appUpdaterService = App.ServiceProvider.GetService<IAppUpdaterService>();
                await appUpdaterService.Update();
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.UncategorizedError);
                _log.LogCritical(ex, nameof(OnButtonAppUpdateClick));
            }
        }
    }
}
