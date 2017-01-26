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
        private readonly UserSettings UserSettings;
        private readonly Dictionary<string, RemoteIDEModel> RemoteModels = new Dictionary<string, RemoteIDEModel>();
        private IRemotelyAccessedDocumentData[] CachedOpenFiles = null;

        public IEnumerable<IRemotelyAccessedDocumentData> GetOpenFiles()
        {
            if (CachedOpenFiles != null)
            {
                return CachedOpenFiles;
            }
            else
            {
                return CachedOpenFiles = RemoteModels.Values.SelectMany(model => model.OpenFiles.Select(of => new RemotelyAccessedDocumentData()
                {
                    Repository = of.RepoUrl,
                    RepositoryBranch = of.RepoBranch,
                    IdeUserIdentity = model.IDEUserIdentity,
                    RelativePath = of.RelativePath,
                    BeingEdited = of.BeingEdited,
                    HasFocus = of == model.OpenFiles.OrderByDescending(oof => oof.LastActioned).FirstOrDefault(),
                    CaretPositionInfo = of.CaretPositionInfo
                })).ToArray();
            }
        }
        public RemoteModelPersisterBase()
        {
            UserSettings = TeamCodingPackage.Current.Settings.UserSettings;
            UserSettings.ShowSelfChanged += SharedSettings_ShowSelfChangedAsync;
        }

        private async void SharedSettings_ShowSelfChangedAsync(object sender, EventArgs e)
        {
            if (UserSettings.ShowSelf)
            {
                await TeamCodingPackage.Current.LocalModelChangeManager.SendUpdateAsync();
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
            CachedOpenFiles = null;
        }
        public void OnRemoteModelReceived(RemoteIDEModel remoteModel)
        {
            TeamCodingPackage.Current.IDEWrapper.InvokeAsync(() =>
            {
                CachedOpenFiles = null;
                if (remoteModel == null)
                {
                    ClearRemoteModels();
                    RemoteModelReceived?.Invoke(this, EventArgs.Empty);
                }
                else if (remoteModel.Id == LocalIDEModel.Id.Value && !TeamCodingPackage.Current.Settings.UserSettings.ShowSelf)
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
            UserSettings.ShowSelfChanged -= SharedSettings_ShowSelfChangedAsync;
        }

    }
}
