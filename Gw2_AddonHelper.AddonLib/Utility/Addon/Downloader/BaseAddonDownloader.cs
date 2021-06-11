using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader
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
            _log = Lib.ServiceProvider.GetService<ILogger<BaseAddonDownloader>>();

            _webClient = new WebClient();
            _webClient.Headers.Add("user-agent",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name + " " +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());

            _addon = addon;
        }
    }
}
