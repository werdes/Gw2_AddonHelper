using Gw2_AddonHelper.AddonLib.Model.GameState;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Installer
{
    public abstract class BaseAddonInstaller
    {
        protected IConfiguration _config;
        protected string _gamePath;
        protected ILogger<BaseAddonInstaller> _log;

        protected Common.Model.AddonList.Addon _addon;

        public BaseAddonInstaller(Common.Model.AddonList.Addon addon, string gamePath)
        {
            _config = Lib.ServiceProvider.GetService<IConfiguration>();
            _log = Lib.ServiceProvider.GetService<ILogger<BaseAddonInstaller>>();
            _gamePath = gamePath;
            _addon = addon;
        }

        public abstract string GetInstallationBaseDirectory();
        public abstract string GetInstallationEntrypointFile();

        /// <summary>
        /// Disables an addon by renaming the central dll file
        /// </summary>
        /// <param name="manifest"></param>
        public virtual async Task<bool> Disable()
        {
            bool disabled = false;
            string gamePath = Path.GetDirectoryName(_gamePath);
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
        public virtual async Task<bool> Enable()
        {
            bool enabled = false;
            string gamePath = Path.GetDirectoryName(_gamePath);
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

        /// <summary>
        /// Returns the file path of the version file
        /// </summary>
        /// <returns></returns>
        public string GetInstallationVersionFile()
        {
            string gamePath = Path.GetDirectoryName(_gamePath);
            string relativeFilePath = GetInstallationEntrypointFile();
            string filePath = Path.Combine(gamePath, relativeFilePath);

            string versionFile = Path.ChangeExtension(filePath,
                                                      _config.GetValue<string>("installation:versionFileExtension"));

            return versionFile;
        }

        /// <summary>
        /// returns the installed version
        /// </summary>
        /// <returns></returns>
        public string GetInstalledVersion()
        {
            string version = string.Empty;
            string versionFile = GetInstallationVersionFile();

            if (File.Exists(versionFile))
            {
                version = File.ReadAllText(versionFile, Encoding.UTF8);
            }

            return version;
        }

        /// <summary>
        /// Saves the downloaded version to a file
        /// </summary>
        /// <param name="download"></param>
        protected void StoreVersionFile(DownloadResult download)
        {
            string filePath = GetInstallationVersionFile();

            File.WriteAllText(filePath, download.Version, Encoding.UTF8);
        }
    }
}
