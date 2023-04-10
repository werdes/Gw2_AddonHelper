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
        private static IAppUpdaterService _appUpdaterService;
        private static List<IAppUpdaterService> GetAppUpdaterServices()
        {
            List<IAppUpdaterService> appUpdaterServices = new List<IAppUpdaterService>();

            appUpdaterServices.Add(new LocalFileAppUpdaterService());
            appUpdaterServices.Add(new GithubAppUpdateService());
            appUpdaterServices.Add(new DummyAppUpdaterService());

            appUpdaterServices = appUpdaterServices.OrderBy(x => x.GetHierarchy()).ToList();
            return appUpdaterServices;
        }

        public static async Task<IAppUpdaterService> GetAppUpdaterService()
        {
            if (_appUpdaterService == null)
            {
                List<IAppUpdaterService> appUpdaterServices = GetAppUpdaterServices();

                foreach (IAppUpdaterService appUpdaterService in appUpdaterServices)
                {
                    if (await appUpdaterService.IsAvailable())
                    {
                        _appUpdaterService = appUpdaterService;
                        break;
                    }
                }

                return _appUpdaterService;
            }
            else return _appUpdaterService;
        }
    }
}
