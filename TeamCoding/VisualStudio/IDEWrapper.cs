using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using System;
using System.Collections.Generic;
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
        private readonly UserImageCache UserImages;
        private readonly EnvDTE.WindowEvents WindowEvents;
        private readonly EnvDTE.SolutionEvents SolutionEvents;
        private readonly Visual WpfMainWindow;
        private readonly EnvDTE.DTE DTE;

        public IDEWrapper(EnvDTE.DTE dte)
        {
            DTE = dte;
            UserImages = new UserImageCache(this);
            WpfMainWindow = GetWpfMainWindow(dte);
            WindowEvents = dte.Events.WindowEvents;
            SolutionEvents = dte.Events.SolutionEvents;
            SolutionEvents.Opened += SolutionEvents_Opened;
            WindowEvents.WindowActivated += WindowEvents_WindowActivated;
            WindowEvents.WindowCreated += WindowEvents_WindowCreated;
        }
        private void SolutionEvents_Opened()
        {
            // It's ok that we're not saying that tabs without documents have been opened since if the document hasn't been opened the user isn't really looking at them yet anyway
            // We do want to update the IDE though because we need to show user icons on tabs even thought the associated document window hasn't been created yet
            UpdateIDE();
        }

        private void WindowEvents_WindowCreated(EnvDTE.Window window)
        {
            InvokeAsync(() =>
            {
                var filePath = window.GetWindowsFilePath();

                if (filePath == null) return;

                var documentTabPanel = WpfMainWindow.FindChild<DocumentTabPanel>();
                var titlePanels = documentTabPanel.FindChildren("TitlePanel").Cast<DockPanel>();
                var tabItems = titlePanels.Select(dp => new { TitlePanel = dp, TitleText = dp.FindChild<TabItemTextControl>() }).ToArray();

                var remoteOpenFiles = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles();

                var tabItemWithFilePath = tabItems.Select(t => new { Item = t, File = (t.TitleText.DataContext as WindowFrameTitle).GetRelatedFilePath() }).Single(t => t.File == filePath);

                UpdateTabImages(tabItemWithFilePath.Item.TitlePanel, filePath, remoteOpenFiles);
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
            // TODO: Pass a cancellation token so we can cancel when disposed. Dispose of this in the package dispose method
            WpfMainWindow.Dispatcher.InvokeAsync(UpdateIDE_Internal);
        }
        private void UpdateIDE_Internal()
        {
            // TODO: Cache this (probably need to re-do cache when closing/opening a solution)
            var documentTabPanel = WpfMainWindow.FindChild<DocumentTabPanel>();
            
            if (documentTabPanel == null)
            { // We don't have a doc panel ATM (no docs are open)
                return;
            }
            
            var titlePanels = documentTabPanel.FindChildren("TitlePanel").Cast<DockPanel>();
            var tabItems = titlePanels.Select(dp => new { TitlePanel = dp, TitleText = dp.FindChild<TabItemTextControl>() }).ToArray();

            var remoteOpenFiles = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles();

            var tabItemsWithFilePaths = tabItems.Select(t => new { Item = t, File = (t.TitleText.DataContext as WindowFrameTitle).GetRelatedFilePath() }).ToArray();

            foreach (var tabItem in tabItemsWithFilePaths)
            {
                UpdateTabImages(tabItem.Item.TitlePanel, tabItem.File, remoteOpenFiles);
            }
        }

        private void UpdateTabImages(DockPanel titlePanel, string filePath, IEnumerable<SourceControlledDocumentData> remoteOpenFiles)
        {
            var repoInfo = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(filePath);
            if (repoInfo == null) return;

            var relativePath = repoInfo.RelativePath;

            var remoteDocuments = remoteOpenFiles.Where(rof => repoInfo.RepoUrl == rof.Repository && rof.RelativePath == repoInfo.RelativePath).ToList();

            UpdateOrRemoveImages(titlePanel, remoteDocuments);

            AddImages(titlePanel, remoteDocuments);
        }

        private void UpdateOrRemoveImages(DockPanel tabPanel, List<SourceControlledDocumentData> remoteDocuments)
        {
            foreach (var userImageControl in tabPanel.Children.OfType<Panel>().Where(fe => fe.Tag is SourceControlledDocumentData).ToArray())
            {
                var imageDocData = (SourceControlledDocumentData)userImageControl.Tag;

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

                    if(imageDocData.HasFocus != matchedRemoteDoc.HasFocus)
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
                        UserImages.SetImageProperties(userImageControl, matchedRemoteDoc);
                    }
                }
            }
        }
        private void AddImages(DockPanel tabPanel, List<SourceControlledDocumentData> remoteDocuments)
        {
            foreach (var remoteTabItem in remoteDocuments)
            {
                if (!tabPanel.Children.OfType<Panel>().Where(fe => fe.Tag is SourceControlledDocumentData).Any(i => (i.Tag as SourceControlledDocumentData).Equals(remoteTabItem)))
                {
                    var imgUser = UserImages.GetUserImageControlFromUserIdentity(remoteTabItem.IdeUserIdentity, (int)(tabPanel.Children[0] as GlyphButton).Width);

                    if (imgUser != null)
                    {
                        imgUser.Width = (tabPanel.Children[0] as GlyphButton).Width;
                        imgUser.Height = (tabPanel.Children[0] as GlyphButton).Height;
                        imgUser.Margin = (tabPanel.Children[0] as GlyphButton).Margin;
                        UserImages.SetImageProperties(imgUser, remoteTabItem);
                        imgUser.Tag = remoteTabItem;

                        tabPanel.Children.Insert(tabPanel.Children.Count, imgUser);
                    }
                }
            }
        }
    }
}
