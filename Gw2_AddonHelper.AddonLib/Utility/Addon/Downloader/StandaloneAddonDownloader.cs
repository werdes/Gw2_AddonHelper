﻿using Gw2_AddonHelper.AddonLib.Model.GameState;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader
{
    public class StandaloneAddonDownloader : BaseAddonDownloader, IAddonDownloader
    {        
        public StandaloneAddonDownloader(Common.Model.AddonList.Addon addon) : base(addon)
        {
        }

        /// <summary>
        /// Loads an addon from a standalone source
        /// </summary>
        /// <returns></returns>
        public async Task<DownloadResult> Download()
        {
            string responseFileName = null;
            byte[] fileContent = null;
            byte[] buffer = new byte[4096];

            // Scuffed WebRequest approach to get response filename for obscured addons (e.g. buildPad)
            WebRequest addonFileRequest = WebRequest.Create(_addon.HostUrl);
            addonFileRequest.Method = "GET";
            using (WebResponse addonFileResponse = await addonFileRequest.GetResponseAsync())
            {
                responseFileName = Path.GetFileName(addonFileResponse.ResponseUri.AbsoluteUri);

                using(Stream responseStream = addonFileResponse.GetResponseStream())
                {
                    using(MemoryStream responseMemoryStream = new MemoryStream())
                    {
                        int readBytes = 0;
                        do
                        {
                            readBytes = responseStream.Read(buffer, 0, buffer.Length);
                            responseMemoryStream.Write(buffer, 0, readBytes);

                        } while (readBytes != 0);

                        fileContent = responseMemoryStream.ToArray();
                    }
                }
            }
            

            string version = DateTime.UtcNow.ToString("s");
            if (_addon.VersioningType == Common.Model.VersioningType.HostFileMd5)
            {
                version = (await WebClient.DownloadStringTaskAsync(_addon.VersionUrl)).Split(' ').First();
            }


            return new DownloadResult()
            {
                FileContent = fileContent,
                FileName = responseFileName,
                Version = version
            };
        }

        /// <summary>
        /// Checks, if an update is available for the addon 
        ///  > Only available if an MD5 Version source is given
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        public async Task<string> GetLatestVersion()
        {
            if(_addon.VersioningType == Common.Model.VersioningType.HostFileMd5)
            {
                string latestVersionHash = await WebClient.DownloadStringTaskAsync(_addon.VersionUrl);
                return latestVersionHash.Split(' ').FirstOrDefault();
            }
            return null;
        }
    }
}
