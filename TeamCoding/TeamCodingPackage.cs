using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.InteropServices;
using TeamCoding.Documents;
using TeamCoding.IdentityManagement;
using TeamCoding.VisualStudio;
using TeamCoding.VisualStudio.Models.Local;
using TeamCoding.VisualStudio.Models.Remote;

namespace TeamCoding
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists)]
    public sealed class TeamCodingPackage : Package
    { // TODO: Test using the extension with no internet connection (the horror!)
        public const string PackageGuidString = "ac66efb2-fad5-442d-87e2-b9b4a206f14d";
        public static TeamCodingPackage Current { get; private set; }
        public readonly RemoteModelManager RemoteModelManager = new RemoteModelManager();
        public readonly LocalIDEModel LocalIdeModel = new LocalIDEModel();
        public readonly GitRepository SourceControlRepo = new GitRepository();
        public readonly HttpClient HttpClient;
        public LocalModelChangeManager LocalModelChangeManager { get; private set; }
        public IDEWrapper IDEWrapper { get; private set; }
        public RemoteModelChangeManager RemoteModelChangeManager { get; private set; }
        public IIdentityProvider IdentityProvider { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCodingPackage"/> class.
        /// </summary>
        public TeamCodingPackage()
        {
            Current = this;
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("User-Agent",
                                                 "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)");
        }
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            IDEWrapper = new IDEWrapper((EnvDTE.DTE)GetService(typeof(EnvDTE.DTE)));
            IdentityProvider = new CachedFailoverIdentityProvider(new CredentialManagerIdentityProvider(new[] { "git:https://github.com", "https://github.com/" }),
                                                                  new VSIdentityProvider(),
                                                                  new MachineIdentityProvider());
            LocalModelChangeManager = new LocalModelChangeManager(LocalIdeModel);

            RemoteModelChangeManager = new RemoteModelChangeManager(IDEWrapper, RemoteModelManager);
        }
        protected override void Dispose(bool disposing)
        {
            RemoteModelChangeManager.Dispose();
            LocalModelChangeManager.Dispose();
            HttpClient.Dispose();
            base.Dispose(disposing);
        }
    }
}