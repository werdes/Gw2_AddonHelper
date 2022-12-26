using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AddonSourceServices
{
    public class HistoricLocalFileAddonSourceService : LocalFileAddonSourceService
    {
        private const int HIERARCHY = int.MaxValue;

        public override int GetHierarchy()
        {
            return HIERARCHY;
        }

        /// <summary>
        /// Tells wether the source is available
        /// here: - local files exist (write date ignored compared to LocalFileAddonSourceService)
        ///       - local files contain addons
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> IsAvailable()
        {
            string addonsFile = _config.GetValue<string>("addonSourceServices:localFile:addonsPath");
            string versionsFile = _config.GetValue<string>("addonSourceServices:localFile:versionsPath");
            
            if (_addonListContainer == null)
            {
                await InitAddonsFromService();
            }
            if (_versionContainer == null)
            {
                await InitVersionsFromService();
            }

            return File.Exists(addonsFile) &&
                   File.Exists(versionsFile) &&
                   _addonListContainer != null &&
                   _addonListContainer.Addons != null &&
                   _addonListContainer.Addons.Count > 0 &&
                   _versionContainer != null &&
                   _versionContainer.Versions != null &&
                   _versionContainer.Versions.Count > 0;
        }
    }
}
