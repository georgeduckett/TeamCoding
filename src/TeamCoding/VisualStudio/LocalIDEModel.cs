using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;
using TeamCoding.SourceControl;
using Microsoft.VisualStudio.Text;

namespace TeamCoding.VisualStudio
{
    /// <summary>
    /// Represents and maintains a model of the IDE
    /// </summary>
    public class LocalIDEModel
    {
        private static LocalIDEModel _Current = new LocalIDEModel();
        
        private readonly ConcurrentDictionary<string, SourceControlRepo.RepoDocInfo> _OpenFiles = new ConcurrentDictionary<string, SourceControlRepo.RepoDocInfo>();

        public event EventHandler OpenViewsChanged;
        public event EventHandler<TextContentChangedEventArgs> TextContentChanged;
        public event EventHandler<TextDocumentFileActionEventArgs> TextDocumentSaved;

        public void OnOpenedTextView(IWpfTextView view)
        {
            var filePath = view.GetTextDocumentFilePath();
            if (!_OpenFiles.ContainsKey(filePath))
            {
                // TODO: Use https://msdn.microsoft.com/en-us/library/envdte.sourcecontrol.aspx to check if it's in source control
                _OpenFiles.AddOrUpdate(filePath, new SourceControl.SourceControlRepo().GetRelativePath(filePath), (v, e) => e);
                OpenViewsChanged?.Invoke(this, new EventArgs());
            }
        }

        public void OnClosedTextView(IWpfTextView view)
        {
            SourceControl.SourceControlRepo.RepoDocInfo tmp;
            _OpenFiles.TryRemove(view.GetTextDocumentFilePath() ?? string.Empty, out tmp);
            OpenViewsChanged?.Invoke(this, new EventArgs());
        }

        public SourceControlRepo.RepoDocInfo[] OpenDocs()
        {
            return _OpenFiles.Values.ToArray();
        }

        internal void OnTextBufferChanged(ITextBuffer textBuffer, TextContentChangedEventArgs e)
        {
            // TODO: *Check it marks edits straight away
            var SourceControlInfo = new SourceControlRepo().GetRelativePath(textBuffer.GetTextDocumentFilePath());
            _OpenFiles.AddOrUpdate(textBuffer.GetTextDocumentFilePath(), SourceControlInfo, (v, r) => SourceControlInfo);

            TextContentChanged?.Invoke(textBuffer, e);
        }

        internal void OnTextDocumentSaved(ITextDocument textDocument, TextDocumentFileActionEventArgs e)
        {
            TextDocumentSaved?.Invoke(textDocument, e);
        }
    }
}
