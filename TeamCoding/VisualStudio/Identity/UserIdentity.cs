using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Identity
{
    [ProtoBuf.ProtoContract]
    public class UserIdentity
    {
        [ProtoBuf.ProtoMember(1)]
        public string DisplayName { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public string ImageUrl { get; set; }
    }
}
