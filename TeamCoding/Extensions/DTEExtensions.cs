using EnvDTE;

namespace TeamCoding.Extensions
{
    public static class DTEExtensions
    {
        public static string GetWindowsFilePath(this Window window)
        {
            return window?.Document?.GetWindowsFilePath();
        }
        public static string GetWindowsFilePath(this Document document)
        {
            return document.FullName.GetCorrectCaseOfParentFolder();
        }
    }
}
