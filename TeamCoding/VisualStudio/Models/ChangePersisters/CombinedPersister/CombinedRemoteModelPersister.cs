using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.CombinedPersister
{
    public class CombinedRemoteModelPersister : IRemoteModelPersister
    {
        private readonly IRemoteModelPersister[] RemoteModelPersisters;
        public event EventHandler RemoteModelReceived
        {
            add
            {
                foreach (var remoteModelPersister in RemoteModelPersisters)
                {
                    remoteModelPersister.RemoteModelReceived += value;
                }
            }
            remove
            {
                foreach (var remoteModelPersister in RemoteModelPersisters)
                {
                    remoteModelPersister.RemoteModelReceived -= value;
                }
            }
        }

        public IEnumerable<RemotelyAccessedDocumentData> GetOpenFiles() => RemoteModelPersisters.SelectMany(rmp => rmp.GetOpenFiles().ToArray()).GroupBy(scdd => new
        {
            scdd.Repository,
            scdd.RepositoryBranch,
            scdd.RelativePath,
            scdd.IdeUserIdentity.Id
        }).Select(g => new RemotelyAccessedDocumentData()
        {
            Repository = g.Key.Repository,
            RepositoryBranch = g.Key.RepositoryBranch,
            RelativePath = g.Key.RelativePath,
            IdeUserIdentity = g.First().IdeUserIdentity,
            HasFocus = g.Any(scdd => scdd.HasFocus),
            BeingEdited = g.Any(scdd => scdd.BeingEdited),
            CaretMemberHashCode = g.FirstOrDefault(scdd => scdd.CaretMemberHashCode != null)?.CaretMemberHashCode
        }).ToArray();
        public CombinedRemoteModelPersister(params IRemoteModelPersister[] remoteModelPersisters)
        {
            RemoteModelPersisters = remoteModelPersisters;
        }
        public void Dispose()
        {
            foreach(var remoteModelPersister in RemoteModelPersisters)
            {
                remoteModelPersister.Dispose();
            }
        }
    }
}
