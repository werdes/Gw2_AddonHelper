using Gw2_AddonHelper.Model.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Downloader
{
    public interface IAddonDownloader
    {
        Task<DownloadResult> Download();
        Task<bool> UpdateAvailable(string currentVersion);
    }
}
