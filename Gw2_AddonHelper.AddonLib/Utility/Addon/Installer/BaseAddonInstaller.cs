using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Extractor;
using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model.AddonList;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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
        public abstract Task<bool> Install(ExtractionResult extraction, DownloadResult download);

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
        /// Tries to determine the Version by comparing it to a list of hashes from the version list
        /// </summary>
        /// <param name="versions"></param>
        public void TryDetermineVersionFromService(VersionContainer versions)
        {
            string baseDir = GetInstallationBaseDirectory();
            string gamePath = Path.GetDirectoryName(_gamePath);

            Dictionary<string, string> addonHashes = null;

            if (versions.FileHashes.TryGetValue(_addon.AddonId, out addonHashes))
            {
                bool allHashesMatch = true;
                foreach (string relativePath in addonHashes.Keys)
                {
                    string filePath = Path.Combine(gamePath, baseDir, relativePath);
                    if (File.Exists(filePath))
                    {
                        string hash = File.ReadAllBytes(filePath).GetMd5Hash();
                        if (hash != addonHashes[relativePath])
                        {
                            allHashesMatch = false;
                        }
                    }
                    else
                    {
                        // -> File from hash list doesn't exist, so maybe it was added in a new version?
                        allHashesMatch = false;
                    }
                }

                if (allHashesMatch)
                {
                    string versionFilePath = GetInstallationVersionFile();
                    string version = null;

                    if (versions.Versions.TryGetValue(_addon.AddonId, out version))
                    {
                        _log.LogInformation($"All hashes match for [{_addon.AddonId}], service version {version} => create new version file");
                        File.WriteAllText(versionFilePath, version, Encoding.UTF8);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the selected addon
        ///  normally just Download->Extract->Install
        /// </summary>
        /// <param name="addonDownloader"></param>
        /// <param name="addonExtractor"></param>
        /// <returns></returns>
        public virtual async Task<bool> Update(IAddonDownloader addonDownloader, IAddonExtractor addonExtractor)
        {
            _log.LogInformation($"Updating [{_addon.AddonId}]");
            DownloadResult downloadResult = await addonDownloader.Download();
            ExtractionResult extractionResult = await addonExtractor.Extract(downloadResult, downloadResult.Version);

            bool updateResult = await this.Install(extractionResult, downloadResult);

            return updateResult;
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
