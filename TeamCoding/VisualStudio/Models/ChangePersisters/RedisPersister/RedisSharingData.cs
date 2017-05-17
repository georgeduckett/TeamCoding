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
        public enum SharingDataType
        {
            RequestingHostInitialisation, // A client requests that a host initialise a sharing session
            HostInitialised,
            HostEndingSession // A host indicates that it's ending the current session
        }
        [ProtoMember(1)]
        public string FromId;
        [ProtoMember(2)]
        public string ToId;
        [ProtoMember(3)]
        public SharingDataType MessageType;
    }
}
