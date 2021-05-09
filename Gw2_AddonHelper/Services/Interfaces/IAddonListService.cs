using Gw2_AddonHelper.Model;
using Gw2_AddonHelper.Model.AddonList;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IAddonListService
    {
        Task<List<Addon>> GetAddonsAsync();
        void Load();
        void Store();
    }
}