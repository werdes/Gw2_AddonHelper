using Gw2_AddonHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Extractor
{
    public static class AddonExtractorFactory
    {
        public static IAddonExtractor GetExtractor(Model.AddonList.Addon addon)
        {
            switch (addon.DownloadType)
            {
                case DownloadType.Archive:
                    break;
                case DownloadType.Dll:
                    break;
                default:
                    break;
            }
            throw new ArgumentException($"No installer for installmode [{addon.InstallMode}, {addon.AddonId}]");
        }
    }
}
