using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TeamCoding.Documents;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.ToolWindows.OverviewWindow
{

    /// <summary>
    /// Interaction logic for OverviewControl.
    /// </summary>
    public partial class OverviewControl : UserControl
    {
        private HashSet<string> ExpandedItems = new HashSet<string>();

        private HashSet<IRemotelyAccessedDocumentData> Documents;

        /// <summary>
        /// Initializes a new instance of the <see cref="OverviewControl"/> class.
        /// </summary>
        public OverviewControl()
        {
            InitializeComponent();

            TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived += RemoteModelChangeManager_RemoteModelReceived;
        }

        private void RemoteModelChangeManager_RemoteModelReceived(object sender, EventArgs e)
        {
            var newDocuments = new HashSet<IRemotelyAccessedDocumentData>(TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles(), OverviewWindowEqualityComparer.Instance);

            if (Documents == null || !(Documents.IsSubsetOf(newDocuments) && Documents.IsSupersetOf(newDocuments)))
            {
                Documents = newDocuments;

                tvUserDocs.DataContext = (from of in Documents
                                          group of by of.IdeUserIdentity into ofg
                                          select new
                                          {
                                              Identity = ofg.Key,
                                              UserAvatarModel = TeamCodingPackage.Current.UserImages.CreateUserAvatarModel(ofg.Key),
                                              Documents = ofg.ToArray()
                                          }).ToArray();

                var treeItems = tvUserDocs.FindChildren<TreeViewItem>();

                foreach (var node in treeItems)
                {
                    if (ExpandedItems.Contains((string)node.Tag))
                    {
                        node.IsExpanded = true;
                    }
                }
            }
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            ExpandedItems.Add((string)((TreeViewItem)sender).Tag);
        }

        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            ExpandedItems.Remove((string)((TreeViewItem)sender).Tag);
        }

        class OverviewWindowEqualityComparer : IEqualityComparer<IRemotelyAccessedDocumentData>
        {
            public static OverviewWindowEqualityComparer Instance { get; } = new OverviewWindowEqualityComparer();

            public bool Equals(IRemotelyAccessedDocumentData x, IRemotelyAccessedDocumentData y)
            {
                if (x == null ^ y == null)
                    return false;

                return x.Repository == y.Repository &&
                   x.RepositoryBranch == y.RepositoryBranch &&
                   x.RelativePath == y.RelativePath &&
                   x.IdeUserIdentity.Id == y.IdeUserIdentity.Id &&
                   x.BeingEdited == y.BeingEdited &&
                   x.HasFocus == y.HasFocus
                   ;
            }

            public int GetHashCode(IRemotelyAccessedDocumentData obj)
            {
                var hash = 17;
                hash = hash * 31 + obj.Repository.GetHashCode();

                if (!string.IsNullOrEmpty(obj.RepositoryBranch))
                    hash = hash * 31 + obj.RepositoryBranch.GetHashCode();

                hash = hash * 31 + obj.IdeUserIdentity.Id.GetHashCode();
                hash = hash * 31 + obj.BeingEdited.GetHashCode();
                hash = hash * 31 + obj.HasFocus.GetHashCode();

                return hash;
            }
        }
    }
}