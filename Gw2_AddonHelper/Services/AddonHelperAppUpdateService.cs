using Gw2_AddonHelper.AddonLib.Utility.Github;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services
{
    public class AddonHelperAppUpdateService : IAppUpdaterService
    {
        private GitHubClient _gitHubClient;
        private IConfiguration _config;
        private ILogger<AddonHelperAppUpdateService> _log;

        public AddonHelperAppUpdateService()
        {
            _gitHubClient = App.ServiceProvider.GetService<GitHubClient>();
            _config = App.ServiceProvider.GetService<IConfiguration>();
            _log = App.ServiceProvider.GetService<ILogger<AddonHelperAppUpdateService>>();
        }


        /// <summary>
        /// Checks if an update to the application is available
        /// </summary>
        /// <returns></returns>
        public async Task<Version> GetLatestVersion()
        {
            if (GithubRatelimitService.Instance.CanCall())
            {
                Uri repoUrl = _config.GetValue<Uri>("selfUpdate:repoReleaseUrl");
                try
                {
                    Release githubRelease = (await _gitHubClient.Connection.Get<Release>(repoUrl, TimeSpan.FromSeconds(30))).Body;
                    GithubRatelimitService.Instance.RegisterCall();

                    Version version = new Version(githubRelease.TagName);
                    return version;
                }
                catch (NotFoundException)
                {
                    _log.LogWarning($"No version found for [{repoUrl}]");
                }
            }
            return null;
        }

        /// <summary>
        /// Updates the launcher
        /// </summary>
        /// <returns></returns>
        public async Task Update()
        {
            throw new NotImplementedException();
        }
    }
}
