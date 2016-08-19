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
using Microsoft.CodeAnalysis.Text;

namespace TeamCoding.VisualStudio.Models
{
    /// <summary>
    /// Represents and maintains a model of the IDE, firing change events as appropriate
    /// </summary>
    public class LocalIDEModel
    {
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

        public class FileFocusChangedEventArgs : EventArgs
        {
            public string LostFocusFile { get; set; }
            public string GotFocusFile { get; set; }
        }

        private static LocalIDEModel Current = new LocalIDEModel();

        private object OpenFilesLock = new object();
        private readonly ConcurrentDictionary<string, DocumentRepoMetaData> OpenFiles = new ConcurrentDictionary<string, DocumentRepoMetaData>();

        public event EventHandler OpenViewsChanged;
        public event EventHandler<CaretPositionChangedEventArgs> CaretPositionChanged;
        public event EventHandler<TextContentChangedEventArgs> TextContentChanged;
        public event EventHandler<TextDocumentFileActionEventArgs> TextDocumentSaved;

        public LocalIDEModel()
        {
            TeamCodingPackage.Current.Settings.UserSettings.UsernameChanged += (s, e) => OnUserIdentityChanged();
            TeamCodingPackage.Current.Settings.UserSettings.UserImageUrlChanged += (s, e) => OnUserIdentityChanged();
        }
        public async System.Threading.Tasks.Task OnCaretPositionChanged(CaretPositionChangedEventArgs e)
        {
            var filePath = e.TextView.TextBuffer.GetTextDocumentFilePath();
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);
            sourceControlInfo.CaretMemberHashCode = await GetMemberHashCode(e.NewPosition.BufferPosition);
            if (sourceControlInfo != null)
            {
                lock (OpenFilesLock)
                {
                    OpenFiles.AddOrUpdate(filePath, sourceControlInfo, (v, d) => sourceControlInfo);
                }
                // TODO: Only trigger if the caret member actually changed
                OpenViewsChanged?.Invoke(this, EventArgs.Empty);
            }
            CaretPositionChanged?.Invoke(this, e);
        }

        private static async System.Threading.Tasks.Task<int?> GetMemberHashCode(SnapshotPoint snapshotPoint)
        {
            var syntaxRoot = await snapshotPoint.Snapshot.GetOpenDocumentInCurrentContextWithChanges().GetSyntaxRootAsync();
            var caretToken = syntaxRoot.FindToken(snapshotPoint);
            int? memberHashCode = null;
            switch (caretToken.Language)
            {
                case "C#":
                    memberHashCode = caretToken.Parent
                                               .AncestorsAndSelf()
                                               .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax>()
                                               .FirstOrDefault()
                                               ?.GetTreePositionHashCode();
                    break;
                default:
                    TeamCodingPackage.Current.Logger.WriteInformation($"Document with unsupported language found: {caretToken.Language}"); break;
            }

            return memberHashCode;
        }

        public async System.Threading.Tasks.Task OnOpenedTextView(IWpfTextView view)
        {
            var filePath = view.GetTextDocumentFilePath();
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);
            sourceControlInfo.CaretMemberHashCode = await GetMemberHashCode(view.Caret.Position.BufferPosition);

            if (sourceControlInfo != null)
            {
                lock (OpenFilesLock)
                {
                    if (!OpenFiles.ContainsKey(filePath))
                    {
                        // TODO: maybe use https://msdn.microsoft.com/en-us/library/envdte.sourcecontrol.aspx to check if it's in source control

                        OpenFiles.AddOrUpdate(filePath, sourceControlInfo, (v, e) => sourceControlInfo);
                    }
                }
                OpenViewsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        internal void OnUserIdentityChanged()
        {
            OpenViewsChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void OnTextDocumentDisposed(ITextDocument textDocument, TextDocumentEventArgs e)
        {
            DocumentRepoMetaData tmp;
            lock (OpenFilesLock)
            {
                OpenFiles.TryRemove(textDocument.FilePath, out tmp);
            }
            OpenViewsChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void OnTextDocumentSaved(ITextDocument textDocument, TextDocumentFileActionEventArgs e)
        {
            TeamCodingPackage.Current.SourceControlRepo.RemoveCachedRepoData(textDocument.FilePath);
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(textDocument.FilePath);
            if (sourceControlInfo != null)
            {
                lock (OpenFilesLock)
                {
                    OpenFiles.AddOrUpdate(textDocument.FilePath, sourceControlInfo, (v, r) => { sourceControlInfo.CaretMemberHashCode = r.CaretMemberHashCode; return sourceControlInfo; });
                }

                TextDocumentSaved?.Invoke(textDocument, e);
            }
        }

        public DocumentRepoMetaData[] OpenDocs()
        {
            lock (OpenFilesLock)
            {
                return OpenFiles.Values.ToArray();
            }
        }

        internal void OnTextBufferChanged(ITextBuffer textBuffer, TextContentChangedEventArgs e)
        {
            var filePath = textBuffer.GetTextDocumentFilePath();
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);

            lock (OpenFilesLock)
            {
                if (sourceControlInfo == null)
                {
                    // The file could have just been put on the ignore list, so remove it from the list
                    OpenFiles.TryRemove(filePath, out sourceControlInfo);
                }
                else
                {
                    OpenFiles.AddOrUpdate(filePath, sourceControlInfo, (v, r) => { sourceControlInfo.CaretMemberHashCode = r.CaretMemberHashCode; return sourceControlInfo; });
                }
            }
            
            TextContentChanged?.Invoke(textBuffer, e);
        }

        internal void OnFileGotFocus(string filePath)
        {
            // Update this source control info to update the time
            var sourceControlInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);
            if (sourceControlInfo != null)
            {
                lock (OpenFilesLock)
                {
                    OpenFiles.AddOrUpdate(filePath, sourceControlInfo, (v, r) => { sourceControlInfo.CaretMemberHashCode = r.CaretMemberHashCode; return sourceControlInfo; });
                }
                OpenViewsChanged?.Invoke(this, new EventArgs());
            }
        }

        internal void OnFileLostFocus(string filePath)
        {
        }
    }
}
