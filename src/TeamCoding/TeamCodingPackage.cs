//------------------------------------------------------------------------------
// <copyright file="TeamCodingPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TeamCoding.Extensions;
using TeamCoding.SourceControl;
using TeamCoding.VisualStudio;
using TeamCoding.VisualStudio.Identity;

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

        private readonly Image SharedUnknownUserImage = new Image() { Source = LoadBitmapFromResource("Resources/UnknownUserImage.png") };

        private Dispatcher UIDispatcher;

        private EnvDTE.DTE _DTE => (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

        private readonly Dictionary<string, ImageSource> _UrlImages = new Dictionary<string, ImageSource>();

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

            UIDispatcher = GetWpfMainWindow(_DTE).Dispatcher;

            IdeChangeManager = new ModelChangeManager(IdeModel);
            
            DispatcherTimer Timer = new DispatcherTimer(DispatcherPriority.Normal, UIDispatcher);

            Timer.Interval = TimeSpan.FromSeconds(2);

            Timer.Tick += Timer_Tick;

            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!Zombied)
            {
                // TODO: Make this react instantly to changes, rather than polling
                RemoteModelManager.SyncChanges();
                
                // TODO: Cache this (probably need to re-do cache when closing/opening a solution)
                var TabItems = GetWpfMainWindow(_DTE).FindChild<DocumentTabPanel>().FindChildren("TitlePanel").Cast<DockPanel>()
                                                     .Select(dp => new { TitlePanel = dp, TitleText = dp.FindChild<TabItemTextControl>() }).ToArray();
                
                foreach (var tabItem in TabItems)
                {
                    (tabItem.TitleText.DataContext as WindowFrameTitle).BindToolTip();
                }

                // TODO: Is there a better way to get the tab's full file path than parsing the tooltip? (there must be!)
                var tabItemsWithFilePaths = TabItems.Select(t => new { Item = t, File = (t.TitleText.DataContext as WindowFrameTitle).ToolTip.TrimEnd('*') }).ToArray();

                var remoteOpenFiles = RemoteModelManager.GetExternalModels()
                    .SelectMany(model => model.OpenFiles.SelectMany(of => of.RepoUrls.Select(repo => new RemoteDocumentData()
                    {
                        Repository = repo,
                        IdeUserIdentity = model.IDEUserIdentity,
                        RelativePath = of.RelativePath,
                        BeingEdited = of.BeingEdited
                    })));

                foreach (var tabItem in tabItemsWithFilePaths)
                {
                    var repoInfo = new SourceControlRepo().GetRelativePath(tabItem.File);
                    var relativePath = repoInfo.RelativePath;

                    var remoteDocuments = remoteOpenFiles.Where(rof => repoInfo.RepoUrls.Contains(rof.Repository) && rof.RelativePath == repoInfo.RelativePath).ToArray();

                    // TODO: Don't just remove all images then add them in later
                    foreach (var image in tabItem.Item.TitlePanel.Children.OfType<Image>().ToArray())
                    {
                        image.Remove();
                    }

                    foreach (var remoteTabItem in remoteDocuments)
                    {
                        var imgUser = GetUserImageFromUrl(remoteTabItem.IdeUserIdentity.ImageUrl);

                        if (imgUser != null)
                        {
                            imgUser.Width = (tabItem.Item.TitlePanel.Children[0] as GlyphButton).Width;
                            imgUser.Height = (tabItem.Item.TitlePanel.Children[0] as GlyphButton).Height;
                            imgUser.Margin = (tabItem.Item.TitlePanel.Children[0] as GlyphButton).Margin;
                            imgUser.ToolTip = remoteTabItem.IdeUserIdentity.DisplayName + (remoteTabItem.BeingEdited ? " [edited]" : string.Empty);
                            imgUser.Tag = remoteTabItem;

                            tabItem.Item.TitlePanel.Children.Insert(tabItem.Item.TitlePanel.Children.Count, imgUser);
                        }
                    }
                }
            }
        }

        private Image GetUserImageFromUrl(string url)
        {
            if (url == null) { return new Image() { Source = SharedUnknownUserImage.Source }; }
            
            if (_UrlImages.ContainsKey(url))
            {
                return new Image() { Source = _UrlImages[url] };
            }

            var Result = new Image() { Source = SharedUnknownUserImage.Source };

            UIDispatcher.InvokeAsync(() =>
            {
                using (MemoryStream stream = new MemoryStream(new System.Net.WebClient().DownloadData(url)))
                {
                    Result.Source = _UrlImages[url] = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            });
            
            return Result;
        }

        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapFromResource(string pathInApplication)
        { // http://stackoverflow.com/a/9737958
            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }
            return new BitmapImage(new Uri(@"pack://application:,,,/" + System.Reflection.Assembly.GetCallingAssembly().GetName().Name + ";component/" + pathInApplication, UriKind.Absolute));
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
