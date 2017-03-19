using ProtoBuf;
using System.Linq;
using System.Collections.Generic;
using TeamCoding.Documents;
using TeamCoding.IdentityManagement;

namespace TeamCoding.VisualStudio.Models
{
    /// <summary>
    /// Represents an IDE being used remotely
    /// </summary>
    [ProtoContract]
    public class RemoteIDEModel
    {
        [ProtoMember(1)]
        public string Id;
        [ProtoMember(2)]
        public UserIdentity IDEUserIdentity;
        [ProtoIgnore]
        private List<DocumentRepoMetaData> _OpenFiles;
        [ProtoMember(3)]
        public List<DocumentRepoMetaData> OpenFiles
        {
            get { return _OpenFiles ?? (_OpenFiles = new List<DocumentRepoMetaData>()); }
            private set { _OpenFiles = value; }
        }
        [ProtoIgnore]
        private Dictionary<string, bool> _SharedSessionInvitedUsers;
        [ProtoMember(4)]
        public Dictionary<string, bool> SharedSessionInvitedUsers
        {
            get { return _SharedSessionInvitedUsers ?? (_SharedSessionInvitedUsers = new Dictionary<string, bool>()); }
            private set { _SharedSessionInvitedUsers = value; }
        }

        public RemoteIDEModel() { } // For protobuf
        public RemoteIDEModel(LocalIDEModel localModel)
        {
            Id = LocalIDEModel.Id.Value;
            IDEUserIdentity = TeamCodingPackage.Current.IdentityProvider.GetIdentity();
            OpenFiles = new List<DocumentRepoMetaData>(localModel.OpenDocs());
            SharedSessionInvitedUsers = localModel.SharedSessionInvitedUsers().ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}
