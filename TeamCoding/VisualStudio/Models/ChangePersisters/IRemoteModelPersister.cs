using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters
{
    public interface IRemoteModelPersister : IDisposable
    {
        IEnumerable<SourceControlledDocumentData> GetOpenFiles();
    }
}
