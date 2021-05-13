﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Extractor
{
    public class ArchiveAddonExtractor : IAddonExtractor
    {
        private Model.AddonList.Addon _addon;
        private byte[] _fileContent;

        public ArchiveAddonExtractor(Model.AddonList.Addon addon, byte[] fileContent)
        {
            _addon = addon;
            _fileContent = fileContent;
        }

        public bool ExtractTo(string path)
        {
            throw new NotImplementedException();
        }
    }
}
