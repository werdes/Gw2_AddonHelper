using Gw2_AddonHelper.Model.GameState;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Downloader
{
    public class StandaloneAddonDownloader : BaseAddonDownloader, IAddonDownloader
    {        
        public StandaloneAddonDownloader(Model.AddonList.Addon addon) : base(addon)
        {
        }

        /// <summary>
        /// Loads an addon from a standalone source
        /// </summary>
        /// <returns></returns>
        public async Task<DownloadResult> Download()
        {
            byte[] fileContent = await WebClient.DownloadDataTaskAsync(_addon.HostUrl);
            
            string version = DateTime.UtcNow.ToString("s");
            if (_addon.VersioningType == Model.VersioningType.HostFileMd5)
            {
                version = (await WebClient.DownloadStringTaskAsync(_addon.VersionUrl)).Split(' ').First();
            }

            return new DownloadResult()
            {
                FileContent = fileContent,
                FileName = Path.GetFileName(_addon.HostUrl.LocalPath),
                Version = version
            };
        }

        /// <summary>
        /// Checks, if an update is available for the addon 
        ///  > Only available if an MD5 Version source is given
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAvailable(string currentVersion)
        {
            if(_addon.VersioningType == Model.VersioningType.HostFileMd5)
            {
                string latestVersionHash = await WebClient.DownloadStringTaskAsync(_addon.VersionUrl);
                return currentVersion != latestVersionHash.Split(' ').FirstOrDefault();
            }
            return false;
        }
    }
}
