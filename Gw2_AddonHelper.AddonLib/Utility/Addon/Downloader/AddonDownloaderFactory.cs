using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader
{
    public static class AddonDownloaderFactory
    {
        public static IAddonDownloader GetDownloader(Model.AddonList.Addon addon)
        {
            switch (addon.HostType)
            {
                case Model.HostType.Github:
                    return new GithubAddonDownloader(addon);
                case Model.HostType.Standalone:
                    return new StandaloneAddonDownloader(addon);
                default:
                    throw new ArgumentException($"No downloader for hosttype [{addon.HostType}, {addon.AddonId}]");
            }
        }
    }
}
