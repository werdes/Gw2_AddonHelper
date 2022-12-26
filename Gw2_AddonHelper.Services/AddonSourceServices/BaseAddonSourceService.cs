using Gw2_AddonHelper.Common.Model.AddonList;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AddonSourceServices
{
    public abstract class BaseAddonSourceService
    {
        protected ILogger _log;
        protected IConfiguration _config;

        /// <summary>
        /// Returns the key which the addon loader plugin uses
        /// </summary>
        /// <param name="addon"></param>
        /// <returns></returns>
        protected string GetLoaderKey(Addon addon)
        {
            string loaderKey = addon.AddonId;
            string loaderPrefix = _config.GetValue<string>("installation:binary:prefix");

            // Replace the gw2addon_-Prefix from Arc-Addons
            // Replace .dll-Suffix
            if (!string.IsNullOrWhiteSpace(addon.PluginName))
            {
                loaderKey = addon.PluginName.Replace(loaderPrefix, string.Empty);
                loaderKey = loaderKey.Replace(".dll", string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            string customConfigKey = $"customLoaderKeys:{addon.AddonId}";
            string customLoaderKey = _config.GetValue<string>(customConfigKey);
            if (!string.IsNullOrEmpty(customLoaderKey))
            {
                loaderKey = customLoaderKey;
            }


            _log.LogDebug($"Addon key for [{addon.AddonId}]: [{loaderKey}]");

            return loaderKey;
        }
    }
}
