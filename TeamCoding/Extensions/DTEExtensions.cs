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
        public static string GetCorrectCaseOfParentFolder(this string fileOrFolder)
        { // http://stackoverflow.com/a/29751774
            string myParentFolder = Path.GetDirectoryName(fileOrFolder);
            string myChildName = Path.GetFileName(fileOrFolder);
            if (ReferenceEquals(myParentFolder, null))
            {
                return fileOrFolder.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }).ToUpperInvariant();
            }

            if (Directory.Exists(myParentFolder))
            {
                //myParentFolder = GetLongPathName.Invoke(myFullName);
                string myFileOrFolder = Directory.GetFileSystemEntries(myParentFolder, myChildName).FirstOrDefault();
                if (!ReferenceEquals(myFileOrFolder, null))
                {
                    myChildName = Path.GetFileName(myFileOrFolder);
                }
            }
            return GetCorrectCaseOfParentFolder(myParentFolder) + Path.DirectorySeparatorChar + myChildName;
        }
    }
}
