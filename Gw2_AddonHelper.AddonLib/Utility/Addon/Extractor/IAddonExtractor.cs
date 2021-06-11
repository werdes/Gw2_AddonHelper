using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Extractor
{
    public interface IAddonExtractor
    {
        Task<ExtractionResult> Extract(DownloadResult download, string version);
    }
}
