using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model.Exceptions
{
    public class GithubRatelimitException : Exception
    {
        public Common.Model.AddonList.Addon Addon { get; set; }
        public GithubRatelimitException(Common.Model.AddonList.Addon addon)
        {
            Addon = addon;
        }
    }
}
