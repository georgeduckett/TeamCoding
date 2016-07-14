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

namespace TeamCoding.VisualStudio.Models
{
    /// <summary>
    /// Represents and maintains a model of the IDE
    /// </summary>
    public class LocalIDEModel
    {
        private static LocalIDEModel Current = new LocalIDEModel();
        
        private readonly ConcurrentDictionary<ITextBuffer, SourceControlRepo.RepoDocInfo> OpenFiles = new ConcurrentDictionary<ITextBuffer, SourceControlRepo.RepoDocInfo>();

        public event EventHandler OpenViewsChanged;
        public event EventHandler<TextContentChangedEventArgs> TextContentChanged;
        public event EventHandler<TextDocumentFileActionEventArgs> TextDocumentSaved;

        public void OnOpenedTextView(IWpfTextView view)
        {
            var filePath = view.GetTextDocumentFilePath();
            var sourceControlInfo = new SourceControlRepo().GetRepoDocInfo(filePath);
            if (!OpenFiles.ContainsKey(view.TextBuffer) && sourceControlInfo != null)
            {
                // TODO: maybe use https://msdn.microsoft.com/en-us/library/envdte.sourcecontrol.aspx to check if it's in source control
                
                OpenFiles.AddOrUpdate(view.TextBuffer, sourceControlInfo, (v, e) => e);
                OpenViewsChanged?.Invoke(this, new EventArgs());
            }
        }

        public void OnClosedTextView(IWpfTextView view)
        {
            SourceControlRepo.RepoDocInfo tmp;
            
            OpenFiles.TryRemove(view.TextBuffer, out tmp);
            OpenViewsChanged?.Invoke(this, new EventArgs());
        }

        public SourceControlRepo.RepoDocInfo[] OpenDocs()
        {
            return OpenFiles.Values.ToArray();
        }

        internal void OnTextBufferChanged(ITextBuffer textBuffer, TextContentChangedEventArgs e)
        {
            var sourceControlInfo = new SourceControlRepo().GetRepoDocInfo(textBuffer.GetTextDocumentFilePath());
            if (sourceControlInfo == null)
            {
                // The file could have just been put on the ignore list, so remove it from the list
                OpenFiles.TryRemove(textBuffer, out sourceControlInfo);
            }
            else
            {
                OpenFiles.AddOrUpdate(textBuffer, sourceControlInfo, (v, r) => sourceControlInfo);
            }

            TextContentChanged?.Invoke(textBuffer, e);
        }

        internal void OnTextDocumentSaved(ITextDocument textDocument, TextDocumentFileActionEventArgs e)
        {
            var sourceControlInfo = new SourceControlRepo().GetRepoDocInfo(textDocument.FilePath);
            OpenFiles.AddOrUpdate(textDocument.TextBuffer, sourceControlInfo, (v, r) => sourceControlInfo);

            TextDocumentSaved?.Invoke(textDocument, e);
        }
    }
}
