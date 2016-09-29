//------------------------------------------------------------------------------
// <copyright file="OverviewControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace TeamCoding.VisualStudio.ToolWindows.Overview
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for OverviewControl.
    /// </summary>
    public partial class OverviewControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverviewControl"/> class.
        /// </summary>
        public OverviewControl()
        {
            InitializeComponent();

            //TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived += RemoteModelChangeManager_RemoteModelReceived;
            //RemoteModelChangeManager_RemoteModelReceived(this, EventArgs.Empty);
        }

        private void RemoteModelChangeManager_RemoteModelReceived(object sender, EventArgs e)
        {
            tvUserDocuments.DataContext = (from of in TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
                                           group of by of.IdeUserIdentity into ofg
                                           select new { Identity = ofg.Key, Documents = ofg.ToArray() }).ToArray();
        }
        
    }
}