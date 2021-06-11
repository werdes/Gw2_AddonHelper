using Gw2_AddonHelper.AddonLib.Model.UserConfig;

namespace Gw2_AddonHelper.AddonLib.Services.Interfaces
{
    public interface IUserConfigService
    {
        UserConfig GetConfig();
        void Load();
        void Store();
    }
}