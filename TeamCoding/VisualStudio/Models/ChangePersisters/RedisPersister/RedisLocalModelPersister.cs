using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, new RemoteIDEModel(IdeModel));
                TeamCodingPackage.Current.Redis.Publish(RedisRemoteModelPersister.ModelPersisterChannel, ms.ToArray());
            }
        }
        public void Dispose()
        {

        }
    }
}
