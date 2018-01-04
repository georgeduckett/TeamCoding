﻿using System;
using TeamCoding.IdentityManagement;

namespace TeamCoding.Documents
{
    /// <summary>
    /// Contains data about a document belonging to source control that's open by a user
    /// </summary>
    public class RemotelyAccessedDocumentData : IEquatable<IRemotelyAccessedDocumentData>, IRemotelyAccessedDocumentData
    {
        public string Repository { get; set; }
        public string RepositoryBranch { get; set; }
        public string RelativePath { get; set; }
        public IUserIdentity IdeUserIdentity { get; set; }
        public bool BeingEdited { get; set; }
        public bool HasFocus { get; set; }
        public DocumentRepoMetaData.CaretInfo CaretPositionInfo { get; set; }
        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 31 + Repository.GetHashCode();

            if (!string.IsNullOrEmpty(RepositoryBranch))
                hash = hash * 31 + RepositoryBranch.GetHashCode();

            hash = hash * 31 + IdeUserIdentity.Id.GetHashCode();
            hash = hash * 31 + BeingEdited.GetHashCode();
            hash = hash * 31 + HasFocus.GetHashCode();
            hash = hash * 31 + (CaretPositionInfo?.LeafMemberCaretOffset.GetHashCode() ?? 0);
            hash = hash * 31 + (CaretPositionInfo?.LeafMemberLineOffset.GetHashCode() ?? 0);
            hash = hash * 31 + (CaretPositionInfo?.SyntaxNodeIds.GetHashCode() ?? 0);

            return hash;
        }
        public bool Equals(IRemotelyAccessedDocumentData other)
        {
            if (other == null)
                return false;

            return Repository == other.Repository &&
                   RepositoryBranch == other.RepositoryBranch &&
                   RelativePath == other.RelativePath &&
                   IdeUserIdentity.Id == other.IdeUserIdentity.Id &&
                   BeingEdited == other.BeingEdited &&
                   HasFocus == other.HasFocus &&
                   CaretPositionInfo?.LeafMemberCaretOffset == other.CaretPositionInfo?.LeafMemberCaretOffset &&
                   CaretPositionInfo?.LeafMemberLineOffset == other.CaretPositionInfo?.LeafMemberLineOffset &&
                   CaretPositionInfo?.SyntaxNodeIds == other.CaretPositionInfo?.SyntaxNodeIds;
        }
        public override bool Equals(object obj)
        {
            var typedObj = obj as IRemotelyAccessedDocumentData;
            return Equals(typedObj);
        }
    }
}
