using Gw2_AddonHelper.Model.GameState;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Extractor
{
    public abstract class BaseAddonExtractor
    {
        protected Model.AddonList.Addon _addon;
        protected ILogger<BaseAddonExtractor> _log;
        protected IConfiguration _config;

        public BaseAddonExtractor(Model.AddonList.Addon addon)
        {
            _config = App.ServiceProvider.GetService<IConfiguration>();
            _log = App.ServiceProvider.GetService<ILogger<BaseAddonExtractor>>();
            _addon = addon;
        }
    }
}
