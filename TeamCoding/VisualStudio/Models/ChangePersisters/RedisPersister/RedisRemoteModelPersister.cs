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
        private static ConnectionMultiplexer RedisClient = ConnectionMultiplexer.Connect("localhost"); // TODO: allow for failing to connect to redis (and connect asyncronously)
        private static ISubscriber RedisSubscriber = RedisClient.GetSubscriber();
        public RedisRemoteModelPersister()
        {
            RedisSubscriber.Subscribe(ModelPersisterChannel, Redis_RemoteModelReceived);
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
            RedisClient?.Dispose();
        }
    }
}