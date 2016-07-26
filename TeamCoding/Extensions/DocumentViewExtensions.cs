using Microsoft.VisualStudio.Platform.WindowManagement;

namespace TeamCoding.Extensions
{
    public static class DocumentViewExtensions
    {
        public static string GetRelatedFilePath(this DocumentView documentView)
        {
            // TODO: Is there a better way to get the tab's full file path than parsing the documentview's name?
            var fileName = documentView.Name.Split('|')[2];
            return fileName.GetCorrectCaseOfParentFolder();
        }
    }
}
