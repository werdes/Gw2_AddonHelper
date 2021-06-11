using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.GameState;
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
    public class BinaryAddonInstaller : BaseAddonInstaller, IAddonInstaller
    {
        public BinaryAddonInstaller(Model.AddonList.Addon addon) : base(addon)
        {
        }

        /// <summary>
        /// returns the relative install directory
        /// </summary>
        /// <returns></returns>
        public override string GetInstallationBaseDirectory()
        {
            string addonFolder = _config.GetValue<string>("installation:binary:directory");
            return Path.Combine(addonFolder, _addon.LoaderKey);
        }

        /// <summary>
        /// Returns the theoretical entry point dll file path
        /// </summary>
        /// <returns></returns>
        public override string GetInstallationEntrypointFile()
        {
            string dir = GetInstallationBaseDirectory();
            string file = _config.GetValue<string>("installation:binary:prefix") +
                          _addon.LoaderKey +
                          _config.GetValue<string>("installation:binary:extension");
            return Path.Combine(dir, file);
        }

        /// <summary>
        /// removes an addon via manifest
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="backupFiles"></param>
        public async Task<bool> Remove()
        {
            bool removed = false;
            string gamePath = Path.GetDirectoryName(_userConfigService.GetConfig().GameLocation.LocalPath);
            string installationFile = Path.Combine(gamePath,
                                                   GetInstallationEntrypointFile());

            if (File.Exists(installationFile))
            {
                _log.LogInformation($"Removing for [{_addon.AddonId}]:  {installationFile}");
                await Task.Run(() => File.Delete(installationFile));
                removed = true;
                _log.LogInformation($"Removed for [{_addon.AddonId}]:  {installationFile}");
            }
            else
            {
                _log.LogWarning($"Cannot remove [{installationFile}] for [{_addon.AddonId}]: File doesn't exist");
            }
            return removed;
        }

        /// <summary>
        /// Installs the addon to the configured arcdps folder
        /// </summary>
        /// <param name="extraction"></param>
        public async Task<bool> Install(ExtractionResult extraction, DownloadResult download)
        {
            bool installed = false;
            string gamePath = Path.GetDirectoryName(_userConfigService.GetConfig().GameLocation.LocalPath);
            string relativePath = GetInstallationBaseDirectory();
            string installDirectory = Path.Combine(gamePath, relativePath);

            if (!Directory.Exists(installDirectory))
            {
                Directory.CreateDirectory(installDirectory);
            }

            if (extraction.AddonFiles.Any(x => x.FileContent == null || x.FileContent.Length == 0))
                throw new ArgumentException($"Manifest has empty files for [{_addon.AddonId}@{extraction.Version}]");


            _log.LogInformation($"Installing addon [{_addon.AddonId}]");

            foreach (ExtractionResultFile file in extraction.AddonFiles)
            {
                string installPath = Path.Combine(installDirectory, file.RelativePath, file.FileName);
                string fileDirectory = Path.GetDirectoryName(installPath);
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                _log.LogDebug($"Installing addon file [{file.FileName}] to [{installPath}]");
                await Task.Run(() => File.WriteAllBytes(installPath, file.FileContent));

                StoreVersionFile(download);
                installed = true;
            }
            return installed;
        }
    }
}
