using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TeamCoding.SourceControl;
using TeamCoding.VisualStudio.Identity.UserImages;
using TeamCoding.Extensions;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Threading;

namespace TeamCoding.VisualStudio
{
    public class IDEWrapper
    {
        private Visual _WpfWindow;

        private readonly UserImageCache UserImages = new UserImageCache();
        private Visual WpfWindow => _WpfWindow;
        public Dispatcher UIDispatcher => _WpfWindow.Dispatcher;

        public IDEWrapper()
        {
            _WpfWindow = GetWpfMainWindow();
        }

        private Visual GetWpfMainWindow()
        {
            var DTE = TeamCodingPackage.Current.DTE;

            if (DTE == null)
            {
                throw new ArgumentNullException(nameof(DTE));
            }

            var hwndMainWindow = (IntPtr)DTE.MainWindow.HWnd;
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

        public void UpdateIDE(ExternalModelManager remoteModelManager)
        {
            remoteModelManager.SyncChanges();

            // TODO: Cache this (probably need to re-do cache when closing/opening a solution)
            var documentTabPanel = GetWpfMainWindow().FindChild<DocumentTabPanel>();

            if (documentTabPanel == null)
            { // We don't have a doc panel ATM (no docs are open)
                return;
            }

            var titlePanels = documentTabPanel.FindChildren("TitlePanel").Cast<DockPanel>();
            var tabItems = titlePanels.Select(dp => new { TitlePanel = dp, TitleText = dp.FindChild<TabItemTextControl>() }).ToArray();

            foreach (var tabItem in tabItems)
            {
                (tabItem.TitleText.DataContext as WindowFrameTitle).BindToolTip();
            }

            // TODO: Is there a better way to get the tab's full file path than parsing the tooltip? (there must be!)
            var tabItemsWithFilePaths = tabItems.Select(t => new { Item = t, File = (t.TitleText.DataContext as WindowFrameTitle).ToolTip.TrimEnd('*') }).ToArray();

            var remoteOpenFiles = TeamCodingPackage.Current.RemoteModelManager.GetOpenFiles();

            foreach (var tabItem in tabItemsWithFilePaths)
            {
                var repoInfo = new SourceControlRepo().GetRepoDocInfo(tabItem.File);
                if (repoInfo == null) continue;

                var relativePath = repoInfo.RelativePath;

                var remoteDocuments = remoteOpenFiles.Where(rof => repoInfo.RepoUrl == rof.Repository && rof.RelativePath == repoInfo.RelativePath).ToList();

                UpdateOrRemoveImages(tabItem.Item.TitlePanel, remoteDocuments);

                AddImages(tabItem.Item.TitlePanel, remoteDocuments);
            }
        }

        private void UpdateOrRemoveImages(DockPanel tabPanel, List<RemoteDocumentData> remoteDocuments)
        {
            foreach (var image in tabPanel.Children.OfType<Image>().ToArray())
            {
                var imageDocData = (RemoteDocumentData)image.Tag;

                var matchedRemoteDoc = remoteDocuments.SingleOrDefault(rd => rd.RelativePath == imageDocData.RelativePath &&
                                                                             rd.IdeUserIdentity.DisplayName == imageDocData.IdeUserIdentity.DisplayName);

                if (matchedRemoteDoc == null)
                {
                    image.Remove();
                }
                else
                {
                    if (imageDocData.BeingEdited != matchedRemoteDoc.BeingEdited)
                    {
                        imageDocData.BeingEdited = matchedRemoteDoc.BeingEdited;
                        UserImages.SetImageTooltip(image, matchedRemoteDoc);
                    }
                }
            }
        }

        private void AddImages(DockPanel tabPanel, List<RemoteDocumentData> remoteDocuments)
        {
            foreach (var remoteTabItem in remoteDocuments)
            {
                if (!tabPanel.Children.OfType<Image>().Any(i => (i.Tag as RemoteDocumentData).Equals(remoteTabItem)))
                {
                    var imgUser = UserImages.GetUserImageFromUrl(remoteTabItem.IdeUserIdentity.ImageUrl);

                    if (imgUser != null)
                    {
                        imgUser.Width = (tabPanel.Children[0] as GlyphButton).Width;
                        imgUser.Height = (tabPanel.Children[0] as GlyphButton).Height;
                        imgUser.Margin = (tabPanel.Children[0] as GlyphButton).Margin;
                        UserImages.SetImageTooltip(imgUser, remoteTabItem);
                        imgUser.Tag = remoteTabItem;

                        tabPanel.Children.Insert(tabPanel.Children.Count, imgUser);
                    }
                }
            }
        }
    }
}
