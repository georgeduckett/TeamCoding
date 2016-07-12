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

                // TODO: Is there a better way to get the tab's full file path than parsing the tooltip?

                // TODO: Cache this (probably need to re-do cache when closing/opening a solution)
                var TabItems = GetWpfMainWindow(_DTE).FindChild<DocumentTabPanel>().FindChildren("TitlePanel").Cast<DockPanel>()
                                                     .Select(dp => new { TitlePanel = dp, TitleText = dp.FindChild<TabItemTextControl>() }).ToArray();
                
                foreach (var tabItem in TabItems)
                {
                    (tabItem.TitleText.DataContext as WindowFrameTitle).BindToolTip();
                }

                var TabItemsWithFilePaths = TabItems.Select(t => new { Item = t, File = (t.TitleText.DataContext as WindowFrameTitle).ToolTip }).ToArray();

                var RemoteOpenFiles = RemoteModelManager.GetExternalModels()
                    .SelectMany(model => model.OpenFiles.SelectMany(of => of.RepoUrls.Select(repo => new
                    {
                        Repo = repo,
                        Identity = model.IDEUserIdentity,
                        File = of.RelativePath
                    })));

                foreach (var tabItem in TabItemsWithFilePaths)
                {
                    var RepoInfo = new SourceControlRepo().GetRelativePath(tabItem.File);
                    var relativePath = RepoInfo.RelativePath;

                    var RemoteTabItems = RemoteOpenFiles.Where(rof => RepoInfo.RepoUrls.Contains(rof.Repo) && rof.File == RepoInfo.RelativePath).ToArray();

                    foreach(var remoteTabItem in RemoteTabItems)
                    {
                        if (remoteTabItem.Identity.ImageUrl == null)
                        {
                            var UserAppendString = $" [{remoteTabItem.Identity.DisplayName}]"; // TODO: Indicate whether they're editing or not
                            if (!tabItem.Item.TitleText.Text.Contains(UserAppendString))
                            {
                                tabItem.Item.TitleText.Text += UserAppendString;
                            }
                        }
                        else
                        {
                            if (!tabItem.Item.TitlePanel.Children.OfType<Image>().Any(i => (string)i.Tag == remoteTabItem.Identity.ImageUrl))
                            {
                                // TODO: Handle spotty internet connection?
                                // Insert the user image
                                var imgUser = ImageFromUrl(remoteTabItem.Identity.ImageUrl);
                                if (imgUser != null)
                                { // TODO: Add a generic icon with tooltip if we can't get the user (and until we async in the proper user image)
                                    imgUser.Width = (tabItem.Item.TitlePanel.Children[0] as GlyphButton).Width;
                                    imgUser.Height = (tabItem.Item.TitlePanel.Children[0] as GlyphButton).Height;
                                    imgUser.Margin = (tabItem.Item.TitlePanel.Children[0] as GlyphButton).Margin;
                                    imgUser.ToolTip = remoteTabItem.Identity.DisplayName; // TODO: Indicate whether they're editing or not
                                    imgUser.Tag = remoteTabItem.Identity.ImageUrl;

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
            // TODO: Make loading an image from a url async using the placeholder image below (make all Image controls reference this same resource, then when available change the image source)
            var image = new Image();
            using (MemoryStream stream = new MemoryStream(new System.Net.WebClient().DownloadData(url)))
            {
                // Could use BitmapFrame.DownloadCompleted event and the url directly
                image.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return image;
            }

            //return new Image() { Source = LoadBitmapFromResource("Resources/UnknownUserImage.png") };
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
