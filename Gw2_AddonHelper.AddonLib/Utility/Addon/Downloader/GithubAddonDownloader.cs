using Gw2_AddonHelper.AddonLib.Model.Exceptions;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.Common.Utility.Github;
using Gw2_AddonHelper.Common;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader
{
    public class GithubAddonDownloader : BaseAddonDownloader, IAddonDownloader
    {
        private GitHubClient _gitHubClient;

        public GithubAddonDownloader(Common.Model.AddonList.Addon addon) : base(addon)
        {
            _gitHubClient = Lib.ServiceProvider.GetService<GitHubClient>(); 
        }

        /// <summary>
        /// Downloads an addon from a github source
        ///  > Only available if ratelimit allows it
        /// </summary>
        /// <returns></returns>
        public async Task<DownloadResult> Download()
        {
            if (GithubRatelimitService.Instance.CanCall())
            {
                Release githubRelease = (await _gitHubClient.Connection.Get<Release>(_addon.HostUrl, TimeSpan.FromSeconds(30))).Body;
                GithubRatelimitService.Instance.RegisterCall();

                if (githubRelease != null)
                {
                    string downloadUrl = githubRelease.Assets.First().BrowserDownloadUrl;
                    byte[] fileContent = await WebClient.DownloadDataTaskAsync(downloadUrl);

                    return new DownloadResult()
                    {
                        FileContent = fileContent,
                        FileName = Path.GetFileName(downloadUrl),
                        Version = githubRelease.TagName
                    };
                }
                else throw new ArgumentException($"Host release not available for [{_addon.AddonId}]");
            }
            else throw new GithubRatelimitException(_addon);
        }

        /// <summary>
        /// Checks, if an update is available from a github source
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        public async Task<string> GetLatestVersion()
        {
            if (GithubRatelimitService.Instance.CanCall())
            {
                Release githubRelease = (await _gitHubClient.Connection.Get<Release>(_addon.HostUrl, TimeSpan.FromSeconds(30))).Body;
                GithubRatelimitService.Instance.RegisterCall();

                return githubRelease.TagName;
            }
            return null;
        }
    }
}
