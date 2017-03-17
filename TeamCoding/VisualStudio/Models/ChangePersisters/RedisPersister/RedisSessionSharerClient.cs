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
    public class RedisSessionSharerClient : RedisSessionSharerBase
    {
        protected override void HandleSharingData(RedisSharingData sharingData)
        {
            switch (sharingData.MessageType)
            {
                case RedisSharingData.SharingDataType.RequestingSession: HandleRequestSession(sharingData); break;
                case RedisSharingData.SharingDataType.EndingSession: HandleEndingSession(sharingData); break;
            }
        }
        private void HandleRequestSession(RedisSharingData sharingData)
        {
            // TODO: Check we're not in a session already then present a dialog (or something) to the user
        }
        private void HandleEndingSession(RedisSharingData sharingData)
        {
            // TODO: Tidy up the session
            PublishSharingData(new RedisSharingData() { ToId = sharingData.FromId, MessageType = RedisSharingData.SharingDataType.EndingSession });
        }
        public void AcceptSession(string userId)
        {
            // TODO: Setup the session
            PublishSharingData(new RedisSharingData() { ToId = userId, MessageType = RedisSharingData.SharingDataType.AccceptingSession });
        }
        public void DeclineSession(string userId)
        {
            PublishSharingData(new RedisSharingData() { ToId = userId, MessageType = RedisSharingData.SharingDataType.DecliningSession });
        }
        public void LeaveSession(string userId)
        {
            PublishSharingData(new RedisSharingData() { ToId = userId, MessageType = RedisSharingData.SharingDataType.LeavingSession });
        }
    }
}
