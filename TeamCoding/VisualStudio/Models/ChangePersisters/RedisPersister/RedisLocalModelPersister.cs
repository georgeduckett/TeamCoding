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
    public class RedisLocalModelPersister : LocalModelPersisterBase
    {
        public RedisLocalModelPersister(LocalIDEModel model) : base(model, TeamCodingPackage.Current.Settings.SharedSettings.RedisServerProperty) { }
        protected override void SendModel(RemoteIDEModel remoteModel)
        {
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, remoteModel);
                TeamCodingPackage.Current.Redis.Publish(RedisRemoteModelPersister.ModelPersisterChannel, ms.ToArray()).HandleException();
            }
        }
    }
}
