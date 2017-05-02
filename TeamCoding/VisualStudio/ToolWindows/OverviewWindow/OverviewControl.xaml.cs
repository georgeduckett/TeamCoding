using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TeamCoding.Extensions;
using TeamCoding.IdentityManagement;
using TeamCoding.VisualStudio.Models;

namespace TeamCoding.VisualStudio.ToolWindows.OverviewWindow
{

    /// <summary>
    /// Interaction logic for OverviewControl.
    /// </summary>
    public partial class OverviewControl : UserControl
    {
        private class TypedDataContext
        {
            /// <summary>
            /// The identity of this user
            /// </summary>
            public IUserIdentity Identity { get; set; }
            public bool RemoteUserInvitedUs { get; set; }
            public bool RemoteUserAcceptedOurInvite { get; set; }
            public bool WeInvitedRemoteUser { get; set; }
            public bool WeAcceptedRemoteUserInvite { get; set; }
            /// <summary>
            /// This users avatar
            /// </summary>
            public Controls.UserAvatarModel UserAvatarModel { get; set; }
            /// <summary>
            /// The documents this user has open
            /// </summary>
            public Documents.IRemotelyAccessedDocumentData[] Documents { get; set; }
        }
        private HashSet<string> ExpandedItems = new HashSet<string>();
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
            var usersWhoSentAnInvite = TeamCodingPackage.Current.RemoteModelChangeManager.UserIdsWithSharedSessionInteractionsToLocalUser().ToArray();
            var localInvitesToRemoteUsers = TeamCodingPackage.Current.LocalIdeModel.SharedSessionInteractedUsers();

            tvUserDocs.DataContext = (from of in TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
                                      group of by of.IdeUserIdentity into ofg
                                      select new TypedDataContext
                                      {
                                          Identity = ofg.Key,
                                          RemoteUserInvitedUs = usersWhoSentAnInvite.Any(i => i.UserId == ofg.Key.Id &&
                                                                                              i.Interaction.HasFlag(SessionInteractions.Invite)),
                                          RemoteUserAcceptedOurInvite = usersWhoSentAnInvite.Any(i => i.UserId == ofg.Key.Id &&
                                                                                                      i.Interaction.HasFlag(SessionInteractions.Accept)),
                                          WeInvitedRemoteUser = localInvitesToRemoteUsers.Any(i => i.Key == ofg.Key.Id &&
                                                                                                   i.Value.HasFlag(SessionInteractions.Invite)),
                                          WeAcceptedRemoteUserInvite = localInvitesToRemoteUsers.Any(i => i.Key == ofg.Key.Id &&
                                                                                                          i.Value.HasFlag(SessionInteractions.Accept)),
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

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            ExpandedItems.Add((string)((TreeViewItem)sender).Tag);
        }

        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            ExpandedItems.Remove((string)((TreeViewItem)sender).Tag);
        }

        private void InviteUser_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (TypedDataContext)((MenuItem)e.Source).DataContext;

            TeamCodingPackage.Current.LocalIdeModel.ShareSessionWithUser(dataContext.Identity.Id);
        }
        private void UninviteUser_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (TypedDataContext)((MenuItem)e.Source).DataContext;

            TeamCodingPackage.Current.LocalIdeModel.CancelShareSessionWithUser(dataContext.Identity.Id);
        }
        private void AcceptInvite_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (TypedDataContext)((MenuItem)e.Source).DataContext;
            TeamCodingPackage.Current.LocalIdeModel.AcceptSessionInvite(dataContext.Identity.Id);
        }
        private void DeclineInvite_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (TypedDataContext)((MenuItem)e.Source).DataContext;
            TeamCodingPackage.Current.LocalIdeModel.DeclineSessionInvite(dataContext.Identity.Id);
        }
    }
}