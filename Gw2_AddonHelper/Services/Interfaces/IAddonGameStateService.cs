using Gw2_AddonHelper.AddonLib.Model.AddonList;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IAddonGameStateService
    {
        Task<IEnumerable<AddonContainer>> GetAddonContainers(IEnumerable<Addon> addons);
        Task<bool> InstallAddon(AddonContainer addonContainer);
        Task<bool> RemoveAddon(AddonContainer addonContainer);
        Task<bool> EnableAddon(AddonContainer addonContainer);
        Task<bool> DisableAddon(AddonContainer addonContainer);
        Task<bool> UpdateAddon(AddonContainer addonContainer);
        Task<bool> CheckAddonForUpdate(AddonContainer addonContainer);
        Task<IEnumerable<AddonContainer>> GetUpdateableAddons(IEnumerable<AddonContainer> installedAddons);
        Task GetParentAddons(List<AddonContainer> allAddons, AddonContainer addon, HashSet<AddonContainer> result);
        Task GetChildAddons(List<AddonContainer> allAddons, AddonContainer addon, HashSet<AddonContainer> result);
        Task<IEnumerable<AddonConflict>> CheckConflicts(IEnumerable<AddonContainer> installedAddons, IEnumerable<AddonContainer> newAddons);
    }
}