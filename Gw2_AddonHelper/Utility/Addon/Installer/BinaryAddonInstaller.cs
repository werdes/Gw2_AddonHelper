using Gw2_AddonHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Installer
{
    public class BinaryAddonInstaller : IAddonInstaller
    {
        private Model.AddonList.Addon _addon;

        public BinaryAddonInstaller(Model.AddonList.Addon addon)
        {
            _addon = addon;
        }

        public string GetInstallDirectory()
        {
            throw new NotImplementedException();
        }

        public InstallState GetInstallState()
        {
            throw new NotImplementedException();
        }

        public bool Install()
        {
            throw new NotImplementedException();
        }
    }
}
