using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.RedisPersister
{
    public class RedisLocalModelPersister : ILocalModelPerisister
    {
        private readonly LocalIDEModel IdeModel;
        public RedisLocalModelPersister(LocalIDEModel model)
        {
            IdeModel = model;
            IdeModel.OpenViewsChanged += IdeModel_OpenViewsChanged;
            IdeModel.TextContentChanged += IdeModel_TextContentChanged;
            IdeModel.TextDocumentSaved += IdeModel_TextDocumentSaved;
            TeamCodingPackage.Current.Settings.SharedSettings.RedisServerChanging += SharedSettings_RedisServerChanging;
            TeamCodingPackage.Current.Settings.SharedSettings.RedisServerChanged += SharedSettings_RedisServerChanged;
        }

        private void SharedSettings_RedisServerChanged(object sender, EventArgs e)
        {
            SendChanges();
        }

        private void SharedSettings_RedisServerChanging(object sender, EventArgs e)
        {
            SendModel(new RemoteIDEModel(new LocalIDEModel()));
        }

        private void IdeModel_TextDocumentSaved(object sender, Microsoft.VisualStudio.Text.TextDocumentFileActionEventArgs e)
        {
            SendChanges();
        }
        private void IdeModel_TextContentChanged(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            // SendChanges();
        }
        private void IdeModel_OpenViewsChanged(object sender, EventArgs e)
        {
            SendChanges();
        }
        protected virtual void SendChanges()
        {
            SendModel(new RemoteIDEModel(IdeModel));
        }

        private void SendModel(RemoteIDEModel remoteModel)
        {
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, remoteModel);
                TeamCodingPackage.Current.Redis.Publish(RedisRemoteModelPersister.ModelPersisterChannel, ms.ToArray()).HandleException();
            }
        }

        public void Dispose()
        {

        }
    }
}
