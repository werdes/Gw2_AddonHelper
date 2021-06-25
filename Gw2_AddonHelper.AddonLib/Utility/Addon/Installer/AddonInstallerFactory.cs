using Gw2_AddonHelper.AddonLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Installer
{
    public static class AddonInstallerFactory
    {
        public static IAddonInstaller GetInstaller(Model.AddonList.Addon addon, string gamePath)
        {
            switch (addon.InstallMode)
            {
                case InstallMode.Arc:
                    return new ArcAddonInstaller(addon, gamePath);
                case InstallMode.Binary:
                    return new BinaryAddonInstaller(addon, gamePath);
                case InstallMode.AddonLoader:
                    return new AddonLoaderAddonInstaller(addon, gamePath);
                default:
                    throw new ArgumentException($"No installer for installmode [{addon.InstallMode}, {addon.AddonId}]");
            }
        }
    }
}
