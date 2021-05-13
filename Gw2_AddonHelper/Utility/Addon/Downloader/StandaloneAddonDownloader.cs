using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Downloader
{
    public class StandaloneAddonDownloader : IAddonDownloader
    {
        private Model.AddonList.Addon _addon;
        public StandaloneAddonDownloader(Model.AddonList.Addon addon)
        {
            _addon = addon;
        }

        public byte[] Download()
        {
            throw new NotImplementedException();
        }
    }
}
