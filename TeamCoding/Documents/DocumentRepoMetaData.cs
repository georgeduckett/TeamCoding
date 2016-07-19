using System;

namespace TeamCoding.Documents
{
    /// <summary>
    /// Represents source control repository data about a document
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class DocumentRepoMetaData
    {
        [ProtoBuf.ProtoMember(1)]
        public string RepoUrl { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public string RelativePath { get; set; }
        [ProtoBuf.ProtoMember(3)]
        public bool BeingEdited { get; set; }
        [ProtoBuf.ProtoMember(4)]
        public DateTime LastActioned { get; set; }
    }
}
