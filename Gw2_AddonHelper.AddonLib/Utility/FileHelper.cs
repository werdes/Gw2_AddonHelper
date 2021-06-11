using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility
{
    public static class FileHelper
    {
        public static string GetFileHashMd5(string path)
        {
            using(MD5 md5 = MD5.Create())
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    byte[] hash = md5.ComputeHash(fileStream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static string GetFileHashMd5(byte[] fileContent)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (MemoryStream stream = new MemoryStream(fileContent))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
