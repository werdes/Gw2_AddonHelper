using Gw2_AddonHelper.Model.UserConfig;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IUserConfigService
    {
        UserConfig GetConfig();
        void Load();
        void Store();
    }
}