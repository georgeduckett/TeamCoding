using System;
using System.Linq;

namespace TeamCoding.Documents
{
    /// <summary>
    /// Represents data about a document
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class DocumentRepoMetaData
    {
        [ProtoBuf.ProtoContract]
        public class CaretInfo
        {
            [ProtoBuf.ProtoMember(1)]
            public int[] SyntaxNodeIds { get; set; }
            [ProtoBuf.ProtoMember(2)]
            public int LeafMemberCaretOffset { get; set; }
        }
        [ProtoBuf.ProtoMember(1)]
        public string RepoUrl { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public string RepoBranch { get; set; }
        [ProtoBuf.ProtoMember(3)]
        public string RelativePath { get; set; }
        [ProtoBuf.ProtoMember(4)]
        public bool BeingEdited { get; set; }
        [ProtoBuf.ProtoMember(5)]
        public DateTime LastActioned { get; set; }
        [ProtoBuf.ProtoMember(6)]
        public CaretInfo CaretPositionInfo { get; set; }
    }
}
