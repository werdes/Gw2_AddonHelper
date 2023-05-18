using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model.SelfUpdate;
using Gw2_AddonHelper.Common.Utility.Github;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AppUpdaterServices
{
    public class GithubAppUpdateService : BaseAppUpdaterService, IAppUpdaterService
    {
        private const string ZIP_MIME = "application/x-zip-compressed";
        private const int HIERARCHY = 2;

        private GitHubClient _gitHubClient;

        public GithubAppUpdateService() : base()
        {
            _gitHubClient = Lib.ServiceProvider.GetService<GitHubClient>();
        }

        public int GetHierarchy()
        {
            return HIERARCHY;
        }


        /// <summary>
        /// Checks if an update to the application is available
        /// </summary>
        /// <returns>Version and Notes</returns>
        public async Task<(Version, string)> GetLatestVersion()
        {
            if (GithubRatelimitService.Instance.CanCall())
            {

                Uri repoUrl = _config.GetValue<Uri>("selfUpdate:updaters:github:repoReleaseUrl");
                _log.LogInformation($"Getting latest app version from [{repoUrl}]");
                try
                {
                    Release githubRelease = (await _gitHubClient.Connection.Get<Release>(repoUrl, TimeSpan.FromSeconds(30))).Body;
                    GithubRatelimitService.Instance.RegisterCall();

                    Version version = new Version(githubRelease.TagName);
                    Uri assetUri = new Uri(githubRelease.Assets.Where(x => x.ContentType == ZIP_MIME).First().BrowserDownloadUrl);
                    string notes = githubRelease.Body;

                    _log.LogInformation($"Latest app version is [{version}]");

                    await StoreFile(assetUri, version.ToString(), notes);
                    return (version, githubRelease.Body);
                }
                catch (Exception ex) when (ex is NotFoundException or ArgumentNullException)
                {
                    _log.LogWarning($"No version found for [{repoUrl}]");
                }
            }
            return (null, null);
        }

        /// <summary>
        /// Returns wether the service implementation can be used for self update
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsAvailable()
        {
            _log.LogInformation($"Github app updater probed");
            return GithubRatelimitService.Instance.CanCall();
        }

        /// <summary>
        /// Updates the launcher
        /// </summary>
        /// <returns></returns>
        public async Task Update()
        {
            Uri repoUrl = _config.GetValue<Uri>("selfUpdate:updaters:github:repoReleaseUrl");
            try
            {
                _log.LogInformation($"Getting app update from [{repoUrl}]");

                if (GithubRatelimitService.Instance.CanCall())
                {
                    Release githubRelease = (await _gitHubClient.Connection.Get<Release>(repoUrl, TimeSpan.FromSeconds(30))).Body;
                    GithubRatelimitService.Instance.RegisterCall();

                    Version version = new Version(githubRelease.TagName);
                    Uri assetUri = new Uri(githubRelease.Assets.Where(x => x.ContentType == ZIP_MIME).First().BrowserDownloadUrl);

                    if (assetUri != null)
                    {
                        string outputFileName = Path.Combine(_config.GetValue<string>("selfUpdate:updaterAssetDirectory"), $"{version}.zip");
                        await UpdateInternal(version, assetUri, outputFileName);
                    }
                    else
                    {
                        _log.LogWarning("Asset Uri not found:");
                        _log.LogObject(githubRelease.Assets.Select(x => $"{x.Name}: {x.BrowserDownloadUrl}"));
                    }
                }
                else
                {
                    _log.LogWarning("Cannot call github due to ratelimit");
                }
            }
            catch (Exception ex) when (ex is NotFoundException or ArgumentNullException)
            {
                _log.LogWarning($"No version found for [{repoUrl}]");
            }
        }
    }
}
