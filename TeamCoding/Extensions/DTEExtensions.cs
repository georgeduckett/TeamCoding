using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class DTEExtensions
    {
        public static string GetWindowsFilePath(this Window window)
        {
            return window?.Document?.FullName.GetCorrectCaseOfParentFolder();
        }
    }
}
