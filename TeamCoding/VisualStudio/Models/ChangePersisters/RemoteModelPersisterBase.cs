using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;
using TeamCoding.Options;

namespace TeamCoding.VisualStudio.Models.ChangePersisters
{
    public abstract class RemoteModelPersisterBase : IRemoteModelPersister
    {
        // TODO: Cache all the different lookups we want to do
        public event EventHandler RemoteModelReceived;
        private readonly SharedSettings SharedSettings;
        private readonly Dictionary<string, RemoteIDEModel> RemoteModels = new Dictionary<string, RemoteIDEModel>();
        public IEnumerable<IRemotelyAccessedDocumentData> GetOpenFiles() => RemoteModels.Values.SelectMany(model => model.OpenFiles.Select(of => new RemotelyAccessedDocumentData()
        {
            Repository = of.RepoUrl,
            RepositoryBranch = of.RepoBranch,
            IdeUserIdentity = model.IDEUserIdentity,
            RelativePath = of.RelativePath,
            BeingEdited = of.BeingEdited,
            HasFocus = of == model.OpenFiles.OrderByDescending(oof => oof.LastActioned).FirstOrDefault(),
            CaretPositionInfo = of.CaretPositionInfo
        }));
        public RemoteModelPersisterBase()
        {
            SharedSettings = TeamCodingPackage.Current.Settings.SharedSettings;
            SharedSettings.ShowSelfChanged += SharedSettings_ShowSelfChanged;
        }

        private async void SharedSettings_ShowSelfChanged(object sender, EventArgs e)
        {
            if (SharedSettings.ShowSelf)
            {
                await TeamCodingPackage.Current.LocalModelChangeManager.SendUpdate();
            }
            else if (RemoteModels.ContainsKey(LocalIDEModel.Id.Value))
            {
                RemoteModels.Remove(LocalIDEModel.Id.Value);
                RemoteModelReceived?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ClearRemoteModels()
        {
            RemoteModels.Clear();
        }
        public void OnRemoteModelReceived(RemoteIDEModel remoteModel)
        {
            TeamCodingPackage.Current.IDEWrapper.InvokeAsync(() =>
            {
                if (remoteModel == null)
                {
                    ClearRemoteModels();
                    RemoteModelReceived?.Invoke(this, EventArgs.Empty);
                }
                else if (remoteModel.Id == LocalIDEModel.Id.Value && !TeamCodingPackage.Current.Settings.SharedSettings.ShowSelf)
                {
                    // If the remote model is the same as the local model, then remove it if it's there already
                    if (RemoteModels.ContainsKey(remoteModel.Id))
                    {
                        RemoteModels.Remove(remoteModel.Id);
                        RemoteModelReceived?.Invoke(this, EventArgs.Empty);
                    }
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
            });
        }
        public virtual void Dispose()
        {
            SharedSettings.ShowSelfChanged -= SharedSettings_ShowSelfChanged;
        }

    }
}
