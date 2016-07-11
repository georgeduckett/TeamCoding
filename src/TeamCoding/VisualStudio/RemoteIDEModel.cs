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
        public RemoteIDEModel(LocalIDEModel localModel)
        {
            UserIdentity = TeamCodingPackage.Current.IdentityProvider.GetIdentity();
            _OpenFiles = new List<SourceControlRepo.RepoDocInfo>(localModel.OpenDocs());
        }
    }
}
