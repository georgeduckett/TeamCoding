using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.SourceControl;
using TeamCoding.VisualStudio.Identity;

namespace TeamCoding.VisualStudio
{
    [ProtoContract]
    public class RemoteIDEModel
    {
        [ProtoMember(1)]
        public readonly UserIdentity IDEUserIdentity;
        [ProtoIgnore]
        private List<SourceControlRepo.RepoDocInfo> _OpenFiles;
        [ProtoMember(2)]
        public List<SourceControlRepo.RepoDocInfo> OpenFiles
        {
            get { return _OpenFiles ?? (_OpenFiles = new List<SourceControlRepo.RepoDocInfo>()); }
            private set { _OpenFiles = value; }
        }

        public RemoteIDEModel() { }
        public RemoteIDEModel(LocalIDEModel localModel)
        {
            IDEUserIdentity = TeamCodingPackage.Current.IdentityProvider.GetIdentity();
            OpenFiles = new List<SourceControlRepo.RepoDocInfo>(localModel.OpenDocs());
        }
    }
}
