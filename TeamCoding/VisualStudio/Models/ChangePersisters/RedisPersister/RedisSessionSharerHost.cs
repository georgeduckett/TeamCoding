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
    /// <summary>
    /// Acts has the host for sharing document edits a session via Redis
    /// </summary>
    public class RedisSessionSharerHost : RedisSessionSharerBase
    {
        protected override void HandleSharingData(RedisSharingData sharingData)
        {
            switch (sharingData.MessageType)
            {
                case RedisSharingData.SharingDataType.HostEndingSession: break;
                case RedisSharingData.SharingDataType.RequestingHostInitialisation: HandleRequestingHostInitialisation(sharingData); break;
            }
        }
        private void HandleRequestingHostInitialisation(RedisSharingData sharingData)
        {
            // TODO: Persist the latest version of each changed file to redis if we haven't already

            PublishSharingData(new RedisSharingData() { ToId = sharingData.ToId, MessageType = RedisSharingData.SharingDataType.HostInitialised });
        }
        public override void Dispose()
        {
            // TODO: Remove all persisted changed files from redis (if any)
            PublishSharingData(new RedisSharingData() { MessageType = RedisSharingData.SharingDataType.HostEndingSession });
            base.Dispose();
        }
    }
}
