using Microsoft.VisualStudio.Platform.WindowManagement;

namespace TeamCoding.Extensions
{
    public static class DocumentViewExtensions
    {
        public static string GetRelatedFilePath(this DocumentView documentView)
        {
            var fileName = documentView.Name.Split('|')[2];
            return fileName.GetCorrectCaseOfParentFolder();
        }
    }
}
