using Microsoft.VisualStudio.Platform.WindowManagement;
using TeamCoding.Documents;

namespace TeamCoding.Extensions
{
    public static class DocumentViewExtensions
    {
        public static string GetRelatedFilePath(this DocumentView documentView)
        {
            var firstPipe = documentView.Name.IndexOf('|');
            var secondPipe = documentView.Name.IndexOf('|', firstPipe + 1);
            var thirdPipe = documentView.Name.IndexOf('|', secondPipe + 1);
            var fileName = documentView.Name.Substring(secondPipe + 1, thirdPipe - secondPipe - 1);
            return DocumentPaths.GetCorrectCase(fileName);
        }
    }
}