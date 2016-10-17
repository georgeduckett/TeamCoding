using Microsoft.VisualStudio.Text;
using System.IO;

namespace TeamCoding.Extensions
{
    public static class TextBufferExtensions
    {
        public static string GetTextDocumentFilePath(this ITextBuffer textBuffer)
        {
            ITextDocument textDoc;
            if (textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDoc))
            {
                return Path.GetFullPath(textDoc.FilePath); // TODO: Cache this (maybe make a class to manage this stuff)
            }
            else
            {
                return null;
            }
        }
    }
}
