using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;
using TeamCoding.VisualStudio.Models;

namespace TeamCoding.VisualStudio.Models.ChangePersisters
{
    /// <summary>
    /// Manages receiving remote IDE model changes.
    /// </summary>
    public interface IRemoteModelPersister : IDisposable
    {
        event EventHandler RemoteModelReceived;
        IEnumerable<IRemotelyAccessedDocumentData> GetOpenFiles();
        IEnumerable<(string UserId, SessionInteractions Interaction)> UserIdsWithSharedSessionInteractionsToLocalUser();
    }
}
