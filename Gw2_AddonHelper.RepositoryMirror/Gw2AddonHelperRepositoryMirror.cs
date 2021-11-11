using Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader;
using Gw2_AddonHelper.Common.Model;
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
            catch(Exception ex)
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
            IAddonListService addonListService = Program.ServiceProvider.GetService<IAddonListService>();
            while (true)
            {
                DateTime end = DateTime.Now + _config.GetValue<TimeSpan>("delay");

                try
                {
                    VersionContainer versionContainer = new VersionContainer();
                    AddonListContainer addonListContainer = new AddonListContainer();

                    List<Addon> addons = await addonListService.GetAddonsAsync();

                    foreach (Addon addon in addons)
                    {
                        IAddonDownloader downloader = AddonDownloaderFactory.GetDownloader(addon);
                        string version = await downloader.GetLatestVersion();

                        versionContainer.Versions.Add(addon.AddonId, version);
                        addonListContainer.Addons.Add(addon);
                        _log.LogDebug($"latest version of {addon.AddonId} is {version}");
                    }

                    versionContainer.CrawlTime = DateTime.UtcNow;
                    addonListContainer.CrawlTime = DateTime.UtcNow;
                    addonListContainer.RepositoryVersion = await ((GithubAddonListService)addonListService).GetListVersion();

                    string[] files = _config.GetSection("updatesOutputPath").Get<string[]>();
                    foreach (string outputFile in files)
                    {
                        _log.LogInformation($"Writing updates output [{outputFile}]");
                        File.WriteAllText(outputFile, JsonConvert.SerializeObject(versionContainer), Encoding.UTF8);
                    }

                    files = _config.GetSection("addonsOutputPath").Get<string[]>();
                    foreach (string outputFile in files)
                    {
                        _log.LogInformation($"Writing addons output [{outputFile}]");
                        File.WriteAllText(outputFile, JsonConvert.SerializeObject(addonListContainer), Encoding.UTF8);
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
