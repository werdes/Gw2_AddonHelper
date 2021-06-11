using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Extensions
{
    public static class UriExtensions
    {
        public static string GetDirectory(this Uri uri)
        {
            return Path.GetDirectoryName(uri.LocalPath);
        }
    }
}
