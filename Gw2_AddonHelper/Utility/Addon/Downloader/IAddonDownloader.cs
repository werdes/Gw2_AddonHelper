﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Downloader
{
    public interface IAddonDownloader
    {
        byte[] Download();
    }
}
