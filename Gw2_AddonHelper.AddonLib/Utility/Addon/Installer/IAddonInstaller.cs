using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Extractor;
using Gw2_AddonHelper.Common.Model.AddonList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Installer
{
    public interface IAddonInstaller
    {
        string GetInstallationEntrypointFile();
        string GetInstallationVersionFile();
        string GetInstallationBaseDirectory();
        string GetInstalledVersion();
        void TryDetermineVersionFromService(VersionContainer versions);
        Task<bool> Install(ExtractionResult extraction, DownloadResult download);
        Task<bool> Enable();
        Task<bool> Disable();
        Task<bool> Remove();
        Task<bool> Update(IAddonDownloader downloader, IAddonExtractor addonExtractor);
    }
}
