using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.AddonLib.Model.Installer;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Extractor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Installer
{
    public class AddonLoaderAddonInstaller : BaseAddonInstaller, IAddonInstaller
    {
        private List<AddonLoaderFile> _configFiles;

        public AddonLoaderAddonInstaller(Common.Model.AddonList.Addon addon, string gamePath) : base(addon, gamePath)
        {
            _configFiles = _config.GetSection("installation:addonLoader:files").Get<List<AddonLoaderFile>>();
        }

        /// <summary>
        /// Returns the installation directory relative to the game executable from configuration
        /// </summary>
        /// <returns></returns>
        public override string GetInstallationBaseDirectory()
        {
            return String.Empty; // _configFiles.First().Directory;
        }

        /// <summary>
        /// Returns the theoretical entry point dll file path
        /// </summary>
        /// <returns></returns>
        public override string GetInstallationEntrypointFile()
        {
            string dir = GetInstallationBaseDirectory();
            string file = _configFiles.First(x => x.Key == Common.Model.AddonLoaderFileKey.Loader).FileName;
            return Path.Combine(dir, file);
        }


        /// <summary>
        /// Installs the extracted file to the install location
        /// </summary>
        /// <param name="extraction"></param>
        public override async Task<bool> Install(ExtractionResult extraction, DownloadResult download)
        {
            bool installed = false;
            string gamePath = Path.GetDirectoryName(_gamePath);

            if (extraction.AddonFiles.Any(x => x.FileContent == null || x.FileContent.Length == 0))
                throw new ArgumentException($"Extraction has empty files for [{_addon.AddonId}@{extraction.Version}]");

            _log.LogInformation($"Installing addon [{_addon.AddonId}]");

            foreach (ExtractionResultFile file in extraction.AddonFiles)
            {
                string installPath = Path.Combine(gamePath, file.RelativePath, file.FileName);
                string fileDirectory = Path.GetDirectoryName(installPath);
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                _log.LogDebug($"Installing addon file [{file.FileName}] to [{installPath}]");
                await Task.Run(() => File.WriteAllBytes(installPath, file.FileContent));

                installed = true;
            }

            StoreVersionFile(download);
            return installed;
        }

        /// <summary>
        /// Removes the addon loader d3d9.dll
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="backupFiles"></param>
        public async Task<bool> Remove()
        {
            bool removed = true;
            string gamePath = Path.GetDirectoryName(_gamePath);

            foreach (AddonLoaderFile file in _configFiles)
            {
                try
                {
                    string installFile = Path.Combine(gamePath, file.Directory, file.FileName);

                    if (File.Exists(installFile))
                    {
                        _log.LogInformation($"Removing for [{_addon.AddonId}]:  {installFile}");
                        await Task.Run(() => File.Delete(installFile));
                        _log.LogInformation($"Removed for [{_addon.AddonId}]:  {installFile}");
                    }
                    else
                    {
                        _log.LogInformation($"[{installFile}] is not present");
                    }
                }
                catch (Exception ex)
                {
                    _log.LogCritical(ex, $"Couldn't remove {file.FileName} from loader installation");
                    removed = false;
                }
            }
            return removed;
        }

        /// <summary>
        /// Special case for Loader addon: Remove old version first
        /// </summary>
        /// <param name="addonDownloader"></param>
        /// <param name="addonExtractor"></param>
        /// <returns></returns>
        public override async Task<bool> Update(IAddonDownloader addonDownloader, IAddonExtractor addonExtractor)
        {
            _log.LogInformation($"Updating [{_addon.AddonId}] with Uninstall");
            DownloadResult downloadResult = await addonDownloader.Download();
            ExtractionResult extractionResult = await addonExtractor.Extract(downloadResult, downloadResult.Version);

            bool updateResult = await this.Remove() && 
                                await this.Install(extractionResult, downloadResult);

            return updateResult;
        }

        /// <summary>
        /// Disables the loader by renaming all configured entrypoint files
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> Disable()
        {
            string gamePath = Path.GetDirectoryName(_gamePath);
            string newExtension = _config.GetValue<string>("installation:disabledExtension");
            bool allFilesDisabled = true;


            foreach (AddonLoaderFile file in _configFiles)
            {
                try
                {
                    string installationFile = Path.Combine(gamePath, file.Directory, file.FileName);

                    if (File.Exists(installationFile))
                    {
                        string disabledPath = Path.ChangeExtension(installationFile, newExtension);

                        _log.LogInformation($"Disabling [{_addon.AddonId}: {installationFile} => {disabledPath}");
                        await Task.Run(() => File.Move(installationFile, disabledPath, true));
                        _log.LogInformation($"Disabled [{_addon.AddonId}: {installationFile} => {disabledPath}");
                    }
                    else
                    {
                        _log.LogWarning($"Cannot disable [{installationFile}] for [{_addon.AddonId}]: File doesn't exist");
                        //allFilesDisabled = false;
                    }
                }
                catch (Exception ex)
                {
                    _log.LogCritical(ex, $"Couldn't remove {file.FileName} from loader installation");
                    allFilesDisabled = false;
                }
            }
            return allFilesDisabled;
        }

        /// <summary>
        /// Enables the loader by renaming all configured entrypoint files
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> Enable()
        {
            bool enabled = true;
            string gamePath = Path.GetDirectoryName(_gamePath);
            string newExtension = _config.GetValue<string>("installation:disabledExtension");

            foreach (AddonLoaderFile file in _configFiles)
            {
                try
                {
                    string installationFile = Path.Combine(gamePath, file.Directory, file.FileName);
                    string disabledFile = Path.ChangeExtension(installationFile, newExtension);

                    if (File.Exists(disabledFile))
                    {
                        _log.LogInformation($"Enabling [{_addon.AddonId}: {installationFile} => {disabledFile}");
                        await Task.Run(() => File.Move(disabledFile, installationFile, true));
                        _log.LogInformation($"Enabled [{_addon.AddonId}: {installationFile} => {disabledFile}");
                    }
                    else
                    {
                        _log.LogWarning($"Cannot enable [{installationFile}] for [{_addon.AddonId}]: File doesn't exist");
                    }
                }
                catch (Exception ex)
                {
                    _log.LogCritical(ex, $"Couldn't remove {file.FileName} from loader installation");
                    enabled = false;
                }
            }
            return enabled;
        }
    }
}
