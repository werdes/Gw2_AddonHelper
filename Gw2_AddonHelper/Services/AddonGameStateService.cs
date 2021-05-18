using Gw2_AddonHelper.Custom.YamlDotNet;
using Gw2_AddonHelper.Extensions;
using Gw2_AddonHelper.Model;
using Gw2_AddonHelper.Model.AddonList;
using Gw2_AddonHelper.Model.AddonList.Github;
using Gw2_AddonHelper.Model.GameState;
using Gw2_AddonHelper.Model.UserConfig;
using Gw2_AddonHelper.Services.Interfaces;
using Gw2_AddonHelper.Utility.Addon;
using Gw2_AddonHelper.Utility.Addon.Downloader;
using Gw2_AddonHelper.Utility.Addon.Extractor;
using Gw2_AddonHelper.Utility.Addon.Installer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Gw2_AddonHelper.Services
{
    public class AddonGameStateService : IAddonGameStateService
    {

        private ILogger _log;
        private IConfiguration _config;
        private IUserConfigService _userConfigService;

        public AddonGameStateService(ILogger<GithubAddonListService> log, IConfiguration configuration, IUserConfigService userConfigService)
        {
            _log = log;
            _config = configuration;
            _userConfigService = userConfigService;
        }

        public async Task<bool> DisableAddon(AddonContainer addonContainer)
        {
            bool disabled = false;
            if (addonContainer.InstallState == InstallState.InstalledEnabled)
            {
                _log.LogInformation($"Enabling [{addonContainer.Addon.AddonId}]");

                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon);
                disabled = await addonInstaller.Disable();
            }
            return disabled;
        }

        public async Task<bool> EnableAddon(AddonContainer addonContainer)
        {
            bool enabled = false;
            if (addonContainer.InstallState == InstallState.InstalledDisabled)
            {
                _log.LogInformation($"Disabling [{addonContainer.Addon.AddonId}]");

                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon);
                enabled = await addonInstaller.Enable();
            }
            return enabled;
        }

        public async Task<bool> InstallAddon(AddonContainer addonContainer)
        {
            bool installed = false;
            if (addonContainer.InstallState == InstallState.NotInstalled)
            {
                _log.LogInformation($"Installing [{addonContainer.Addon.AddonId}]");

                IAddonDownloader addonDownloader = AddonDownloaderFactory.GetDownloader(addonContainer.Addon);
                IAddonExtractor addonExtractor = AddonExtractorFactory.GetExtractor(addonContainer.Addon);
                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon);

                DownloadResult downloadResult = await addonDownloader.Download();
                ExtractionResult extractionManifest = await addonExtractor.Extract(downloadResult, downloadResult.Version);

                installed = await addonInstaller.Install(extractionManifest);
            }
            return installed;
        }

        public async Task<bool> RemoveAddon(AddonContainer addonContainer)
        {
            bool removed = false;
            if (addonContainer.InstallState == InstallState.InstalledEnabled ||
                addonContainer.InstallState == InstallState.InstalledDisabled)
            {
                _log.LogInformation($"Removing [{addonContainer.Addon.AddonId}]");

                IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addonContainer.Addon);
                removed = await addonInstaller.Remove();
            }
            return removed;
        }


        /// <summary>
        /// Returns the local installation of the given addon
        /// </summary>
        public AddonContainer GetAddonInstallation(Addon addon)
        {
            AddonContainer addonContainer = new AddonContainer(addon);
            IAddonInstaller addonInstaller = AddonInstallerFactory.GetInstaller(addon);

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
    }
}
