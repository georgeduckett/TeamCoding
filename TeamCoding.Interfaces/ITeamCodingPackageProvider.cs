using System.Net.Http;
using TeamCoding.Documents.SourceControlRepositories;
using TeamCoding.IdentityManagement;
using TeamCoding.Interfaces.Documents;
using TeamCoding.Logging;
using TeamCoding.VisualStudio;
using TeamCoding.VisualStudio.Models;
using TeamCoding.VisualStudio.Models.ChangePersisters;

namespace TeamCoding
{
    public interface ITeamCodingPackageProvider
    {
        ICaretAdornmentDataProvider CaretAdornmentDataProvider { get; }
        ICaretInfoProvider CaretInfoProvider { get; }
        HttpClient HttpClient { get; }
        IRemoteModelPersister RemoteModelChangeManager { get; }
        ILogger Logger { get; }
    }
}