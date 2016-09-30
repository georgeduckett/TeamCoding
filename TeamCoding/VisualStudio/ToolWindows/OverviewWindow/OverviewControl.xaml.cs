using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.ToolWindows.OverviewWindow
{

    /// <summary>
    /// Interaction logic for OverviewControl.
    /// </summary>
    public partial class OverviewControl : UserControl
    {
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
            tvUserDocs.DataContext = (from of in TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
                                      group of by of.IdeUserIdentity into ofg
                                      select new { Identity = ofg.Key, Documents = ofg.ToArray() }).ToArray();

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
    }
}