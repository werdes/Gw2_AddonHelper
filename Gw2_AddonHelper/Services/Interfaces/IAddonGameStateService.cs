using Gw2_AddonHelper.Model.AddonList;
using Gw2_AddonHelper.Model.GameState;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IAddonGameStateService
    {
        AddonInstallation GetAddonInstallation(Addon addon);
    }
}