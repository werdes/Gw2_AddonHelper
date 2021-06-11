using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.AddonList;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Services.Interfaces
{
    public interface IAddonListService
    {
        Task<List<Addon>> GetAddonsAsync();
        void Load();
        void Store();
    }
}