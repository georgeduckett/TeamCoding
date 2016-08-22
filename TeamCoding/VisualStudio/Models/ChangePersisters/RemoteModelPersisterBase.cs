using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters
{
    public abstract class RemoteModelPersisterBase : IRemoteModelPersister
    {
        public event EventHandler RemoteModelReceived;
        private readonly Dictionary<string, RemoteIDEModel> RemoteModels = new Dictionary<string, RemoteIDEModel>();
        public IEnumerable<RemotelyAccessedDocumentData> GetOpenFiles() => RemoteModels.Values.SelectMany(model => model.OpenFiles.Select(of => new RemotelyAccessedDocumentData()
        {
            Repository = of.RepoUrl,
            RepositoryBranch = of.RepoBranch,
            IdeUserIdentity = model.IDEUserIdentity,
            RelativePath = of.RelativePath,
            BeingEdited = of.BeingEdited,
            HasFocus = of == model.OpenFiles.OrderByDescending(oof => oof.LastActioned).FirstOrDefault(),
            CaretPositionInfo = of.CaretPositionInfo
        }));
        public void ClearRemoteModels()
        {
            RemoteModels.Clear();
        }
        public void OnRemoteModelReceived(RemoteIDEModel remoteModel)
        {
            if(remoteModel == null)
            {
                ClearRemoteModels();
                RemoteModelReceived?.Invoke(this, EventArgs.Empty);
            }
            else if(remoteModel.Id == LocalIDEModel.Id.Value && !System.Diagnostics.Debugger.IsAttached)
            {
                // If we're not debugging and the remote model is the same as the local model, then don't process it
            }
            else if (remoteModel.OpenFiles.Count == 0)
            {
                if (RemoteModels.ContainsKey(remoteModel.Id))
                {
                    RemoteModels.Remove(remoteModel.Id);
                    RemoteModelReceived?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                RemoteModels[remoteModel.Id] = remoteModel;
                RemoteModelReceived?.Invoke(this, EventArgs.Empty);
            }
        }
        public virtual void Dispose() { }

    }
}
