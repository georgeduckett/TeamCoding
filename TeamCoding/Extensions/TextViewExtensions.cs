using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;

namespace TeamCoding.Extensions
{
    public static class TextViewExtensions
    {
        public static string GetTextDocumentFilePath(this IWpfTextView textView)
        {
            return textView.TextBuffer.GetTextDocumentFilePath();
        }
        public static IEnumerable<string> GetTextDocumentFilePaths(this IWpfTextView textView)
        {
            return textView.TextBuffer.GetTextDocumentFilePaths();
        }
    }
}
