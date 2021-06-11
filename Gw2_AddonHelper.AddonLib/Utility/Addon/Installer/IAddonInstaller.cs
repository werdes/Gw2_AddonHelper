using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.GameState;
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
        Task<bool> Install(ExtractionResult extraction, DownloadResult download);
        Task<bool> Enable();
        Task<bool> Disable();
        Task<bool> Remove();
    }
}
