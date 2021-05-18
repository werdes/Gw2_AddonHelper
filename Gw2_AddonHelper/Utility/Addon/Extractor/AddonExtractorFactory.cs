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
                    return new ArchiveAddonExtractor(addon);
                case DownloadType.Dll:
                    return new DllAddonExtractor(addon);
                default:
                    break;
            }
            throw new ArgumentException($"No extractor for type [{addon.DownloadType}, {addon.AddonId}]");
        }
    }
}
