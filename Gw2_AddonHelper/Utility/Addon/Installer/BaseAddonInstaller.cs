using Gw2_AddonHelper.Model.GameState;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Installer
{
    public abstract class BaseAddonInstaller
    {
        protected IConfiguration _config;
        protected IUserConfigService _userConfigService;
        protected ILogger<BaseAddonInstaller> _log;

        protected Model.AddonList.Addon _addon;

        public BaseAddonInstaller(Model.AddonList.Addon addon)
        {
            _config = App.ServiceProvider.GetService<IConfiguration>();
            _userConfigService = App.ServiceProvider.GetService<IUserConfigService>();
            _log = App.ServiceProvider.GetService<ILogger<BaseAddonInstaller>>();
            _addon = addon;
        }

        public abstract string GetInstallationBaseDirectory();
        public abstract string GetInstallationEntrypointFile();

        /// <summary>
        /// Disables an addon by renaming the central dll file
        /// </summary>
        /// <param name="manifest"></param>
        public async Task<bool> Disable()
        {
            bool disabled = false;
            string gamePath = Path.GetDirectoryName(_userConfigService.GetConfig().GameLocation.LocalPath);
            string newExtension = _config.GetValue<string>("installation:disabledExtension");
            string installationFile = Path.Combine(gamePath,
                                                   GetInstallationEntrypointFile());

            if (File.Exists(installationFile))
            {
                string disabledPath = Path.ChangeExtension(installationFile, newExtension);

                _log.LogInformation($"Disabling [{_addon.AddonId}: {installationFile} => {disabledPath}");
                await Task.Run(() => File.Move(installationFile, disabledPath, true));
                disabled = true;
                _log.LogInformation($"Disabled [{_addon.AddonId}: {installationFile} => {disabledPath}");
            }
            else
            {
                _log.LogWarning($"Cannot disable [{installationFile}] for [{_addon.AddonId}]: File doesn't exist");
            }
            return disabled;
        }

        /// <summary>
        /// enables an addon file by naming the dll file back to its original name
        /// </summary>
        /// <param name="manifest"></param>
        public async Task<bool> Enable()
        {
            bool enabled = false;
            string gamePath = Path.GetDirectoryName(_userConfigService.GetConfig().GameLocation.LocalPath);
            string newExtension = _config.GetValue<string>("installation:disabledExtension");
            string installationFile = Path.Combine(gamePath,
                                                   GetInstallationEntrypointFile());
            string disabledFile = Path.ChangeExtension(installationFile, newExtension);

            if (File.Exists(disabledFile))
            {
                _log.LogInformation($"Enabling [{_addon.AddonId}: {installationFile} => {disabledFile}");
                await Task.Run(() => File.Move(disabledFile, installationFile, true));
                enabled = true;
                _log.LogInformation($"Enabled [{_addon.AddonId}: {installationFile} => {disabledFile}");
            }
            else
            {
                _log.LogWarning($"Cannot enable [{installationFile}] for [{_addon.AddonId}]: File doesn't exist");
            }
            return enabled;
        }
    }
}
