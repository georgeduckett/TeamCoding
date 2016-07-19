using System.IO;
using System.Linq;

namespace TeamCoding.Extensions
{
    public static class StringExtensions
    {
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
