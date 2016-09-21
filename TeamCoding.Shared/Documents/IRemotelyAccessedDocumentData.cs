using TeamCoding.IdentityManagement;

namespace TeamCoding.Documents
{
    public interface IRemotelyAccessedDocumentData
    {
        bool BeingEdited { get; set; }
        DocumentRepoMetaData.CaretInfo CaretPositionInfo { get; set; }
        bool HasFocus { get; set; }
        IUserIdentity IdeUserIdentity { get; set; }
        string RelativePath { get; set; }
        string Repository { get; set; }
        string RepositoryBranch { get; set; }

        bool Equals(IRemotelyAccessedDocumentData other);
        bool Equals(object obj);
        int GetHashCode();
    }
}