using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Downloader
{
    public class GithubAddonDownloader : IAddonDownloader
    {
        private Model.AddonList.Addon _addon;
        public GithubAddonDownloader(Model.AddonList.Addon addon)
        {
            _addon = addon;
        }

        public byte[] Download()
        {
            throw new NotImplementedException();
        }
    }
}
