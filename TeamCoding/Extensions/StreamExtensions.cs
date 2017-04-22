using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class StreamExtensions
    {
        public static string ReadAlltext(this Stream stream)
        {
            using(var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
