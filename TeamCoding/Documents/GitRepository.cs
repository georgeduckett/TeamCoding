using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCoding.VisualStudio;

namespace TeamCoding.Documents
{
    /// <summary>
    /// Provides methods to get information about a file in a Git repository
    /// </summary>
    public class GitRepository
    {
        private readonly Dictionary<string, DocumentRepoMetaData> RepoData = new Dictionary<string, DocumentRepoMetaData>();
        public void RemoveCachedRepoData(string docFilePath)
        {
            RepoData.Remove(docFilePath);
        }
        private string GetRepoPath(string fullFilePath)
        {
            var repoPath = Repository.Discover(fullFilePath);

            if (repoPath == null) return null; // No repository for file

            return fullFilePath.Substring(new DirectoryInfo(repoPath).Parent.FullName.Length).TrimStart('\\');
        }
        public DocumentRepoMetaData GetRepoDocInfo(string fullFilePath)
        {
            // TODO: Handle repositories other than Git using http://www.codewrecks.com/blog/index.php/2010/09/13/how-to-get-tfs-server-address-from-a-local-folder-mapped-to-a-workspace/
            if (RepoData.ContainsKey(fullFilePath))
            {
                var fileRepoData = RepoData[fullFilePath];

                fileRepoData.LastActioned = DateTime.UtcNow;

                return fileRepoData;
            }

            var relativePath = GetRepoPath(fullFilePath);

            // It's ok to return null here since calling methods will handle it and it allows us to not have some global "is this a repository setting"
            // Another reason it's better to do it this way is there's no "before loading a solution" event, meaning lots of listeners get the IsEnabled setting change too late (after docs are loaded)
            if (relativePath == null) return null;

            var repo = new Repository(Repository.Discover(fullFilePath));

            if (repo.Ignore.IsPathIgnored(relativePath)) return null;

            var repoHeadTree = repo.Head.Tip.Tree;
            var remoteMasterTree = repo.Head.TrackedBranch.Tip.Tree;

            // Check for local changes, then server changes.
            // It's possible there is a local change that actually makes it the same as the remote, but I think that's ok to say the user is editing anyway
            var isEdited = repo.Diff.Compare<TreeChanges>(new[] { fullFilePath }).Any() ||
                           repo.Diff.Compare<TreeChanges>(remoteMasterTree, repoHeadTree, new[] { fullFilePath }).Any();

            RepoData[fullFilePath] = new DocumentRepoMetaData()
            {
                RepoUrl = repo.Head.TrackedBranch.Remote.Url,
                RepoBranch = repo.Head.TrackedBranch.CanonicalName,
                RelativePath = relativePath,
                BeingEdited = isEdited,
                LastActioned = DateTime.UtcNow
            };

            return RepoData[fullFilePath];
        }
    }
}