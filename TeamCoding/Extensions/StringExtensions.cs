using System.IO;
using System.Linq;

namespace TeamCoding.Extensions
{
    public static class StringExtensions
    {
        private static string PathSeperator = Path.DirectorySeparatorChar.ToString();
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
                // TODO: Cache this, maybe handle folder re-names?
                string myFileOrFolder = Directory.GetFileSystemEntries(myParentFolder, myChildName).FirstOrDefault();
                if (!ReferenceEquals(myFileOrFolder, null))
                {
                    myChildName = Path.GetFileName(myFileOrFolder);
                }
            }
            return GetCorrectCaseOfParentFolder(myParentFolder) + PathSeperator + myChildName; // Don't use path.combine as it mucks up the casing
        }
    }
}
