using Gw2_AddonHelper.AddonLib.Model.AddonList;
using Gw2_AddonHelper.Common.Model.AddonList;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IAddonListService
    {
        Task<List<Addon>> GetAddonsAsync();
        Task Load();
        Task Store();
        Task<string> GetListVersion();
        Task<DateTime> GetListTimestamp();
        Task<VersionContainer> GetVersions();
    }
}