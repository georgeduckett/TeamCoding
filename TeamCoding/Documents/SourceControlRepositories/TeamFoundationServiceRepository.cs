using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Documents.SourceControlRepositories
{
    public class TeamFoundationServiceRepository : ISourceControlRepository
    {
        public DocumentRepoMetaData GetRepoDocInfo(string fullFilePath)
        {
            var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(fullFilePath);
            if (workspaceInfo == null) return null;

            var projectCollection = new TfsTeamProjectCollection(workspaceInfo.ServerUri);
            if (projectCollection == null) return null;

            var serverWorkspace = workspaceInfo.GetWorkspace(projectCollection);
            if (serverWorkspace == null) return null;
            var versionControlServer = projectCollection.GetService<VersionControlServer>();
            if (versionControlServer == null) return null;

            if (!versionControlServer.ServerItemExists(fullFilePath, ItemType.File)) return null;

            

            var serverItem = serverWorkspace.GetServerItemForLocalItem(fullFilePath);
            return new DocumentRepoMetaData()
            { // TODO: Populate the branch property
                RelativePath = serverItem,
                LastActioned = DateTime.UtcNow,
                RepoUrl = workspaceInfo.ServerUri.ToString(),
                BeingEdited = serverWorkspace.GetPendingChanges(fullFilePath).Any()
            };
        }
    }
}
