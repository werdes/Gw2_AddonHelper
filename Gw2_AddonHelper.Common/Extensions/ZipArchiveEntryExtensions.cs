using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace Gw2_AddonHelper.Common.Extensions
{
    public static class ZipArchiveEntryExtensions
    {
        public static bool IsDirectory(this ZipArchiveEntry entry)
        {
            string name = entry.FullName;
            int nameLength = name.Length;
            bool result = (
                            (nameLength > 0) &&
                            (
                              (name[nameLength - 1] == '/') || 
                              (name[nameLength - 1] == '\\')
                            )
                          ) ||
                          entry.ExternalAttributes == 16;
            return result;
        }

        public static bool IsFile(this ZipArchiveEntry entry)
        {
            return !entry.IsDirectory() && entry.ExternalAttributes != 8;
        }
    }
}
