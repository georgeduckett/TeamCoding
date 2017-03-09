using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCoding.Documents;

namespace TeamCoding.Extensions
{
    public static class TextBufferExtensions
    {
        public static string GetTextDocumentFilePath(this ITextBuffer textBuffer)
        {
            return GetTextDocumentFilePaths(textBuffer).FirstOrDefault();
        }
        public static IEnumerable<string> GetTextDocumentFilePaths(this ITextBuffer textBuffer)
        {
            if (textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDoc))
            {
                return new[] { DocumentPaths.GetFullPath(textDoc.FilePath) };
            }
            else if (textBuffer is IProjectionBufferBase ProjBuffer && ProjBuffer.SourceBuffers.Count != 0)
            {
                return GetTextDocumentFilePaths(ProjBuffer.SourceBuffers);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }
        public static IEnumerable<string> GetTextDocumentFilePaths(IEnumerable<ITextBuffer> textBuffers)
        {
            return textBuffers.Select(b => GetTextDocumentFilePath(b)).Distinct();
        }
    }
}
