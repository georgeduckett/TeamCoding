using System;

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
            [ProtoBuf.ProtoContract]
            public class SyntaxNodeIdentifier
            {
                [ProtoBuf.ProtoMember(1)]
                public int Id;
                public SyntaxNodeIdentifier() { } // For protobuf
                public SyntaxNodeIdentifier(int id) { Id = id; }
                public override bool Equals(object obj)
                {
                    var syntaxNodeIdentifier = obj as SyntaxNodeIdentifier;
                    if (syntaxNodeIdentifier == null) { return false; }
                    return Id.Equals(syntaxNodeIdentifier.Id);
                }
                public override int GetHashCode()
                {
                    return Id.GetHashCode();
                }
            }
            [ProtoBuf.ProtoMember(1)]
            public SyntaxNodeIdentifier[] SyntaxNodeIds { get; set; }
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
