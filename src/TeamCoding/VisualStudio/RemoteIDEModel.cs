using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.SourceControl;

namespace TeamCoding.VisualStudio
{
    public class RemoteIDEModel
    {
        public readonly string UserIdentity;
        public readonly List<SourceControlRepo.RepoDocInfo> _OpenFiles;

        public RemoteIDEModel(string[] fileLines)
        {
            UserIdentity = fileLines[0];
            _OpenFiles = fileLines.Skip(1)
                                  .Where(s => !string.IsNullOrWhiteSpace(s))
                                  .Select(s => new SourceControlRepo.RepoDocInfo() { BeingEdited = bool.Parse(s.Split(' ')[0]), RelativePath = s.Split(' ')[1] })
                                  .ToList();
        }
    }
}
