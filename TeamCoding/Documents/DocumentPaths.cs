using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Documents
{
    public static class DocumentPaths
    {
        private readonly static string PathSeperator = Path.DirectorySeparatorChar.ToString();
        private readonly static Dictionary<string, string> _GetFullPathCache = new Dictionary<string, string>();
        private readonly static Dictionary<string, string> _GetCorrectCaseCache = new Dictionary<string, string>();
        public static string GetFullPath(string partialPath)
        {
            if (_GetFullPathCache.ContainsKey(partialPath))
            {
                return _GetFullPathCache[partialPath];
            }
            else
            {
                var fullPath = Path.GetFullPath(partialPath);
                _GetFullPathCache.Add(partialPath, fullPath);

                return fullPath;
            }
        }
        public static string GetCorrectCase(string fileOrFolder)
        {
            if (_GetCorrectCaseCache.ContainsKey(fileOrFolder))
            {
                return _GetCorrectCaseCache[fileOrFolder];
            }

            var result = GetCorrectCaseInternal(fileOrFolder);
            _GetCorrectCaseCache.Add(fileOrFolder, result);
            return result;
        }
        private static string GetCorrectCaseInternal(string fileOrFolder)
        { // http://stackoverflow.com/a/29751774
            string myParentFolder = Path.GetDirectoryName(fileOrFolder);
            string myChildName = Path.GetFileName(fileOrFolder);
            if (ReferenceEquals(myParentFolder, null))
            {
                return fileOrFolder.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }).ToUpperInvariant();
            }

            if (Directory.Exists(myParentFolder))
            {
                string myFileOrFolder = Directory.GetFileSystemEntries(myParentFolder, myChildName).FirstOrDefault();
                if (!ReferenceEquals(myFileOrFolder, null))
                {
                    myChildName = Path.GetFileName(myFileOrFolder);
                }
            }
            return GetCorrectCase(myParentFolder) + PathSeperator + myChildName; // Don't use path.combine as it mucks up the casing
        }
    }
}
