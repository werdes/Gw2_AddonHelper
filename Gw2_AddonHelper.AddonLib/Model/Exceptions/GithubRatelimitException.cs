using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model.Exceptions
{
    public class GithubRatelimitException : Exception
    {
        public Model.AddonList.Addon Addon { get; set; }
        public GithubRatelimitException(Model.AddonList.Addon addon)
        {
            Addon = addon;
        }
    }
}
