using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IAddonSourceService
    {
        /// <summary>
        /// Returns the position in call hierarchy (e.g. 0 = first, 1 = second, int.Max = last) 
        /// Multiple instances of different implementations will be created by the AddonSourceServiceHelper class, 
        /// ordered by this position and called in order until one is available (see IsAvailable())
        /// </summary>
        /// <returns></returns>
        int GetHierarchy();
        Task<bool> IsAvailable();
        Task<List<Addon>> GetAddonsAsync();
        Task<VersionContainer> GetVersions();
        Task<string> GetVersion();
        Task<DateTime> GetListTimestamp();
        Task<AddonListSource> GetSource();
    }
}
