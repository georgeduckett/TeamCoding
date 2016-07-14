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
using TeamCoding.VisualStudio.Identity.UserImages;
using TeamCoding.VisualStudio.Models;

namespace TeamCoding
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
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
        public IDEWrapper IDEWrapper;
        
        public EnvDTE.DTE DTE => (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

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

            IDEWrapper = new IDEWrapper();
            IdeChangeManager = new ModelChangeManager(IdeModel);
            
            var timer = new DispatcherTimer(DispatcherPriority.Normal, IDEWrapper.UIDispatcher);
            // TODO: Make this react instantly to changes, rather than polling
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!Zombied)
            {
                IDEWrapper.UpdateIDE(RemoteModelManager);
            }
        }
        protected override void Dispose(bool disposing)
        {
            RemoteModelManager.SyncChanges();
            base.Dispose(disposing);
        }
    }
}