using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using TeamCoding.Extensions;
using LibGit2Sharp;
using System.IO;
using TeamCoding.SourceControl;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Linq;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using System.Windows.Controls;
using Microsoft.VisualStudio.Platform.WindowManagement;

namespace TeamCoding.VisualStudio
{
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class TeamCodingTextViewConnectionListener : IWpfTextViewConnectionListener
    {
        [Import]
        private readonly ITextDocumentFactoryService _TextDocFactory = null;
        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            if (reason == ConnectionReason.TextViewLifetime)
            { // TextView opened
                TeamCodingPackage.Current.IdeModel.OnOpenedTextView(textView);
                textView.TextBuffer.Changed += TextBuffer_Changed;
                ITextDocument textDoc;
                _TextDocFactory.TryGetTextDocument(textView.TextBuffer, out textDoc);

                if(textDoc != null)
                {
                    textDoc.FileActionOccurred += TextDoc_FileActionOccurred;
                }
            }
        }

        private void TextDoc_FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if(e.FileActionType == FileActionTypes.ContentSavedToDisk || e.FileActionType == FileActionTypes.DocumentRenamed)
            {
                TeamCodingPackage.Current.IdeModel.OnTextDocumentSaved(sender as ITextDocument, e);
            }
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            TeamCodingPackage.Current.IdeModel.OnTextBufferChanged(sender as ITextBuffer, e);
        }

        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            if (reason == ConnectionReason.TextViewLifetime)
            { // TextView closed
                TeamCodingPackage.Current.IdeModel.OnClosedTextView(textView);
                textView.TextBuffer.Changed -= TextBuffer_Changed;
            }
        }
    }
}
