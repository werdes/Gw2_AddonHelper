using Gw2_AddonHelper.Model;
using Gw2_AddonHelper.Model.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Installer
{
    public interface IAddonInstaller
    {
        string GetInstallationEntrypointFile();
        string GetInstallationBaseDirectory();
        Task<bool> Install(ExtractionResult manifest);
        Task<bool> Enable();
        Task<bool> Disable();
        Task<bool> Remove();
    }
}
