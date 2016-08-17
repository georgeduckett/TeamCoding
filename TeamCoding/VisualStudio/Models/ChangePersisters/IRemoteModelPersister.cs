using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters
{
    /// <summary>
    /// Manages receiving remote IDE model changes.
    /// </summary>
    public interface IRemoteModelPersister : IDisposable
    {
        event EventHandler RemoteModelReceived;
        IEnumerable<RemotelyAccessedDocumentData> GetOpenFiles();
    }
}
