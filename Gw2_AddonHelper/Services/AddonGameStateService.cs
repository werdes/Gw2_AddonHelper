using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.AddonList;
using Gw2_AddonHelper.AddonLib.Model.Exceptions;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Extractor;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Installer;
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

namespace Gw2_AddonHelper.Services
{
    public class AddonGameStateService : IAddonGameStateService
    {
        private WebClient _webClient;
        private ILogger _log;
        private IConfiguration _config;
        private IUserConfigService _userConfigService;

        public AddonGameStateService(ILogger<GithubAddonListService> log, IConfiguration configuration, IUserConfigService userConfigService)
        {
            _log = log;
            _config = configuration;
            _userConfigService = userConfigService;

            _webClient = new WebClient();
            _webClient.Headers.Add("user-agent",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name + " " +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());

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
            }

            _log.LogInformation($"Updating [{addonContainer.Addon.AddonId}]");

            IAddonDownloader addonDownloader = AddonDownloaderFactory.GetDownloader(addonContainer.Addon);
            IAddonExtractor addonExtractor = AddonExtractorFactory.GetExtractor(addonContainer.Addon);
            IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);

            DownloadResult downloadResult = await addonDownloader.Download();
            ExtractionResult extractionResult = await addonExtractor.Extract(downloadResult, downloadResult.Version);

            bool installResult = await addonInstaller.Install(extractionResult, downloadResult);

            if (enableBeforeUpdate)
            {
                disableResult = await DisableAddon(addonContainer);
            }

            return enableResult && disableResult && installResult;
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

            if (addonContainer.InstallState == InstallState.InstalledDisabled ||
                addonContainer.InstallState == InstallState.InstalledEnabled)
            {
                IAddonDownloader addonDownloader = AddonDownloaderFactory.GetDownloader(addonContainer.Addon);
                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);

                string installedVersion = addonInstaller.GetInstalledVersion();
                updateAvailable = await addonDownloader.GetLatestVersion() != installedVersion;
            }

            return updateAvailable;
        }

        /// <summary>
        /// Checks a list of addons for Updates, either by quick update service (if recent enough) or by the provided downloader/installer methods
        /// </summary>
        /// <param name="installedAddons"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AddonContainer>> GetUpdateableAddons(IEnumerable<AddonContainer> installedAddons)
        {
            List<AddonContainer> addonContainers = new List<AddonContainer>();
            VersionContainer versionContainer = await GetVersions();
            DateTime minCrawlTime = DateTime.UtcNow - _config.GetValue<TimeSpan>("quickUpdateCheck:maxAge");

            List<AddonContainer> quickVersionedAddons = new List<AddonContainer>();
            List<AddonContainer> fullVersionedAddons = new List<AddonContainer>();

            if (versionContainer.CrawlTime >= minCrawlTime)
            {
                quickVersionedAddons.AddRange(installedAddons.Where(x => versionContainer.Versions.ContainsKey(x.Addon.AddonId)));
            }
            fullVersionedAddons = installedAddons.Where(x => !quickVersionedAddons.Contains(x)).ToList();

            // Addons that are available in the quick update service will be checked against that
            foreach (AddonContainer addonContainer in quickVersionedAddons)
            {
                if (addonContainer.InstallState == InstallState.InstalledDisabled ||
                    addonContainer.InstallState == InstallState.InstalledEnabled)
                {
                    IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon, _userConfigService.GetConfig().GameLocation.LocalPath);
                    string installedVersion = addonInstaller.GetInstalledVersion();

                    if (addonInstaller.GetInstalledVersion() != versionContainer.Versions[addonContainer.Addon.AddonId])
                    {
                        addonContainers.Add(addonContainer);
                    }
                }
            }

            //Addons not available in the quick update service will be checked by traditional means
            foreach (AddonContainer addonContainer in fullVersionedAddons)
            {
                if(await CheckAddonForUpdate(addonContainer))
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
        public async Task<IEnumerable<AddonContainer>> GetAddonContainers(IEnumerable<Addon> addons)
        {
            List<AddonContainer> addonContainers = new List<AddonContainer>();
            VersionContainer versions = await GetVersions();
            foreach (Addon addon in addons)
            {
                AddonContainer container = GetAddonContainer(addon);

                if (container.InstallState == InstallState.InstalledDisabled ||
                    container.InstallState == InstallState.InstalledEnabled)
                {
                    if (versions.Versions.ContainsKey(addon.AddonId))
                    {
                        IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addon, _userConfigService.GetConfig().GameLocation.LocalPath);
                        container.QuickUpdateAvailable = !addon.AdditionalFlags.Contains(AddonFlag.SelfUpdating) &&
                                                         addonInstaller.GetInstalledVersion() != versions.Versions[addon.AddonId];
                    }
                }
                addonContainers.Add(container);
            }

            return addonContainers;
        }

        /// <summary>
        /// Loads a list of current versions for quick update check on list creation
        /// </summary>
        /// <returns></returns>
        private async Task<VersionContainer> GetVersions()
        {
            VersionContainer container = new VersionContainer();
            try
            {
                Uri updateUri = new Uri(_config.GetValue<string>("quickUpdateCheck:url"));
                string json = await _webClient.DownloadStringTaskAsync(updateUri);

                container = JsonConvert.DeserializeObject<VersionContainer>(json);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, nameof(GetVersions));
            }
            return container;
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
