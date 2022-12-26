using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Gw2_AddonHelper.Common.Model;
using System.Text.RegularExpressions;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Installer
{
    public class ArcAddonInstaller : BaseAddonInstaller, IAddonInstaller
    {
        private const string ARC_PLUGIN_PREFIX = "arcdps_";

        public ArcAddonInstaller(Common.Model.AddonList.Addon addon, string gamePath) : base(addon, gamePath)
        {
        }


        /// <summary>
        /// Returns the install directory relative from the game exe path
        /// </summary>
        /// <returns></returns>
        public override string GetInstallationBaseDirectory()
        {
            return _config.GetValue<string>("installation:arc:directory");
        }

        /// <summary>
        /// Returns the theoretical entry point dll file path
        /// </summary>
        /// <returns></returns>
        public override string GetInstallationEntrypointFile()
        {
            string dir = GetInstallationBaseDirectory();
            string file = GetAddonDllFilename();
            return Path.Combine(dir, file);
        }

        /// <summary>
        /// Returns the filename of the arc plugin dll
        ///  > If it has the "Obscured Filenames" Flag, a http request for retrieving the original filename will be made
        ///  > otherwise 
        /// </summary>
        /// <returns></returns>
        private string GetAddonDllFilename()
        {
            if (_addon.HostType == HostType.Standalone &&
                _addon.AdditionalFlags.Contains(AddonFlag.ObscuredFilename))
            {
                string obscuredPatternConfigValue = _config.GetValue<string>("obscuredPatterns:" + _addon.AddonId + ":filePattern");
                if (!string.IsNullOrEmpty(obscuredPatternConfigValue))
                {
                    string gamePath = Path.GetDirectoryName(_gamePath);
                    string lookupDirectory = Path.Combine(gamePath, GetInstallationBaseDirectory());

                    if (Directory.Exists(lookupDirectory))
                    {
                        List<string> files = Directory.GetFiles(lookupDirectory, obscuredPatternConfigValue).ToList();
                        files.RemoveAll(x => x.EndsWith(_config.GetValue<string>("installation:versionFileExtension")));
                        if (files.Count == 1)
                        {
                            return Path.ChangeExtension(Path.GetFileName(files[0]), "dll");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(_addon.PluginName))
                            {
                                return _addon.PluginName;
                            }

                            // This is an exception for obscured addons (e.g. buildPad)
                            //   it doesn't matter what is returned from here, since the addon wasn't found with a the configured pattern
                            //   so it's not installed. 
                            return String.Empty;
                        }
                    }
                    else
                    {
                        // This is an exception for obscured addons (e.g. buildPad)
                        //   it doesn't matter what is returned from here, since the addon wasn't found with a the configured pattern
                        //   so it's not installed. 
                        return String.Empty;
                    }
                }

                throw new ArgumentException($"No filename for standalone with obscured filename for [{_addon.AddonId}]");
            }
            else if (!string.IsNullOrWhiteSpace(_addon.PluginName))
            { 
                return _addon.PluginName;
            }
            throw new ArgumentException($"No filename for [{_addon.AddonId}]");
        }

        /// <summary>
        /// removes an addon via manifest
        /// </summary>
        public async Task<bool> Remove()
        {
            bool removed = false;
            string gamePath = Path.GetDirectoryName(_gamePath);
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
        /// <param name="manifest"></param>
        public async Task<bool> Install(ExtractionResult manifest, DownloadResult download)
        {
            string gamePath = Path.GetDirectoryName(_gamePath);
            string relativePath = GetInstallationBaseDirectory();
            string installDirectory = Path.Combine(gamePath, relativePath);

            if (!Directory.Exists(installDirectory))
            {
                Directory.CreateDirectory(installDirectory);
            }

            if (manifest.AddonFiles.Any(x => x.FileContent == null || x.FileContent.Length == 0))
                throw new ArgumentException($"Manifest has empty files for [{_addon.AddonId}@{manifest.Version}]");

            foreach (ExtractionResultFile file in manifest.AddonFiles)
            {
                string installPath = Path.Combine(installDirectory, file.RelativePath, file.FileName);

                // Either it's a recognized addon entrypoint file OR it's the only .dll-File in the Archive
                // i cannot think of another way to identify the entrypoint file with wrong information in the repo 
                //  ( looking at you, boontable a.k.a. d3d9_arcdps_table.dll )
                //  if there's multiple dll files in an archive with wrong meta information i have no idea which one is the entrypoint
                if (( CheckFilenameForEntrypoint(file.FileName) ||
                     ( manifest.AddonFiles.Count(x => x.FileName.EndsWith(".dll")) == 1 && file.FileName.EndsWith(".dll"))) &&
                    !_addon.AdditionalFlags.Contains(AddonFlag.ObscuredFilename))
                {
                    installPath = Path.Combine(installDirectory, file.RelativePath, GetAddonDllFilename());
                }
                await Task.Run(() => File.WriteAllBytes(installPath, file.FileContent));
            }
            StoreVersionFile(download);

            return true;
        }

        private bool CheckFilenameForEntrypoint(string fileName)
        {
            // Loader Key+dll = Entrypoint
            if(fileName.Contains(_addon.LoaderKey) &&
               fileName.EndsWith(".dll"))
            {
                return true;
            }

            // obscured plugins need pattern check
            if(_addon.AdditionalFlags.Contains(AddonFlag.ObscuredFilename))
            {
                string obscuredPatternConfigValue = _config.GetValue<string>("obscuredPatterns:" + _addon.AddonId + ":regexPattern");
                Regex regex = new Regex(obscuredPatternConfigValue);

                if(regex.Match(fileName).Success)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
