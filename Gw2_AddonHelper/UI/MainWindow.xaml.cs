using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.AddonList;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.AddonLib.Extensions;
using Gw2_AddonHelper.Model.UI;
using Gw2_AddonHelper.AddonLib.Model.UserConfig;
using Gw2_AddonHelper.AddonLib.Services.Interfaces;
using Gw2_AddonHelper.UI.Localization;
using Gw2_AddonHelper.Extensions;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        public MainWindow(IConfiguration config, ILogger<MainWindow> log, IAddonListService addonListManager, IUserConfigService userConfigService)
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();
            _viewModel.UiState = UiState.Loading;
            _viewModel.UserConfig = userConfigService.GetConfig();
            _viewModel.UserConfig.LanguageChanged += OnUserConfigLanguageChanged;

            _log = log;
            _config = config;
            _userConfigService = userConfigService;

            //Tell the localization provider to refresh the object data provider
            LocalizationProvider.ChangeCulture(_viewModel.UserConfig.Language);
            DataContext = _viewModel;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.AddonContainers.Count == 0)
                {
                    await InitializeUi();
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.UiErrorWarning);
            }
        }

        /// <summary>
        /// Returns the available Languages from the resource file
        /// </summary>
        /// <returns></returns>
        private async Task<List<CultureInfo>> LoadAvailableLanguages()
        {
            List<CultureInfo> availableCultures = new List<CultureInfo>();

            await Task.Run(() =>
            {
                availableCultures = LocalizationProvider.GetAvailableCultures();
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
                _viewModel.UiState = UiState.Loading;

                IAddonListService addonListService = App.ServiceProvider.GetService<IAddonListService>();
                IAddonGameStateService addonGameStateService = App.ServiceProvider.GetService<IAddonGameStateService>();

                List<Addon> lstAddons = await addonListService.GetAddonsAsync();
                addonListService.Store();

                List<AddonContainer> containers = (await addonGameStateService.GetAddonContainers(lstAddons)).OrderByDescending(x => x.SortKey).ToList();
                _viewModel.AddonContainers = new ObservableCollection<AddonContainer>(containers);
                _viewModel.AvailableCultures = new ObservableCollection<CultureInfo>(await LoadAvailableLanguages());

                _viewModel.UiState = UiState.AddonList;
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.UiErrorWarning);
                _log.LogError(ex, $"Initializing UI");
            }
        }

        /// <summary>
        /// Sets the UI "Error" Panel to visible
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="errorTitle"></param>
        private void SetUiError(Exception ex, string errorTitle)
        {
            _viewModel.UiState = UiState.Error;
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
            _viewModel.UiState = UiState.Settings;
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
                _log.LogError(ex, $"Save Click");
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
                _log.LogError(ex, $"Cancel Click");
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
                _log.LogError(ex, $"Opening directory for [{e.AddonContainer?.Addon?.AddonId}]");
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

                    _viewModel.UiState = UiState.Installer;
                }
                else
                {
                    _viewModel.AddonConflicts.Clear();
                    _viewModel.AddonConflicts.AddRange(addonConflicts);
                    _viewModel.UiState = UiState.Conflicts;
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError + e.AddonContainer?.Addon?.AddonName);
                _log.LogError(ex, $"Installing [{e.AddonContainer?.Addon?.AddonId}]");
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

                _viewModel.UiState = UiState.Installer;

            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError + e.AddonContainer?.Addon?.AddonName);
                _log.LogError(ex, $"Removing [{e.AddonContainer?.Addon?.AddonId}]");
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

                _viewModel.UiState = UiState.Installer;
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError + e.AddonContainer?.Addon?.AddonName);
                _log.LogError(ex, $"Disabling [{e.AddonContainer?.Addon?.AddonId}]");
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

                    _viewModel.UiState = UiState.Installer;
                }
                else
                {
                    _viewModel.AddonConflicts.Clear();
                    _viewModel.AddonConflicts.AddRange(addonConflicts);
                    _viewModel.UiState = UiState.Conflicts;
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError + e.AddonContainer?.Addon?.AddonName);
                _log.LogError(ex, $"Enabling [{e.AddonContainer?.Addon?.AddonId}]");
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
                _log.LogError(ex, $"Setting game path");
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
        /// Performs the requested action on the list of selected addons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonInstallerInstallClick(object sender, RoutedEventArgs e)
        {
            bool chainSuccess = true;
            try
            {
                _viewModel.AddonInstallProgresses.Clear();
                _viewModel.AddonInstallProgresses.AddRange(_viewModel.AddonInstallActions.Select(x => new AddonInstallProgress(x)));

                _viewModel.UiState = UiState.InstallerProgress;

                foreach (AddonInstallProgress addonInstallProgress in _viewModel.AddonInstallProgresses)
                {
                    if (!await PerformAddonAction(addonInstallProgress))
                    {
                        chainSuccess = false;
                    }
                }

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
                _log.LogError(ex, $"Installing");
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
                _viewModel.UiState = UiState.Loading;
                IAddonGameStateService addonGameStateService = App.ServiceProvider.GetService<IAddonGameStateService>();

                List<AddonContainer> installedAddons = _viewModel.AddonContainers.Where(x => x.InstallState == InstallState.InstalledEnabled ||
                                                                                             x.InstallState == InstallState.InstalledDisabled).ToList();

                IEnumerable<AddonContainer> updateableAddons = await addonGameStateService.GetUpdateableAddons(installedAddons);

                _viewModel.AddonInstallActions.Clear();
                _viewModel.AddonInstallActions.AddRange(updateableAddons.Select(x => new AddonInstallAction(x, InstallerActionType.Update)));

                if (_viewModel.AddonInstallActions.Count > 0)
                {
                    _viewModel.UiState = UiState.Installer;
                }
                else
                {
                    _viewModel.UiState = UiState.AddonList;
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.InstallAddonError);
                _log.LogError(ex, $"Updating");
            }
        }

        /// <summary>
        /// Shortcut to Install State group 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonInstallStateShortcutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                InstallState installState;

                if (e.Source is Button)
                {
                    Button senderButton = (Button)e.Source;
                    string installStateName = senderButton.Tag.ToString();

                    if (Enum.TryParse(installStateName, out installState))
                    {
                        List<GroupItem> items = itemscontrolAddonListAddonItems.GetChildrenOfType<GroupItem>();
                        GroupItem desiredItem = items.Where(x => Enum.Parse<InstallState>(x.Tag.ToString()) == installState).FirstOrDefault();

                        if(desiredItem != null)
                        {
                            ItemsControl itemsControl = (ItemsControl)scrollviewerAddonListAddonItems.Content;
                            var point = desiredItem.TranslatePoint(new Point(), itemsControl);
                            scrollviewerAddonListAddonItems.ScrollToVerticalOffset(point.Y);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetUiError(ex, Localization.Localization.UncategorizedError);
                _log.LogError(ex, $"Finding scroll point");
            }
        }
    }
}
