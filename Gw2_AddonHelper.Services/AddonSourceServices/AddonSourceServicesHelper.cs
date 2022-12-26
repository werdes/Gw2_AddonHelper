using Gw2_AddonHelper.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AddonSourceServices
{
    public static class AddonSourceServicesHelper
    {
        /// <summary>
        /// Returns a list of possible addon sources in order to be tested for availability and valid result
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IAddonSourceService> GetAddonSourceServices()
        {
            List<IAddonSourceService> addonSourceServices = new List<IAddonSourceService>();

            addonSourceServices.Add(new GithubAddonSourceService());
            addonSourceServices.Add(new RepositoryMirrorAddonSourceService());
            addonSourceServices.Add(new LocalFileAddonSourceService());
            addonSourceServices.Add(new HistoricLocalFileAddonSourceService());
            addonSourceServices.Add(new GithubActionMirrorAddonSourceService());

            addonSourceServices = addonSourceServices.OrderBy(x => x.GetHierarchy()).ToList();
            return addonSourceServices;
        }

    }
}
