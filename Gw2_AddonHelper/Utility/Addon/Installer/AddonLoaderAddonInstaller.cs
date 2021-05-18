using Gw2_AddonHelper.Model.GameState;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Installer
{
    public class AddonLoaderAddonInstaller : BaseAddonInstaller, IAddonInstaller
    {
        public AddonLoaderAddonInstaller(Model.AddonList.Addon addon) : base(addon)
        {
        }

        /// <summary>
        /// Returns the installation directory relative to the game executable from configuration
        /// </summary>
        /// <returns></returns>
        public override string GetInstallationBaseDirectory()
        {
            return _config.GetValue<string>("installation:addonLoader:directory");
        }

        /// <summary>
        /// Returns the theoretical entry point dll file path
        /// </summary>
        /// <returns></returns>
        public override string GetInstallationEntrypointFile()
        {
            string dir = GetInstallationBaseDirectory();
            string file = _config.GetValue<string>("installation:addonLoader:fileName");
            return Path.Combine(dir, file);
        }


        /// <summary>
        /// Installs the extracted file to the install location
        /// </summary>
        /// <param name="manifest"></param>
        public async Task<bool> Install(ExtractionResult manifest)
        {
            string gamePath = Path.GetDirectoryName(_userConfigService.GetConfig().GameLocation.LocalPath);
            string installFile = Path.Combine(gamePath, GetInstallationEntrypointFile());
            string installDirectory = Path.GetDirectoryName(installFile);
            if (!Directory.Exists(installDirectory))
            {
                Directory.CreateDirectory(installDirectory);
            }

            if (manifest.AddonFiles.Any(x => x.FileContent == null || x.FileContent.Length == 0))
                throw new ArgumentException($"Manifest has empty files for [{_addon.AddonId}@{manifest.Version}]");

            ExtractionResultFile loaderManifest = manifest.AddonFiles.First();
            await Task.Run(() => File.WriteAllBytes(installFile, loaderManifest.FileContent));

            return true;
        }

        /// <summary>
        /// Removes the addon loader d3d9.dll
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="backupFiles"></param>
        public async Task<bool> Remove()
        {
            bool removed = false;
            string gamePath = Path.GetDirectoryName(_userConfigService.GetConfig().GameLocation.LocalPath);
            string installFile = Path.Combine(gamePath, GetInstallationEntrypointFile());

            if (File.Exists(installFile))
            {
                _log.LogInformation($"Removing for [{_addon.AddonId}]:  {installFile}");
                await Task.Run(() => File.Delete(installFile));
                removed = true;
                _log.LogInformation($"Removed for [{_addon.AddonId}]:  {installFile}");
            }
            return removed;
        }
    }
}
