using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Downloader
{
    public abstract class BaseAddonDownloader
    {
        private WebClient _webClient;
        protected ILogger<BaseAddonDownloader> _log;
        protected Model.AddonList.Addon _addon;

        protected WebClient WebClient
        {
            get => _webClient;
        }

        public BaseAddonDownloader(Model.AddonList.Addon addon)
        {
            _log = App.ServiceProvider.GetService<ILogger<BaseAddonDownloader>>();

            _webClient = new WebClient();
            _webClient.Headers.Add("user-agent",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name + " " +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());

            _addon = addon;
        }
    }
}
