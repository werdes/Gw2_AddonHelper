using Gw2_AddonHelper.AddonLib.Model.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader
{
    public interface IAddonDownloader
    {
        Task<DownloadResult> Download();
        Task<string> GetLatestVersion();
    }
}
