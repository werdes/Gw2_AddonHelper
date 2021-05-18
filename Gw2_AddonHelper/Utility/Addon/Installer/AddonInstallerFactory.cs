using Gw2_AddonHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Installer
{
    public static class AddonInstallerFactory
    {
        public static IAddonInstaller GetInstaller(Model.AddonList.Addon addon)
        {
            switch (addon.InstallMode)
            {
                case InstallMode.Arc:
                    return new ArcAddonInstaller(addon);
                case InstallMode.Binary:
                    return new BinaryAddonInstaller(addon);
                case InstallMode.AddonLoader:
                    return new AddonLoaderAddonInstaller(addon);
                default:
                    throw new ArgumentException($"No installer for installmode [{addon.InstallMode}, {addon.AddonId}]");
            }
        }
    }
}
