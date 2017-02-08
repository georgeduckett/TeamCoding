using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
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
            else if (textBuffer is IProjectionBuffer ProjBuffer && ProjBuffer.SourceBuffers.Count == 1) // TODO: Handle multiple source buffers
            {
                return GetTextDocumentFilePath(ProjBuffer.SourceBuffers[0]);
            }

            return null;
        }
    }
}
