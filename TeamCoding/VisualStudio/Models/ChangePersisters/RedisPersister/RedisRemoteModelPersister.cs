using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.RedisPersister
{
    public class RedisRemoteModelPersister : RemoteModelPersisterBase
    {
        public const string ModelPersisterChannel = "TeamCoding.ModelPersister";
        public RedisRemoteModelPersister()
        {
            TeamCodingPackage.Current.Redis.Subscribe(ModelPersisterChannel, Redis_RemoteModelReceived).ContinueWith((t) =>
            {
                // TODO: Handle subscribe exception
            });
        }
        private void Redis_RemoteModelReceived(RedisChannel channel, RedisValue value)
        {
            using (var ms = new MemoryStream(value))
            {
                OnRemoteModelReceived(ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(ms));
            }
        }
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}