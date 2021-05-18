using Gw2_AddonHelper.Model.AddonList;
using Gw2_AddonHelper.Model.GameState;
using Gw2_AddonHelper.Utility.Addon;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IAddonGameStateService
    {
        AddonContainer GetAddonInstallation(Addon addon);
        Task<bool> InstallAddon(AddonContainer addonContainer);
        Task<bool> RemoveAddon(AddonContainer addonContainer);
        Task<bool> EnableAddon(AddonContainer addonContainer);
        Task<bool> DisableAddon(AddonContainer addonContainer);
    }
}