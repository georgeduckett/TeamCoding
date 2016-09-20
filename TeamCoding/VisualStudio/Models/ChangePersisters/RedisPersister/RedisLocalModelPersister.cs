using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Events;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.RedisPersister
{
    public class RedisLocalModelPersister : ILocalModelPerisister
    {
        private readonly LocalIDEModel IdeModel;
        public RedisLocalModelPersister(LocalIDEModel model)
        {
            IdeModel = model;
            IdeModel.ModelChanged += IdeModel_ModelChanged;
            TeamCodingPackage.Current.Settings.SharedSettings.RedisServerChanged += IdeModel_ModelChanged;
            TeamCodingPackage.Current.Settings.SharedSettings.RedisServerChanging += SharedSettings_RedisServerChanging;
        }
        private void SharedSettings_RedisServerChanging(object sender, EventArgs e)
        {
            SendModel(new RemoteIDEModel(new LocalIDEModel()));
        }
        private void IdeModel_ModelChanged(object sender, EventArgs e)
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
                TeamCodingPackage.Current.Logger.WriteInformation("Publishing Model");
                TeamCodingPackage.Current.Redis.Publish(RedisRemoteModelPersister.ModelPersisterChannel, ms.ToArray()).HandleException();
            }
        }
        public void Dispose()
        {

        }
    }
}
