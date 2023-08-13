using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Extractor;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Installer;
using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model.AddonList;
using Gw2_AddonHelper.Services;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.UpdateCheck
{
    public class Gw2AddonHelperRepositoryMirror
    {
        private ILogger _log;
        private IConfiguration _config;

        public Gw2AddonHelperRepositoryMirror()
        {
            _log = Program.ServiceProvider.GetService<ILogger<Gw2AddonHelperRepositoryMirror>>();
            _config = Program.ServiceProvider.GetService<IConfiguration>();

            try
            {
                Task.WaitAll(Run());
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Task runner");
            }
        }

        /// <summary>
        /// Runs the stuff
        /// </summary>
        /// <returns></returns>
        private async Task Run()
        {
            IAddonSourceService addonSourceService = Program.ServiceProvider.GetService<IAddonSourceService>();
            while (true)
            {
                DateTime end = DateTime.Now + _config.GetValue<TimeSpan>("delay");

                try
                {

                    VersionContainer versionContainer = new VersionContainer();
                    VersionContainer previousVersionContainer = new VersionContainer();
                    AddonListContainer addonListContainer = new AddonListContainer();

                    string[] updateFiles = _config.GetSection("updatesOutputPath").Get<string[]>();

                    string lastUpdateFile = updateFiles[0];
                    if (File.Exists(lastUpdateFile))
                    {
                        string lastUpdateJson = File.ReadAllText(lastUpdateFile, Encoding.UTF8);
                        previousVersionContainer = (VersionContainer)JsonConvert.DeserializeObject<VersionContainer>(lastUpdateJson);
                    }

                    List<Addon> addons = await addonSourceService.GetAddonsAsync();

                    foreach (Addon addon in addons)
                    {
                        IAddonDownloader downloader = AddonDownloaderFactory.GetDownloader(addon);
                        string version = await downloader.GetLatestVersion();

                        string previousRunAddonVersion;
                        Dictionary<string, string> oldHashes;
                        bool previousRunAddonFound = previousVersionContainer.Versions.TryGetValue(addon.AddonId, out previousRunAddonVersion);

                        // Download version and determine hash if:
                        // > Addon was present in previous run and version has changed
                        // > Addon was newly added (not present in previous run)
                        // > No hash was found for the addon in the previous run 
                        if ((previousRunAddonFound && previousRunAddonVersion != version) ||
                            !previousRunAddonFound ||
                            !previousVersionContainer.FileHashes.ContainsKey(addon.AddonId))
                        {
                            try
                            {
                                DownloadResult downloadResult = await downloader.Download();

                                IAddonExtractor addonExtractor = AddonExtractorFactory.GetExtractor(addon);
                                ExtractionResult extractionResult = await addonExtractor.Extract(downloadResult, version);

                                versionContainer.FileHashes.Add(addon.AddonId, new Dictionary<string, string>());
                                foreach (ExtractionResultFile extractedFile in extractionResult.AddonFiles)
                                {
                                    string fullRelativePath = Path.Combine(extractedFile.RelativePath, extractedFile.FileName);
                                    versionContainer.FileHashes[addon.AddonId].Add(fullRelativePath, extractedFile.FileContent.GetMd5Hash());
                                }
                            }
                            catch(Exception ex)
                            {
                                _log.LogCritical($"{addon.AddonId}: {ex}");
                            }
                        }
                        else if (previousVersionContainer.FileHashes.TryGetValue(addon.AddonId, out oldHashes))
                        {
                            versionContainer.FileHashes.Add(addon.AddonId, oldHashes);
                        }
                        else
                        {
                            versionContainer.FileHashes.Add(addon.AddonId, null);
                        }

                        versionContainer.Versions.Add(addon.AddonId, version);
                        addonListContainer.Addons.Add(addon);
                        _log.LogDebug($"latest version of {addon.AddonId} is {version}");
                    }

                    versionContainer.CrawlTime = DateTime.UtcNow;
                    addonListContainer.CrawlTime = DateTime.UtcNow;
                    addonListContainer.RepositoryVersion = await addonSourceService.GetVersion();

                    foreach (string updateFile in updateFiles)
                    {
                        _log.LogInformation($"Writing updates output [{updateFile}]");
                        File.WriteAllText(updateFile, JsonConvert.SerializeObject(versionContainer), Encoding.UTF8);
                    }

                    string[] addonFiles = _config.GetSection("addonsOutputPath").Get<string[]>();
                    foreach (string addonFile in addonFiles)
                    {
                        _log.LogInformation($"Writing addons output [{addonFile}]");
                        File.WriteAllText(addonFile, JsonConvert.SerializeObject(addonListContainer), Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogCritical(ex, nameof(Run));
                }

                double sleepMs = Math.Max(0, (end - DateTime.Now).TotalMilliseconds);
                _log.LogInformation($"Sleeping for [{sleepMs}] ms");

                Thread.Sleep((int)sleepMs);
            }
        }
    }
}
