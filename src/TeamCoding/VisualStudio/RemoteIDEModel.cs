using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.SourceControl;

namespace TeamCoding.VisualStudio
{
    [ProtoContract]
    public class RemoteIDEModel
    {
        [ProtoMember(1)]
        public readonly string UserIdentity;
        [ProtoMember(2)]
        public readonly List<SourceControlRepo.RepoDocInfo> _OpenFiles; // TODO: Make backing field so it handles null from protobuf

        public RemoteIDEModel() { }
        public RemoteIDEModel(string[] fileLines)
        {
            UserIdentity = fileLines[0];
            _OpenFiles = fileLines.Skip(1)
                                  .Where(s => !string.IsNullOrWhiteSpace(s))
                                  .Select(s => new SourceControlRepo.RepoDocInfo() { BeingEdited = bool.Parse(s.Split(' ')[0]), RelativePath = s.Split(' ')[1] })
                                  .ToList();
        }
    }
}
