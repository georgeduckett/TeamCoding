using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCoding.VisualStudio;

namespace TeamCoding.Documents.SourceControlRepositories
{
    /// <summary>
    /// Provides methods to get information about a file in a Git repository
    /// </summary>
    public class GitRepository : ISourceControlRepository
    {
        private string GetRepoPath(string fullFilePath)
        {
            var repoPath = Repository.Discover(fullFilePath);

            if (repoPath == null) return null; // No repository for file

            return fullFilePath.Substring(new DirectoryInfo(repoPath).Parent.FullName.Length).TrimStart('\\');
        }
        public DocumentRepoMetaData GetRepoDocInfo(string fullFilePath)
        {
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

            return new DocumentRepoMetaData()
            {
                RepoProvider = nameof(GitRepository),
                RepoUrl = repo.Head.TrackedBranch.Remote.Url,
                RepoBranch = repo.Head.TrackedBranch.CanonicalName,
                RelativePath = relativePath,
                BeingEdited = isEdited,
                LastActioned = DateTime.UtcNow
            };
        }

        public int? GetLineNumber(string fullFilePath, int fileLineNumber, FileNumberBasis targetBasis)
        {
            var relativePath = GetRepoPath(fullFilePath);

            // It's ok to return null here since calling methods will handle it and it allows us to not have some global "is this a repository setting"
            // Another reason it's better to do it this way is there's no "before loading a solution" event, meaning lots of listeners get the IsEnabled setting change too late (after docs are loaded)
            if (relativePath == null) return null;

            var repo = new Repository(Repository.Discover(fullFilePath));

            if (repo.Ignore.IsPathIgnored(relativePath)) return null;

            var change = repo.Diff.Compare<Patch>(new[] { fullFilePath }).SingleOrDefault();

            if(change == null)
            {
                return null;
            }

            return ParsePatch(change.Patch, fileLineNumber, targetBasis);
        }

        private int? ParsePatch(string patch, int fileLineNumber, FileNumberBasis targetBasis)
        {
            fileLineNumber++; // Because file lines are zero index based, but Git Diff isn't
            var lineAdditions = new List<int>();
            var lineDeletions = new List<int>();
            int? changedLine = null;
            int? sourceLine = null;

            foreach(var line in patch.Split('\n'))
            {
                if (line.StartsWith("@@"))
                {
                    var newLineStartIndex = line.IndexOf('+') + 1;
                    var newChangedLine = int.Parse(line.Substring(newLineStartIndex, line.IndexOf(',', newLineStartIndex) - newLineStartIndex)) - 1;
                    
                    var newSourceLine = int.Parse(line.Substring(line.IndexOf('+') + 1, line.IndexOf(',') - 4)) - 1;


                    if (targetBasis == FileNumberBasis.Server && newChangedLine >= fileLineNumber ||
                        targetBasis == FileNumberBasis.Local && newSourceLine >= fileLineNumber)
                    {
                        break;
                    }

                    changedLine = newChangedLine;
                    sourceLine = newSourceLine;
                }
                else if(line.StartsWith(" "))
                {
                    changedLine++;
                    sourceLine++;
                }
                else if(line.StartsWith("+") && changedLine != null)
                {
                    changedLine++;
                }
                else if (line.StartsWith("-") && changedLine != null)
                {
                    changedLine--;
                }

                if(targetBasis == FileNumberBasis.Server && changedLine == fileLineNumber)
                {
                    return sourceLine - 1;
                }
                if(targetBasis == FileNumberBasis.Local && sourceLine == fileLineNumber)
                {
                    return changedLine - 1;
                }
            }

            
            if (targetBasis == FileNumberBasis.Server)
            {
                if(sourceLine == null)
                {
                    return fileLineNumber - 1;
                }

                return sourceLine + (fileLineNumber - changedLine) - 1;
            }
            else
            {
                if (changedLine == null)
                {
                    return fileLineNumber - 1;
                }

                return changedLine + (fileLineNumber - sourceLine) - 1;
            }
        }
    }
}