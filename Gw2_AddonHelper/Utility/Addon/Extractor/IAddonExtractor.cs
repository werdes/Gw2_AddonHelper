using Gw2_AddonHelper.Model;
using Gw2_AddonHelper.Model.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Extractor
{
    public interface IAddonExtractor
    {
        Task<ExtractionResult> Extract(DownloadResult download, string version);
    }
}
