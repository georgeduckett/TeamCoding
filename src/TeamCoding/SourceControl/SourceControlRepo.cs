using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.SourceControl
{
    public class SourceControlRepo
    {
        [ProtoBuf.ProtoContract]
        public class RepoDocInfo
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
        public string GetRepoPath(string fullFilePath)
        {
            if(fullFilePath.ToLower() == fullFilePath)
            {

            }

            var repoPath = Repository.Discover(fullFilePath);

            if (repoPath == null) return null; // No repository for file

            return fullFilePath.Substring(new DirectoryInfo(repoPath).Parent.FullName.Length).TrimStart('\\');
        }
        public RepoDocInfo GetRepoDocInfo(string fullFilePath)
        {
            // TODO: Handle repositories other than Git
            var relativePath = GetRepoPath(fullFilePath);

            var repo = new Repository(Repository.Discover(fullFilePath));

            if (repo.Ignore.IsPathIgnored(relativePath)) return null;

            var repoHeadTree = repo.Head.Tip.Tree;
            var remoteMasterTree = repo.Head.TrackedBranch.Tip.Tree;

            // Check for local changes, then server changes.
            // It's possible there is a local change that actually makes it the same as the remote, but I think that's ok to say the user is editing anyway
            var isEdited = repo.Diff.Compare<TreeChanges>(new[] { fullFilePath }).Any() ||
                           repo.Diff.Compare<TreeChanges>(remoteMasterTree, repoHeadTree, new[] { fullFilePath }).Any();
            
            return new RepoDocInfo()
            {
                RepoUrl = repo.Head.TrackedBranch.Remote.Url,
                RelativePath = relativePath,
                BeingEdited = isEdited,
                LastActioned = DateTime.UtcNow
            };
        }
    }
}