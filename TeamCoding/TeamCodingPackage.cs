using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using TeamCoding.Documents;
using TeamCoding.Documents.SourceControlRepositories;
using TeamCoding.IdentityManagement;
using TeamCoding.Interfaces;
using TeamCoding.Interfaces.Documents;
using TeamCoding.Logging;
using TeamCoding.Options;
using TeamCoding.VisualStudio;
using TeamCoding.VisualStudio.Models;
using TeamCoding.VisualStudio.Models.ChangePersisters;
using TeamCoding.VisualStudio.Models.ChangePersisters.CombinedPersister;
using TeamCoding.VisualStudio.Models.ChangePersisters.DebugPersister;
using TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister;
using TeamCoding.VisualStudio.Models.ChangePersisters.RedisPersister;
using TeamCoding.VisualStudio.Models.ChangePersisters.SlackPersister;
using TeamCoding.VisualStudio.Models.ChangePersisters.SqlServerPersister;

namespace TeamCoding
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(Guids.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideOptionPage(typeof(OptionPageGrid), OptionPageGrid.OptionsName, "General", 0, 0, true)]
    public sealed class TeamCodingPackage : Package
    {
        public static TeamCodingPackage Current { get; private set; }
        [Export]
        public readonly Logger Logger = new Logger();
        public readonly UserImageCache UserImages = new UserImageCache();
        public readonly ObjectSlackMessageConverter ObjectSlackMessageConverter = new ObjectSlackMessageConverter();
        public SqlConnectionWrapper ConnectionWrapper;
        private uint SolutionEventsHandlerId;
        public HttpClient HttpClient { get; private set; }
        public ILocalModelPerisister LocalModelChangeManager { get; private set; }
        public IRemoteModelPersister RemoteModelChangeManager { get; private set; }
        public IDEWrapper IDEWrapper { get; private set; }
        public CachedSourceControlRepository SourceControlRepo { get; private set; }
        public IIdentityProvider IdentityProvider { get; private set; }
        public Settings Settings { get; private set; }
        public LocalIDEModel LocalIdeModel { get; private set; }
        public RedisWrapper Redis { get; private set; }
        public SlackWrapper Slack { get; private set; }
        public ICaretInfoProvider CaretInfoProvider { get; private set; }
        public ICaretAdornmentDataProvider CaretAdornmentDataProvider { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCodingPackage"/> class.
        /// </summary>
        public TeamCodingPackage()
        {
            Current = this;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName= new AssemblyName(args.Name);
            if (assemblyName.Name == "Newtonsoft.Json")
            {
                assemblyName.Version = new Version(9, 0);

                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

                var foundAssy = Assembly.Load(assemblyName);

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                return foundAssy;
            }

            return null;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            Logger.WriteInformation("Initializing");

            try
            {
                HttpClient = new HttpClient();
                HttpClient.DefaultRequestHeaders.Add("User-Agent",
                                                     "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)");
                var pSolution = (IVsSolution)GetService(typeof(SVsSolution));
                var result = pSolution.AdviseSolutionEvents(new SolutionEventsHandler(), out SolutionEventsHandlerId);
                Settings = new Settings();
                Redis = new RedisWrapper();
                Slack = new SlackWrapper();
                LocalIdeModel = new LocalIDEModel();
                IDEWrapper = new IDEWrapper((EnvDTE.DTE)GetService(typeof(EnvDTE.DTE)));


                var versionedAssembly = GetVersionedAssembly();
                CaretInfoProvider = GetVersionedType<ICaretInfoProvider>(versionedAssembly, "TeamCoding.Documents.CaretInfoProvider");
                CaretAdornmentDataProvider = GetVersionedType<ICaretAdornmentDataProvider>(versionedAssembly, "TeamCoding.Documents.CaretAdornmentDataProvider");
                SourceControlRepo = new CachedSourceControlRepository(new GitRepository(), GetVersionedType<ISourceControlRepository>(versionedAssembly, "TeamCoding.Documents.SourceControlRepositories.TeamFoundationServiceRepository"));
                IdentityProvider = new CachedFailoverIdentityProvider(new VSOptionsIdentityProvider(),
                                                                      new CredentialManagerIdentityProvider(new[] { "git:https://github.com", "https://github.com/" }),
                                                                      new VSIdentityProvider(),
                                                                      new MachineIdentityProvider());
                ConnectionWrapper = new SqlConnectionWrapper();
                RemoteModelChangeManager = new CombinedRemoteModelPersister(new RedisRemoteModelPersister(),
                                                                            new SharedFolderRemoteModelPersister(),
                                                                            new SlackRemoteModelPersister(),
                                                                            new SqlServerRemoteModelPersister(ConnectionWrapper));
                LocalModelChangeManager = new CombinedLocalModelPersister(new RedisLocalModelPersister(LocalIdeModel),
                                                                          new SharedFolderLocalModelPersister(LocalIdeModel),
                                                                          new SlackLocalModelPersister(LocalIdeModel),
                                                                          new SqlServerLocalModelPersister(ConnectionWrapper, LocalIdeModel));
                RemoteModelChangeManager.RemoteModelReceived += RemoteModelChangeManager_RemoteModelReceived;
            }
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
            {
                Logger.WriteError(ex);
            }
        }
        private T GetVersionedType<T>(Assembly versionedAssembly, string typeName)
        {
            return (T)Activator.CreateInstance(versionedAssembly.GetType(typeName));
        }
        private Assembly GetVersionedAssembly()
        {
            return Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(typeof(TeamCodingPackage).Assembly.Location), $"TeamCoding.v{GetMajorVsVersion()}.dll"));
        }
        private int GetMajorVsVersion()
        {
            var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            Version version;
            if (Version.TryParse(dte.Version, out version))
            {
                return version.Major;
            }
            return 15;
        }
        private void RemoteModelChangeManager_RemoteModelReceived(object sender, EventArgs e)
        {
            IDEWrapper.UpdateIDE();
        }
        protected override void Dispose(bool disposing)
        {
            if (SolutionEventsHandlerId != 0)
            {
                (GetService(typeof(SVsSolution)) as IVsSolution).UnadviseSolutionEvents(SolutionEventsHandlerId);
            }
            Redis?.Dispose();
            ConnectionWrapper?.Dispose();
            RemoteModelChangeManager?.Dispose();
            LocalModelChangeManager?.Dispose();
            HttpClient?.Dispose();
            base.Dispose(disposing);
        }
    }
}