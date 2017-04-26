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
    public abstract class RedisSessionSharerBase : IDisposable
    {
        public const string SharedSessionHostChannel = "TeamCoding.RedisSessionSharer";
        private readonly string LocalModelId = LocalIDEModel.Id.Value;
        private readonly Task SubscribeTask;
        public RedisSessionSharerBase()
        {
            SubscribeTask = TeamCodingPackage.Current.Redis.SubscribeAsync(SharedSessionHostChannel, Redis_SharingDataReceived).HandleException();
        }
        private void Redis_SharingDataReceived(RedisChannel channel, RedisValue value)
        {
            using (var ms = new MemoryStream(value))
            {
                var sharingData = ProtoBuf.Serializer.Deserialize<RedisSharingData>(ms);
                if (sharingData.ToId != LocalModelId)
                {
                    // If we're not the intended receipent then stop
                    return;
                }

                HandleSharingData(sharingData);
            }
        }
        protected abstract void HandleSharingData(RedisSharingData sharingData);
        protected void PublishSharingData(RedisSharingData sharingData)
        {
            using (var ms = new MemoryStream())
            {
                sharingData.FromId = LocalModelId;
                ProtoBuf.Serializer.Serialize(ms, sharingData);
                TeamCodingPackage.Current.Redis.PublishAsync(SharedSessionHostChannel, ms.ToArray()).HandleException();
            }
        }
        public void Dispose()
        {
            Task.WaitAll(SubscribeTask);
        }
    }
}
