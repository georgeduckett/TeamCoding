using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.SourceControl
{
    internal class SourceControlRepo
    {
        public string GetRelativePath(string fullFilePath)
        {
            // TODO: Handle repositories other than Git
            var RepoPath = Repository.Discover(fullFilePath);
            if (RepoPath == null) return null;

            return fullFilePath.Substring(new DirectoryInfo(RepoPath).Parent.FullName.Length).TrimStart('\\');
        }
    }
}
