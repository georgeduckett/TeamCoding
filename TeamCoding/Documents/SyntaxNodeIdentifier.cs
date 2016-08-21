using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Documents
{
    [ProtoBuf.ProtoContract]
    public class SyntaxNodeIdentifier
    {
        [ProtoBuf.ProtoMember(1)]
        public int[] Id;
        public SyntaxNodeIdentifier() { } // For protobuf
        public SyntaxNodeIdentifier(int[] id) { Id = id; }
        public override bool Equals(object obj)
        {
            var syntaxNodeIdentifier = obj as SyntaxNodeIdentifier;
            if (syntaxNodeIdentifier == null) { return false; }
            return Id.SequenceEqual(syntaxNodeIdentifier.Id);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return Id.Aggregate(17, (acc, next) => acc * 31 + next);
            }
        }
    }
}
