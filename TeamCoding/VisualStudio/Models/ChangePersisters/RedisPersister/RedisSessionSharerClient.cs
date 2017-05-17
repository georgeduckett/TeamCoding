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
        public RedisSessionSharerClient()
        {
            TeamCodingPackage.Current.LocalIdeModel.AcceptedSharedSession += LocalIdeModel_AcceptedSharedSession;
        }
        private void LocalIdeModel_AcceptedSharedSession(object sender, LocalIDEModel.AcceptedSessionEventArgs e)
        {
            // We've accepted a session invite so send a message to the host requestion the session be initialised
            PublishSharingData(new RedisSharingData() { ToId = e.UserId, MessageType = RedisSharingData.SharingDataType.RequestingHostInitialisation });
        }

        protected override void HandleSharingData(RedisSharingData sharingData)
        {
            switch (sharingData.MessageType)
            {
                case RedisSharingData.SharingDataType.HostEndingSession: HandleHostEndingSession(sharingData); break;
                case RedisSharingData.SharingDataType.HostInitialised: HandleHostInitialised(sharingData); break;
            }
        }
        private void HandleHostInitialised(RedisSharingData sharingData)
        {
            // TODO: Do something when we receive the host initialised message (get the host's versions of the files)
        }
        private void HandleHostEndingSession(RedisSharingData sharingData)
        {
            TeamCodingPackage.Current.LocalIdeModel.MarkLeftSession(sharingData.FromId);
        }
    }
}
