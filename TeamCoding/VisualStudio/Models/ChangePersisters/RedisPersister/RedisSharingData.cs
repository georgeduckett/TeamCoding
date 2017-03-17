using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.RedisPersister
{
    [ProtoContract]
    public class RedisSharingData
    {
        public enum SharingDataType { RequestingSession, AccceptingSession, DecliningSession, LeavingSession, EndingSession }
        [ProtoMember(1)]
        public string FromId;
        [ProtoMember(2)]
        public string ToId;
        [ProtoMember(3)]
        public SharingDataType MessageType;
    }
}
