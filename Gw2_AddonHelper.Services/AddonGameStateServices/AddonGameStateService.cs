using Gw2_AddonHelper.AddonLib.Model.Exceptions;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Extractor;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Installer;
using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AddonGameStateServices
{
    public class AddonGameStateService : IAddonGameStateService
    {
        private ILogger _log;
        private IConfiguration _config;
        private IUserConfigService _userConfigService;

        public AddonGameStateService(ILogger<AddonGameStateService> log, IConfiguration configuration, IUserConfigService userConfigService)
        {
            _log = log;
            _config = configuration;
            _userConfigService = userConfigService;
        }

        /// <summary>
        /// Disables an addon by addon container
        /// </summary>
        /// <param name="addonContainer"></param>
        /// <returns></returns>
        public async Task<bool> DisableAddon(AddonContainer addonContainer)
        {
            bool disabled = false;
            if (addonContainer.InstallState == InstallState.InstalledEnabled)
            {
                _log.LogInformation($"Enabling [{addonContainer.Addon.AddonId}]");

                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);
                disabled = await addonInstaller.Disable();
            }
            return disabled;
        }

        /// <summary>
        /// Enables an addon by addon container
        /// </summary>
        /// <param name="addonContainer"></param>
        /// <returns></returns>
        public async Task<bool> EnableAddon(AddonContainer addonContainer)
        {
            bool enabled = false;
            if (addonContainer.InstallState == InstallState.InstalledDisabled)
            {
                _log.LogInformation($"Disabling [{addonContainer.Addon.AddonId}]");

                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);
                enabled = await addonInstaller.Enable();
            }
            return enabled;
        }

        /// <summary>
        /// Installs an addon by addon container
        /// </summary>
        /// <param name="addonContainer"></param>
        /// <returns></returns>
        public async Task<bool> InstallAddon(AddonContainer addonContainer)
        {
            bool installed = false;
            if (addonContainer.InstallState == InstallState.NotInstalled)
            {
                _log.LogInformation($"Installing [{addonContainer.Addon.AddonId}]");

                IAddonDownloader addonDownloader = AddonDownloaderFactory.GetDownloader(addonContainer.Addon);
                IAddonExtractor addonExtractor = AddonExtractorFactory.GetExtractor(addonContainer.Addon);
                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);

                DownloadResult downloadResult = await addonDownloader.Download();
                ExtractionResult extractionResult = await addonExtractor.Extract(downloadResult, downloadResult.Version);

                installed = await addonInstaller.Install(extractionResult, downloadResult);
            }
            return installed;
        }

        /// <summary>
        /// Uninstalls an addon by addon container
        /// </summary>
        /// <param name="addonContainer"></param>
        /// <returns></returns>
        public async Task<bool> RemoveAddon(AddonContainer addonContainer)
        {
            bool removalResult = false;
            bool enableResult = true;

            if (addonContainer.InstallState == InstallState.InstalledEnabled ||
                addonContainer.InstallState == InstallState.InstalledDisabled)
            {
                bool enableBeforeUpdate = addonContainer.InstallState == InstallState.InstalledDisabled;

                // If the addon is disabled, enable it first, then remove
                if (enableBeforeUpdate)
                {
                    enableResult = await EnableAddon(addonContainer);
                }

                _log.LogInformation($"Removing [{addonContainer.Addon.AddonId}]");

                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);
                removalResult = await addonInstaller.Remove();
            }
            return removalResult && enableResult;
        }

        /// <summary>
        /// Updates an addon, enables and disables it if necessary
        /// </summary>
        /// <param name="addonContainer"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAddon(AddonContainer addonContainer)
        {
            bool enableBeforeUpdate = addonContainer.InstallState == InstallState.InstalledDisabled;
            bool enableResult = true;
            bool disableResult = true;

            // If the addon is disabled, enable it first, then update an re-disable
            if (enableBeforeUpdate)
            {
                enableResult = await EnableAddon(addonContainer);
                addonContainer.InstallState = enableResult ? InstallState.InstalledEnabled : InstallState.InstalledDisabled;
            }

            IAddonDownloader addonDownloader = AddonDownloaderFactory.GetDownloader(addonContainer.Addon);
            IAddonExtractor addonExtractor = AddonExtractorFactory.GetExtractor(addonContainer.Addon);
            IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);

            bool updateResult = await addonInstaller.Update(addonDownloader, addonExtractor);

            if (enableBeforeUpdate)
            {
                disableResult = await DisableAddon(addonContainer);
                addonContainer.InstallState = disableResult ? InstallState.InstalledDisabled : InstallState.InstalledEnabled;
            }

            return enableResult && disableResult && updateResult;
        }

        /// <summary>
        /// Checks if the installed addon version is different from the supplied one
        /// </summary>
        /// <param name="addonContainer"></param>
        /// <param name="latestVersion"></param>
        /// <returns></returns>
        public async Task<bool> CheckAddonForUpdate(AddonContainer addonContainer)
        {
            bool updateAvailable = true;

            _log.LogInformation($"Checking [{addonContainer.Addon.AddonId}] for updates");

            if(addonContainer.Addon.AdditionalFlags.Contains(AddonFlag.SelfUpdating))
            {
                return false;
            }

            if (addonContainer.InstallState == InstallState.InstalledDisabled ||
                addonContainer.InstallState == InstallState.InstalledEnabled)
            {
                _log.LogDebug($"Addon [{addonContainer.Addon.AddonId}] is [{addonContainer.InstallState}]");
                IAddonDownloader addonDownloader = AddonDownloaderFactory.GetDownloader(addonContainer.Addon);
                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);

                _log.LogDebug($"Installer for [{addonContainer.Addon.AddonId}] is [{addonInstaller.GetType().Name}]");
                _log.LogDebug($"Downloader for [{addonContainer.Addon.AddonId}] is [{addonDownloader.GetType().Name}]");

                string installedVersion = addonInstaller.GetInstalledVersion();
                if(string.IsNullOrEmpty(installedVersion))
                {
                    // installed Version empty = no version file, try to get by hash comparison
                    
                }

                string latestVersion = await addonDownloader.GetLatestVersion();

                _log.LogDebug($"Installed version for [{addonContainer.Addon.AddonId}] is [{installedVersion}]");
                _log.LogDebug($"Latest version for [{addonContainer.Addon.AddonId}] is [{latestVersion}]");

                updateAvailable = !string.IsNullOrEmpty(latestVersion) &&
                                  latestVersion != installedVersion;
            }

            return updateAvailable;
        }

        /// <summary>
        /// Checks a list of addons for Updates, either by quick update service (if recent enough) or by the provided downloader/installer methods
        /// </summary>
        /// <param name="installedAddons"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AddonContainer>> GetUpdateableAddons(IEnumerable<AddonContainer> installedAddons, VersionContainer versions)
        {
            List<AddonContainer> addonContainers = new List<AddonContainer>();
            List<AddonContainer> quickVersionedAddons = new List<AddonContainer>();
            List<AddonContainer> fullVersionedAddons = new List<AddonContainer>();

            quickVersionedAddons.AddRange(installedAddons.Where(x => versions.Versions.ContainsKey(x.Addon.AddonId)));
            fullVersionedAddons = installedAddons.Where(x => !quickVersionedAddons.Contains(x)).ToList();

            // Addons that are available in the quick update service will be checked against that
            foreach (AddonContainer addonContainer in quickVersionedAddons)
            {
                if (addonContainer.InstallState == InstallState.InstalledDisabled ||
                    addonContainer.InstallState == InstallState.InstalledEnabled)
                {
                    IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);
                    string installedVersion = addonInstaller.GetInstalledVersion();

                    if (string.IsNullOrEmpty(installedVersion))
                    {
                        // installed Version empty = no version file, try to get by hash comparison
                        addonInstaller.TryDetermineVersionFromService(versions);
                        installedVersion = addonInstaller.GetInstalledVersion();
                    }

                    if (addonInstaller.GetInstalledVersion() != versions.Versions[addonContainer.Addon.AddonId] &&
                        !addonContainer.Addon.AdditionalFlags.Contains(AddonFlag.SelfUpdating))
                    {
                        addonContainers.Add(addonContainer);
                    }
                }
            }

            //Addons not available in the quick update service will be checked by traditional means
            foreach (AddonContainer addonContainer in fullVersionedAddons)
            {
                if (await CheckAddonForUpdate(addonContainer))
                {
                    addonContainers.Add(addonContainer);
                }
            }

            return addonContainers;
        }

        /// <summary>
        /// Returns the local installation of the given addon
        /// </summary>
        private AddonContainer GetAddonContainer(Addon addon)
        {
            AddonContainer addonContainer = new AddonContainer(addon);
            IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addon, _userConfigService.GetConfig().GameLocation.LocalPath);

            string gameDirectory = Path.GetDirectoryName(_userConfigService.GetConfig().GameLocation.LocalPath);
            string disabledExtension = _config.GetValue<string>("installation:disabledExtension");
            string installFile = Path.Combine(gameDirectory, addonInstaller.GetInstallationEntrypointFile());
            string installFileDisabled = Path.ChangeExtension(installFile, disabledExtension);

            if (File.Exists(installFile))
            {
                addonContainer.InstallState = InstallState.InstalledEnabled;
                addonContainer.InstallationEntrypointFile = installFile;
            }
            else if (File.Exists(installFileDisabled))
            {
                addonContainer.InstallState = InstallState.InstalledDisabled;
                addonContainer.InstallationEntrypointFile = installFile;
            }
            else
            {
                addonContainer.InstallState = InstallState.NotInstalled;
            }
            return addonContainer;
        }

        /// <summary>
        /// returns multiple containers
        /// </summary>
        /// <param name="addons"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AddonContainer>> GetAddonContainers(IEnumerable<Addon> addons, VersionContainer versions)
        {
            List<AddonContainer> addonContainers = new List<AddonContainer>();
            foreach (Addon addon in addons)
            {
                AddonContainer container = GetAddonContainer(addon);



                if (container.InstallState == InstallState.InstalledDisabled ||
                    container.InstallState == InstallState.InstalledEnabled)
                {
                    if (versions.Versions.ContainsKey(addon.AddonId))
                    {
                        IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addon, _userConfigService.GetConfig().GameLocation.LocalPath);
                        string installedVersion = addonInstaller.GetInstalledVersion();

                        if (string.IsNullOrEmpty(installedVersion))
                        {
                            // installed Version empty = no version file, try to get by hash comparison
                            addonInstaller.TryDetermineVersionFromService(versions);
                            installedVersion = addonInstaller.GetInstalledVersion();
                        }

                        container.QuickUpdateAvailable = !addon.AdditionalFlags.Contains(AddonFlag.SelfUpdating) &&
                                                         installedVersion != versions.Versions[addon.AddonId];
                    }
                }
                addonContainers.Add(container);
            }

            return addonContainers;
        }


        /// <summary>
        /// fills the "result" parameter with required addons for the installation of the given addon
        /// </summary>
        /// <param name="allAddons"></param>
        /// <param name="addon"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task GetParentAddons(List<AddonContainer> allAddons, AddonContainer addon, HashSet<AddonContainer> result)
        {
            if (result == null)
                result = new HashSet<AddonContainer>();

            List<AddonContainer> currentAddonRequirements = allAddons.Where(x => addon.Addon.RequiredAddons.Contains(x.Addon.AddonId)).ToList();

            if (currentAddonRequirements.Contains(addon))
                throw new AddonTreeException(AddonTreeException.ExceptionType.ReferenceLoop);

            foreach (AddonContainer requiredAddon in currentAddonRequirements)
            {
                await Task.Run(() => GetParentAddons(allAddons, requiredAddon, result));

                if (!result.Contains(requiredAddon))
                    result.Add(requiredAddon);
            }
        }

        /// <summary>
        /// Fills the "result" parameter with addons that have the given addon as a requirement
        /// </summary>
        /// <param name="allAddons"></param>
        /// <param name="addon"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task GetChildAddons(List<AddonContainer> allAddons, AddonContainer addon, HashSet<AddonContainer> result)
        {
            if (result == null)
                result = new HashSet<AddonContainer>();

            List<AddonContainer> currentAddonChildren = allAddons.Where(x => x.Addon.RequiredAddons.Contains(addon.Addon.AddonId)).ToList();

            if (currentAddonChildren.Contains(addon))
                throw new AddonTreeException(AddonTreeException.ExceptionType.ReferenceLoop);

            foreach (AddonContainer requiredAddon in currentAddonChildren)
            {
                await Task.Run(() => GetChildAddons(allAddons, requiredAddon, result));

                if (!result.Contains(requiredAddon))
                    result.Add(requiredAddon);
            }
        }

        /// <summary>
        /// Checks the list of new addons against the list of installed addons for conflicts
        /// </summary>
        /// <param name="installedAddons"></param>
        /// <param name="newAddons"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AddonConflict>> CheckConflicts(IEnumerable<AddonContainer> installedAddons, IEnumerable<AddonContainer> newAddons)
        {
            List<AddonConflict> conflicts = new List<AddonConflict>();

            await Task.Run(() =>
            {

                foreach (AddonContainer newAddon in newAddons)
                {
                    foreach (AddonContainer installedAddon in installedAddons)
                    {
                        if (newAddon.Addon.Conflicts.Contains(installedAddon.Addon.AddonId))
                        {
                            conflicts.Add(new AddonConflict(installedAddon, newAddon));
                        }
                    }
                }
            });

            return conflicts;
        }
    }
}
