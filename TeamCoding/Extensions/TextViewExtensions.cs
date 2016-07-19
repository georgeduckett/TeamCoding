using Microsoft.VisualStudio.Text.Editor;

namespace TeamCoding.Extensions
{
    public static class TextViewExtensions
    {
        public static string GetTextDocumentFilePath(this IWpfTextView textView)
        {
            return textView.TextBuffer.GetTextDocumentFilePath();
        }
    }
}
