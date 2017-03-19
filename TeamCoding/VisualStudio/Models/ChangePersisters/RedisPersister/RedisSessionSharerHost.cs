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
    /// Acts has the host for sharing a session via Redis
    /// </summary>
    public class RedisSessionSharerHost : RedisSessionSharerBase
    {
        protected override void HandleSharingData(RedisSharingData sharingData)
        {
            switch (sharingData.MessageType)
            {
                case RedisSharingData.SharingDataType.AccceptingSession: HandleAcceptSession(sharingData); break;
                case RedisSharingData.SharingDataType.DecliningSession: HandleDeclineSession(sharingData); break;
                case RedisSharingData.SharingDataType.LeavingSession: HandleLeavingSession(sharingData); break;
            }
        }
        public void EndSession()
        {
            PublishSharingData(new RedisSharingData() { MessageType = RedisSharingData.SharingDataType.EndingSession });
        }
        public void HandleAcceptSession(RedisSharingData data)
        {

        }
        public void HandleDeclineSession(RedisSharingData data)
        {

        }
        public void HandleLeavingSession(RedisSharingData data)
        {

        }
    }
}
