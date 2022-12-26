using Gw2_AddonHelper.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AppUpdaterServices
{
    public static class AppUpdaterServicesHelper
    {
        private static List<IAppUpdaterService> GetAppUpdaterServices()
        {
            List<IAppUpdaterService> appUpdaterServices = new List<IAppUpdaterService>();

            appUpdaterServices.Add(new GithubAppUpdateService());
            appUpdaterServices.Add(new DummyAppUpdaterService());

            appUpdaterServices = appUpdaterServices.OrderBy(x => x.GetHierarchy()).ToList();
            return appUpdaterServices;
        }

        public static async Task<IAppUpdaterService> GetAppUpdaterService()
        {
            List<IAppUpdaterService> appUpdaterServices = GetAppUpdaterServices();
            IAppUpdaterService availableService = null;

            foreach (IAppUpdaterService appUpdaterService in appUpdaterServices)
            {
                if(await appUpdaterService.IsAvailable())
                {
                    availableService = appUpdaterService;
                    break;
                }
            }

            return availableService;

        }
    }
}
