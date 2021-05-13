using Gw2_AddonHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Extractor
{
    public interface IAddonExtractor
    {
        bool ExtractTo(string path);
    }
}
