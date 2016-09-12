using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using TeamCoding.Documents;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio
{
    /// <summary>
    /// Wraps the Visual Studio IDE, reacts to dev environment events, updates the UI in response to external events
    /// </summary>
    public class IDEWrapper
    {
        public class DocumentSavedEventArgs : EventArgs
        {
            public string DocumentFilePath { get; set; }
        }
        private readonly UserImageCache UserImages;
        private readonly EnvDTE.WindowEvents WindowEvents;
        private readonly Visual WpfMainWindow;
        private readonly EnvDTE.DTE DTE;

        public IDEWrapper(EnvDTE.DTE dte)
        {
            DTE = dte;
            UserImages = TeamCodingPackage.Current.UserImages;
            WpfMainWindow = GetWpfMainWindow(dte);
            WindowEvents = dte.Events.WindowEvents;
            WindowEvents.WindowActivated += WindowEvents_WindowActivated;
            WindowEvents.WindowCreated += WindowEvents_WindowCreated;
        }
        private void WindowEvents_WindowCreated(EnvDTE.Window window)
        {
            InvokeAsync(() =>
            {
                var filePath = window.GetWindowsFilePath();

                if (filePath == null) return;

                var documentTabPanel = WpfMainWindow.FindChild<DocumentTabPanel>();
                var titlePanels = documentTabPanel.FindChildren("TitlePanel").Cast<DockPanel>();

                var remoteOpenFiles = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles();

                var tabItemWithFilePath = titlePanels.Select(t => new { Item = t, File = (t.DataContext as DocumentView).GetRelatedFilePath() }).Single(t => t.File == filePath);

                UpdateTabImages(tabItemWithFilePath.Item, filePath, remoteOpenFiles);
            });
        }

        private void WindowEvents_WindowActivated(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus)
        {
            var newFilePath = gotFocus.GetWindowsFilePath();

            if(newFilePath != null)
            {
                TeamCodingPackage.Current.LocalIdeModel.OnFileGotFocus(newFilePath);
            }

            var oldFilePath = lostFocus.GetWindowsFilePath();

            if (oldFilePath != null)
            {
                TeamCodingPackage.Current.LocalIdeModel.OnFileLostFocus(oldFilePath);
            }
        }
        public DispatcherOperation InvokeAsync(Action callback)
        {
            return WpfMainWindow.Dispatcher.InvokeAsync(callback, DispatcherPriority.ContextIdle);
        }
        private Visual GetWpfMainWindow(EnvDTE.DTE dte)
        {
            if (dte == null)
            {
                throw new ArgumentNullException(nameof(dte));
            }

            var hwndMainWindow = (IntPtr)dte.MainWindow.HWnd;
            if (hwndMainWindow == IntPtr.Zero)
            {
                throw new NullReferenceException("DTE.MainWindow.HWnd is null.");
            }

            var hwndSource = HwndSource.FromHwnd(hwndMainWindow);
            if (hwndSource == null)
            {
                throw new NullReferenceException("HwndSource for DTE.MainWindow is null.");
            }

            return hwndSource.RootVisual;
        }
        public void UpdateIDE()
        {
            WpfMainWindow.Dispatcher.InvokeAsync(UpdateIDE_Internal);
        }
        private void UpdateIDE_Internal()
        {
            try
            {
                var documentTabPanel = WpfMainWindow.FindChild<DocumentTabPanel>();

                if (documentTabPanel == null)
                { // We don't have a doc panel ATM (no docs are open)
                    return;
                }

                var remoteOpenFiles = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles();

                foreach (var titlePanel in documentTabPanel.FindChildren("TitlePanel").Cast<DockPanel>().Where(tp => tp.DataContext is DocumentView))
                {
                    UpdateTabImages(titlePanel, (titlePanel.DataContext as DocumentView).GetRelatedFilePath(), remoteOpenFiles);
                }
            }
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
            {
                TeamCodingPackage.Current.Logger.WriteError(ex);
            }
        }

        private void UpdateTabImages(DockPanel titlePanel, string filePath, IEnumerable<IRemotelyAccessedDocumentData> remoteOpenFiles)
        {
            var repoInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);
            if (repoInfo == null) return;

            var relativePath = repoInfo.RelativePath;

            var remoteDocuments = remoteOpenFiles.Where(rof => repoInfo.RepoUrl == rof.Repository && repoInfo.RepoBranch == rof.RepositoryBranch && rof.RelativePath == repoInfo.RelativePath).ToList();

            UpdateOrRemoveImages(titlePanel, remoteDocuments);

            AddImages(titlePanel, remoteDocuments);
        }

        private void UpdateOrRemoveImages(DockPanel tabPanel, List<IRemotelyAccessedDocumentData> remoteDocuments)
        {
            foreach (var userImageControl in tabPanel.Children.OfType<Panel>().Where(fe => fe.Tag is IRemotelyAccessedDocumentData).ToArray())
            {
                var imageDocData = (IRemotelyAccessedDocumentData)userImageControl.Tag;

                var matchedRemoteDoc = remoteDocuments.SingleOrDefault(rd => rd.RelativePath == imageDocData.RelativePath &&
                                                                             rd.IdeUserIdentity.Id == imageDocData.IdeUserIdentity.Id);

                if (matchedRemoteDoc == null || imageDocData.IdeUserIdentity.ImageUrl != matchedRemoteDoc.IdeUserIdentity.ImageUrl)
                {
                    // If we can't find an image control for this document, or the one that's there has a different image url then remove it
                    userImageControl.Remove();
                }
                else
                {
                    var PropertiesUpdated = false;
                    if (imageDocData.BeingEdited != matchedRemoteDoc.BeingEdited)
                    {
                        imageDocData.BeingEdited = matchedRemoteDoc.BeingEdited;
                        PropertiesUpdated = true;
                    }

                    if (imageDocData.CaretPositionInfo != matchedRemoteDoc.CaretPositionInfo)
                    {
                        imageDocData.CaretPositionInfo = matchedRemoteDoc.CaretPositionInfo;
                        PropertiesUpdated = true;
                    }

                    if (imageDocData.HasFocus != matchedRemoteDoc.HasFocus)
                    {
                        imageDocData.HasFocus = matchedRemoteDoc.HasFocus;
                        PropertiesUpdated = true;
                    }

                    if (imageDocData.IdeUserIdentity.DisplayName != matchedRemoteDoc.IdeUserIdentity.DisplayName)
                    {
                        imageDocData.IdeUserIdentity.DisplayName = matchedRemoteDoc.IdeUserIdentity.DisplayName;
                        PropertiesUpdated = true;
                    }

                    if (PropertiesUpdated)
                    {
                        UserImages.SetUserControlProperties(userImageControl, matchedRemoteDoc);
                    }
                }
            }
        }
        private void AddImages(DockPanel tabPanel, List<IRemotelyAccessedDocumentData> remoteDocuments)
        {
            foreach (var remoteTabItem in remoteDocuments)
            {
                if (!tabPanel.Children.OfType<Panel>().Where(fe => fe.Tag is IRemotelyAccessedDocumentData).Any(i => (i.Tag as IRemotelyAccessedDocumentData).Equals(remoteTabItem)))
                {
                    var imgUser = UserImages.CreateUserIdentityControl(remoteTabItem.IdeUserIdentity);

                    if (imgUser != null)
                    {
                        imgUser.Width = (tabPanel.Children[0] as GlyphButton).Width;
                        imgUser.Height = (tabPanel.Children[0] as GlyphButton).Height;
                        imgUser.Margin = (tabPanel.Children[0] as GlyphButton).Margin;
                        UserImages.SetUserControlProperties(imgUser, remoteTabItem);
                        imgUser.Tag = remoteTabItem;

                        tabPanel.Children.Insert(tabPanel.Children.Count, imgUser);
                    }
                }
            }
        }
    }
}
