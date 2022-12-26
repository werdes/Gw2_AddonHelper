using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Common.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string GetMd5Hash(this byte[] bytes)
        {
            string stringHash = string.Empty;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                md5.TransformFinalBlock(bytes, 0, bytes.Length);
                byte[] hash = md5.Hash;
                hash.ForEach(x => stringHash += x.ToString("X2"));
            }
            return stringHash;
        }
    }
}
