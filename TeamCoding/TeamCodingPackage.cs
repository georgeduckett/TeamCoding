using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using TeamCoding.Models;
using TeamCoding.VisualStudio;
using TeamCoding.VisualStudio.Identity;
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
        public LocalModelChangeManager IdeChangeManager { get; private set; }
        public readonly IIdentityProvider IdentityProvider = new CachedGitHubIdentityProvider();
        public readonly ExternalModelManager RemoteModelManager = new ExternalModelManager();
        public IDEWrapper IDEWrapper;
        public RemoteModelChangeManager RemoteModelChangeManager;
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

            IDEWrapper = new IDEWrapper((EnvDTE.DTE)GetService(typeof(EnvDTE.DTE)));
            IdeChangeManager = new LocalModelChangeManager(IdeModel);

            RemoteModelChangeManager = new RemoteModelChangeManager(IDEWrapper, RemoteModelManager);
        }
        protected override void Dispose(bool disposing)
        {
            RemoteModelChangeManager.Dispose();
            IdeChangeManager.Dispose();
            base.Dispose(disposing);
        }
    }
}