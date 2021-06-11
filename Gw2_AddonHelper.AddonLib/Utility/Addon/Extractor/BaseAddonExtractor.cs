using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Extractor
{
    public abstract class BaseAddonExtractor
    {
        protected Model.AddonList.Addon _addon;
        protected ILogger<BaseAddonExtractor> _log;
        protected IConfiguration _config;

        public BaseAddonExtractor(Model.AddonList.Addon addon)
        {
            _config = Lib.ServiceProvider.GetService<IConfiguration>();
            _log = Lib.ServiceProvider.GetService<ILogger<BaseAddonExtractor>>();
            _addon = addon;
        }
    }
}
