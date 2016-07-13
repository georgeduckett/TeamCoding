using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.VisualStudio.Identity;

namespace TeamCoding
{
    public class RemoteDocumentData : IEquatable<RemoteDocumentData>
    {
        public string Repository { get; set; }
        public string RelativePath { get; set; }
        public UserIdentity IdeUserIdentity { get; set; }
        public bool BeingEdited { get; set; }

        public override int GetHashCode()
        {
            return Repository.GetHashCode() ^ Repository.GetHashCode() ^ IdeUserIdentity.GetHashCode() ^ BeingEdited.GetHashCode();
        }
        public bool Equals(RemoteDocumentData other)
        {
            if (other == null)
                return false;

            return (Repository == other.Repository &&
                    RelativePath == other.RelativePath &&
                    IdeUserIdentity.DisplayName == IdeUserIdentity.DisplayName &&
                    IdeUserIdentity.ImageUrl == IdeUserIdentity.ImageUrl &&
                    BeingEdited == other.BeingEdited);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var tObj = obj as RemoteDocumentData;
            return Equals(tObj);
        }
    }
}
