using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Concurrent;
using System.Linq;
using TeamCoding.Extensions;
using Microsoft.VisualStudio.Text;
using TeamCoding.Documents;
using System.Management;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using TeamCoding.Events;

namespace TeamCoding.VisualStudio.Models
{
    /// <summary>
    /// Represents and maintains a model of the IDE, firing change events as appropriate
    /// </summary>
    public class LocalIDEModel
    {
        /// <summary>
        /// A unique id for this IDE instance
        /// </summary>
        public static Lazy<string> Id = new Lazy<string>(() =>
        {
            string uuid = string.Empty;

            using (var mc = new ManagementClass("Win32_ComputerSystemProduct"))
            using (var moc = mc.GetInstances())
            {
                foreach (ManagementObject mo in moc)
                {
                    uuid = mo.Properties["UUID"].Value.ToString();
                    break;
                }
                return uuid + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            }
        }, false);
        /// <summary>
        /// Provides information for when a the file in focus changes
        /// </summary>
        public class FileFocusChangedEventArgs : EventArgs
        {
            public string LostFocusFile { get; set; }
            public string GotFocusFile { get; set; }
        }
        /// <summary>
        /// The curent IDE model instance
        /// </summary>
        private static LocalIDEModel Current = new LocalIDEModel();

        private readonly ConcurrentDictionary<string, DocumentRepoMetaData> OpenFiles = new ConcurrentDictionary<string, DocumentRepoMetaData>();

        private DelayedEvent OpenViewsChangedInternal = new DelayedEvent(500);
        /// <summary>
        /// Occurs when the IDE changed the views that are open (after a 500ms delay)
        /// </summary>
        public event EventHandler OpenViewsChanged { add { OpenViewsChangedInternal.Event += value; } remove { OpenViewsChangedInternal.Event -= value; } }

        private DelayedEvent<CaretPositionChangedEventArgs> CaretPositionChangedInternal = new DelayedEvent<CaretPositionChangedEventArgs>(500);
        /// <summary>
        /// Occurs when the caret position is changed (after a 500ms delay)
        /// </summary>
        public event EventHandler<CaretPositionChangedEventArgs> CaretPositionChanged { add { CaretPositionChangedInternal.Event += value; } remove { CaretPositionChangedInternal.Event -= value; } }
        
        private DelayedEvent<TextDocumentFileActionEventArgs> TextDocumentSavedInternal = new DelayedEvent<TextDocumentFileActionEventArgs>(500);
        public event EventHandler<TextDocumentFileActionEventArgs> TextDocumentSaved { add { TextDocumentSavedInternal.Event += value; } remove { TextDocumentSavedInternal.Event -= value; } }

        private DelayedEvent ModelChangedInternal = new DelayedEvent(500);
        /// <summary>
        /// Occurs when the IDE model is changed (after a 500ms delay)
        /// </summary>
        public event EventHandler ModelChanged { add { ModelChangedInternal.Event += value; } remove { ModelChangedInternal.Event -= value; } }
        public LocalIDEModel()
        {
            TeamCodingPackage.Current.Settings.UserSettings.UsernameChanged += (s, e) => OnUserIdentityChanged();
            TeamCodingPackage.Current.Settings.UserSettings.UserImageUrlChanged += (s, e) => OnUserIdentityChanged();

            // Hook up to the internal event as the model changed event is rate-limited anyway
            OpenViewsChangedInternal.PassthroughEvent += ModelChangedInternal.Invoke;
            CaretPositionChangedInternal.PassthroughEvent += ModelChangedInternal.Invoke;
            TextDocumentSavedInternal.PassthroughEvent += ModelChangedInternal.Invoke;
        }
        public async System.Threading.Tasks.Task OnCaretPositionChangedAsync(CaretPositionChangedEventArgs e)
        {
            var filePath = e.TextView.TextBuffer.GetTextDocumentFilePath();
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);
            sourceControlInfo.CaretPositionInfo = await TeamCodingPackage.Current.CaretInfoProvider.GetCaretInfoAsync(e.NewPosition.BufferPosition);
            if (sourceControlInfo != null)
            {
                OpenFiles.AddOrUpdate(filePath, sourceControlInfo, (v, d) => sourceControlInfo);
                
                OpenViewsChangedInternal?.Invoke(this, EventArgs.Empty);
            }
            CaretPositionChangedInternal?.Invoke(this, e);
        }
        public async System.Threading.Tasks.Task OnOpenedTextViewAsync(IWpfTextView view)
        {
            var filePath = view.GetTextDocumentFilePath();
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);

            sourceControlInfo.CaretPositionInfo = await TeamCodingPackage.Current.CaretInfoProvider.GetCaretInfoAsync(view.Caret.Position.BufferPosition);

            if (sourceControlInfo != null)
            {
                if (!OpenFiles.ContainsKey(filePath))
                {
                    OpenFiles.AddOrUpdate(filePath, sourceControlInfo, (v, e) => sourceControlInfo);
                }
                OpenViewsChangedInternal?.Invoke(this, EventArgs.Empty);
            }
        }

        internal void OnUserIdentityChanged()
        {
            OpenViewsChangedInternal?.Invoke(this, EventArgs.Empty);
        }

        internal void OnTextDocumentDisposed(ITextDocument textDocument, TextDocumentEventArgs e)
        {
            OpenFiles.TryRemove(textDocument.FilePath, out var tmp);

            OpenViewsChangedInternal?.Invoke(this, EventArgs.Empty);
        }

        internal void OnTextDocumentSaved(ITextDocument textDocument, TextDocumentFileActionEventArgs e)
        {
            TeamCodingPackage.Current.SourceControlRepo.RemoveCachedRepoData(textDocument.FilePath);
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(textDocument.FilePath);
            if (sourceControlInfo != null)
            {
                OpenFiles.AddOrUpdate(textDocument.FilePath, sourceControlInfo, (v, r) => { sourceControlInfo.CaretPositionInfo = r.CaretPositionInfo; return sourceControlInfo; });

                TextDocumentSavedInternal?.Invoke(textDocument, e);
            }
        }
        /// <summary>
        /// Gets an arrayt of currently open files
        /// </summary>
        /// <returns></returns>
        public DocumentRepoMetaData[] OpenDocs()
        {
            return OpenFiles.Values.ToArray();
        }

        internal void OnTextBufferChanged(ITextBuffer textBuffer, TextContentChangedEventArgs e)
        {
            var filePath = textBuffer.GetTextDocumentFilePath();
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);

            if (sourceControlInfo == null)
            {
                // The file could have just been put on the ignore list, so remove it from the list
                OpenFiles.TryRemove(filePath, out sourceControlInfo);
            }
            else
            {
                OpenFiles.AddOrUpdate(filePath, sourceControlInfo, (v, r) => { sourceControlInfo.CaretPositionInfo = r.CaretPositionInfo; return sourceControlInfo; });
            }
        }

        internal void OnFileGotFocus(string filePath)
        {
            // Update this source control info to update the time
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);
            if (sourceControlInfo != null)
            {
                OpenFiles.AddOrUpdate(filePath, sourceControlInfo, (v, r) => { sourceControlInfo.CaretPositionInfo = r.CaretPositionInfo; return sourceControlInfo; });

                OpenViewsChangedInternal?.Invoke(this, new EventArgs());
            }
        }

        internal void OnFileLostFocus(string filePath)
        {
        }
    }
}
