//------------------------------------------------------------------------------
// <copyright file="TeamCodingPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using TeamCoding.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using TeamCoding.VisualStudio;
using TeamCoding.SourceControl;
using System.Windows.Interop;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using System.Linq;
using Microsoft.VisualStudio.Platform.WindowManagement;
using System.Windows.Threading;
using TeamCoding.VisualStudio.Identity;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TeamCoding
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(TeamCodingPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists)]
    public sealed class TeamCodingPackage : Package
    {
        /// <summary>
        /// TeamCodingPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "ac66efb2-fad5-442d-87e2-b9b4a206f14d";

        public static TeamCodingPackage Current { get; private set; }

        public readonly LocalIDEModel IdeModel = new LocalIDEModel();
        internal ModelChangeManager IdeChangeManager { get; private set; }
        public readonly IIdentityProvider IdentityProvider = new CachedGitHubIdentityProvider();
        public readonly ExternalModelManager RemoteModelManager = new ExternalModelManager();
        
        private EnvDTE.DTE _DTE => (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCodingPackage"/> class.
        /// </summary>
        public TeamCodingPackage()
        {
            Current = this;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            IdeChangeManager = new ModelChangeManager(IdeModel);
            
            DispatcherTimer Timer = new DispatcherTimer(DispatcherPriority.Normal, GetWpfMainWindow(_DTE).Dispatcher);

            Timer.Interval = TimeSpan.FromSeconds(10);

            Timer.Tick += Timer_Tick;

            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!Zombied)
            {
                // TODO: Make this react instantly to changes, rather than polling
                RemoteModelManager.SyncChanges();

                // TODO: Is there a better way to get the tab's full file path than parsing the tooltip?

                // TODO: Cache this (probably need to re-do cache when closing/opening a solution)
                var TabItems = GetWpfMainWindow(_DTE).FindChild<DocumentTabPanel>().FindChildren("TitlePanel").Cast<DockPanel>()
                                                     .Select(dp => new { TitlePanel = dp, TitleText = dp.FindChild<TabItemTextControl>() }).ToArray();
                
                foreach (var tabItem in TabItems)
                {
                    (tabItem.TitleText.DataContext as WindowFrameTitle).BindToolTip();
                }

                var TabItemsWithFilePaths = TabItems.Select(t => new { Item = t, File = (t.TitleText.DataContext as WindowFrameTitle).ToolTip }).ToArray();

                var RemoteOpenFiles = RemoteModelManager.GetExternalModels().SelectMany(m => m.OpenFiles.Select(of => new { OpenFile = of, m.IDEUserIdentity })).ToDictionary(g => g.OpenFile.RelativePath, g => g);

                foreach (var tabItem in TabItemsWithFilePaths)
                {
                    var relativePath = new SourceControlRepo().GetRelativePath(tabItem.File).RelativePath;
                    if (RemoteOpenFiles.ContainsKey(relativePath))
                    {
                        if (RemoteOpenFiles[relativePath].IDEUserIdentity.ImageUrl == null)
                        {
                            var UserAppendString = $" [{RemoteOpenFiles[relativePath].IDEUserIdentity.DisplayName}]";
                            if (!tabItem.Item.TitleText.Text.Contains(UserAppendString))
                            {
                                tabItem.Item.TitleText.Text += UserAppendString;
                            }
                        }
                        else
                        {
                            if (!tabItem.Item.TitlePanel.Children.OfType<Image>().Any(i => (string)i.ToolTip == RemoteOpenFiles[relativePath].IDEUserIdentity.DisplayName))
                            {
                                // TODO: Handle spotty internet connection?
                                // Insert the user image
                                var imgUser = ImageFromUrl(RemoteOpenFiles[relativePath].IDEUserIdentity.ImageUrl);
                                if (imgUser != null)
                                { // TODO: Add a generic icon with tooltip if we can't get the user (and until we async in the proper user image)
                                    imgUser.Width = (tabItem.Item.TitlePanel.Children[0] as GlyphButton).Width;
                                    imgUser.Height = (tabItem.Item.TitlePanel.Children[0] as GlyphButton).Height;
                                    imgUser.Margin = (tabItem.Item.TitlePanel.Children[0] as GlyphButton).Margin;
                                    imgUser.ToolTip = RemoteOpenFiles[relativePath].IDEUserIdentity.DisplayName;

                                    tabItem.Item.TitlePanel.Children.Insert(tabItem.Item.TitlePanel.Children.Count, imgUser);
                                }
                            }
                        }
                    }
                }
            }
        }

        private Image ImageFromUrl(string url)
        {
            // TODO: Make this async
            var wc = new System.Net.WebClient();
            var image = new Image();
            using (MemoryStream stream = new MemoryStream(wc.DownloadData(url)))
            {
                image.Source = BitmapFrame.Create(stream,
                                                  BitmapCreateOptions.None,
                                                  BitmapCacheOption.OnLoad);
                return image;
            }
        }

        protected override void Dispose(bool disposing)
        {
            RemoteModelManager.SyncChanges();
            base.Dispose(disposing);
        }
        private System.Windows.Media.Visual GetWpfMainWindow(EnvDTE.DTE dte)
        {
            if (dte == null)
            {
                throw new ArgumentNullException("dte");
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
    }
}
