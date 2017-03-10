using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Interfaces.Extensions
{
    public static class StringExtensions
    {
        public static int ToIntegerCode(this string str, bool ignoreCase = false)
        {
            using (var md5provider = new MD5CryptoServiceProvider())
            {
                if (ignoreCase)
                    str = str.ToLowerInvariant();

                var bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(str));
                var integer = BitConverter.ToInt32(bytes, 0);
                return integer;
            }
        }
    }
}
