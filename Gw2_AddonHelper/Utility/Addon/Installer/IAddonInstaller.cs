using Gw2_AddonHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Installer
{
    public interface IAddonInstaller
    {
        InstallState GetInstallState();
        string GetInstallDirectory();
        bool Install();
    }
}
