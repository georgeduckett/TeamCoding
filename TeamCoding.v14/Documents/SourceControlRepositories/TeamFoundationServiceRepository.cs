using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using System;
using System.Collections.Generic;
using System.IO;
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

            using (var projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(workspaceInfo.ServerUri))
            {
                if (projectCollection == null) return null;
                projectCollection.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                try
                {
                    projectCollection.EnsureAuthenticated();
                }
                catch (TeamFoundationServerUnauthorizedException ex)
                {
                    TeamCodingProjectTypeProvider.Get<ITeamCodingPackageProvider>().Logger.WriteError(ex);
                    return null;
                }

                var serverWorkspace = workspaceInfo.GetWorkspace(projectCollection);
                if (serverWorkspace == null) return null;
                var versionControlServer = projectCollection.GetService<VersionControlServer>();
                if (versionControlServer == null) return null;

                try
                {
                    if (!versionControlServer.ServerItemExists(fullFilePath, ItemType.File)) return null;
                }
                catch (TeamFoundationServerUnauthorizedException ex)
                {
                    TeamCodingProjectTypeProvider.Get<ITeamCodingPackageProvider>().Logger.WriteError(ex);
                    return null;
                }

                var serverItem = serverWorkspace.GetServerItemForLocalItem(fullFilePath);

                var branch = versionControlServer.QueryBranchObjects(new ItemIdentifier(serverItem), RecursionType.None).FirstOrDefault()?.Properties?.RootItem?.Item;
                return new DocumentRepoMetaData()
                {
                    RepoProvider = nameof(TeamFoundationServiceRepository),
                    RelativePath = serverItem,
                    LastActioned = DateTime.UtcNow,
                    RepoUrl = workspaceInfo.ServerUri.ToString(),
                    RepoBranch = branch,
                    BeingEdited = serverWorkspace.GetPendingChanges(fullFilePath).Any()
                };
            }
        }

        public int? GetLineNumber(string fullFilePath, int fileLineNumber, FileNumberBasis targetBasis)
        {
            return null;
        }
    }
}
