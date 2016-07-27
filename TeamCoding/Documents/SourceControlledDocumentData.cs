using System;
using TeamCoding.IdentityManagement;

namespace TeamCoding.Documents
{
    /// <summary>
    /// Contains data about a document belonging to source control
    /// </summary>
    public class SourceControlledDocumentData : IEquatable<SourceControlledDocumentData>
    {
        public string Repository { get; set; }
        public string RepositoryBranch { get; set; }
        public string RelativePath { get; set; }
        public UserIdentity IdeUserIdentity { get; set; }
        public bool BeingEdited { get; set; }
        public bool HasFocus { get; set; }
        public override int GetHashCode()
        {
            return Repository.GetHashCode() ^ RepositoryBranch.GetHashCode() ^ IdeUserIdentity.Id.GetHashCode() ^ BeingEdited.GetHashCode() ^ HasFocus.GetHashCode();
        }
        public bool Equals(SourceControlledDocumentData other)
        {
            if (other == null)
                return false;

            return Repository == other.Repository &&
                   RepositoryBranch == other.RepositoryBranch &&
                   RelativePath == other.RelativePath &&
                   IdeUserIdentity.Id == IdeUserIdentity.Id &&
                   BeingEdited == other.BeingEdited &&
                   HasFocus == other.HasFocus;
        }
        public override bool Equals(object obj)
        {
            var typedObj = obj as SourceControlledDocumentData;
            return Equals(typedObj);
        }
    }
}
