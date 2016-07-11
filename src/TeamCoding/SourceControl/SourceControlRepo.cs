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
        public class RepoDocInfo
        {
            public string RelativePath { get; set; }
            public bool BeingEdited { get; set; }
        }
        public RepoDocInfo GetRelativePath(string fullFilePath)
        {
            // TODO: Handle repositories other than Git
            var RepoPath = Repository.Discover(fullFilePath);
            var IsEdited = new Repository(RepoPath).Diff.Compare<TreeChanges>(new[] { fullFilePath }).Any();
            if (RepoPath == null) return null;

            return new RepoDocInfo()
            {
                RelativePath = fullFilePath.Substring(new DirectoryInfo(RepoPath).Parent.FullName.Length).TrimStart('\\'),
                BeingEdited = IsEdited
            };
        }
    }
}
