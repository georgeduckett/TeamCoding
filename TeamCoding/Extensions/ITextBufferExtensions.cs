using Microsoft.VisualStudio.Text;
using System.IO;
using TeamCoding.Documents;

namespace TeamCoding.Extensions
{
    public static class TextBufferExtensions
    {
        public static string GetTextDocumentFilePath(this ITextBuffer textBuffer)
        {
            if (textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDoc))
            {
                return DocumentPaths.GetFullPath(textDoc.FilePath);
            }
            else
            {
                return null;
            }
        }
    }
}
