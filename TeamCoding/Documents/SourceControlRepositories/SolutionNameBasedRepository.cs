using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCoding.VisualStudio;

namespace TeamCoding.Documents.SourceControlRepositories
{
    /// <summary>
    /// Provides methods to get information about a file based on just the solution name, treating each unique solution name as a repository.
    /// This could link unrelated solutions, but only 2 users were using the same sync method and working on different identically-named solutions.
    /// This generally won't be a problem unless I create some kind of global sync option that's enabled by default.
    /// This must be added last in a list of repositories as it could match anything.
    /// </summary>
    public class SolutionNameBasedRepository : ISourceControlRepository
    {
        private string GetRepoPath(string fullFilePath)
        {
            var repoPath = Path.GetDirectoryName(TeamCodingPackage.Current.IDEWrapper.SolutionFilePath);

            return fullFilePath.Substring(new DirectoryInfo(repoPath).Parent.FullName.Length).TrimStart('\\');
        }
        public DocumentRepoMetaData GetRepoDocInfo(string fullFilePath)
        {
            //Ensure it's not another repo that's providing information for this solution's sln file (if it is then don't fall back to this as ignored files etc. would get covered by this
            if(fullFilePath != TeamCodingPackage.Current.IDEWrapper.SolutionFilePath &&
                TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(TeamCodingPackage.Current.IDEWrapper.SolutionFilePath).RepoProvider != nameof(SolutionNameBasedRepository))
            {
                return null;
            }

            var relativePath = GetRepoPath(fullFilePath);

            if (relativePath == null) return null;

            return new DocumentRepoMetaData()
            {
                RepoProvider = nameof(SolutionNameBasedRepository),
                RepoUrl = Path.GetFileNameWithoutExtension(TeamCodingPackage.Current.IDEWrapper.SolutionFilePath),
                RelativePath = relativePath,
                LastActioned = DateTime.UtcNow
            };
        }
        public bool CanProvideDataForSolution(string solutionFilePath)
        {
            return true;
        }

        public int? GetLineNumber(string fullFilePath, int fileLineNumber, FileNumberBasis targetBasis)
        {
            if (fullFilePath == null)
            {
                return null;
            }
            else
            {
                return fileLineNumber;
            }
        }
        public string[] GetRemoteFileLines(string fullFilePath)
        {
            return File.ReadAllText(fullFilePath).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }
    }
}