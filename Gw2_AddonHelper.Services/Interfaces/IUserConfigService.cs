using Gw2_AddonHelper.Services.UserConfigServices.Model;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IUserConfigService
    {
        UserConfig GetConfig();
        void Load();
        void Store();
    }
}