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
            public string[] RepoUrls { get; set; }
            [ProtoBuf.ProtoMember(2)]
            public string RelativePath { get; set; }
            [ProtoBuf.ProtoMember(3)]
            public bool BeingEdited { get; set; }
        }
        public RepoDocInfo GetRelativePath(string fullFilePath)
        {
            // TODO: Handle repositories other than Git
            var RepoPath = Repository.Discover(fullFilePath);

            var Repo = new Repository(RepoPath);

            var IsEdited = Repo.Diff.Compare<TreeChanges>(new[] { fullFilePath }).Any();
            if (RepoPath == null) return null;
            return new RepoDocInfo()
            {
                RepoUrls = Repo.Network.Remotes.Select(r => r.Url).ToArray(),
                RelativePath = fullFilePath.Substring(new DirectoryInfo(RepoPath).Parent.FullName.Length).TrimStart('\\'),
                BeingEdited = IsEdited
            };
        }
    }
}
