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
        public (int[] LineAdditions, int[] LineDeletions)? GetDiffWithServer(string fullFilePath)
        {
            return null;

            /*var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(fullFilePath);
            if (workspaceInfo == null) return null;

            var projectCollection = new TfsTeamProjectCollection(workspaceInfo.ServerUri);
            if (projectCollection == null) return null;

            var versionControlServer = projectCollection.GetService<VersionControlServer>();
            if (versionControlServer == null) return null;
            projectCollection.Credentials = System.Net.CredentialCache.DefaultCredentials;
            try
            {
                projectCollection.EnsureAuthenticated();
            }
            catch (TeamFoundationServerUnauthorizedException ex)
            {
                TeamCodingProjectTypeProvider.Get<ITeamCodingPackageProvider>().Logger.WriteError(ex);
                return null;
            }

            if (!versionControlServer.ServerItemExists(fullFilePath, ItemType.File)) return null;

            var serverPath = workspaceInfo.GetWorkspace(projectCollection).GetServerItemForLocalItem(fullFilePath);
            var serverVersion = new DiffItemVersionedFile(versionControlServer, serverPath, VersionSpec.Latest);
            var localVersion = new DiffItemLocalFile(fullFilePath, Encoding.UTF8.CodePage, DateTime.Now, false);

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                var diffOptions = new DiffOptions
                {
                    Flags = DiffOptionFlags.EnablePreambleHandling,
                    OutputType = DiffOutputType.Unified,
                    TargetEncoding = Encoding.UTF8,
                    SourceEncoding = Encoding.UTF8,
                    StreamWriter = writer
                };

                Difference.DiffFiles(versionControlServer, serverVersion, localVersion, diffOptions, serverPath, true);
                writer.Flush();
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var line = reader.ReadLine();
                }
            }

            return null; // TODO: Get the proper diff for TFS repositories
            */
        }

        public DocumentRepoMetaData GetRepoDocInfo(string fullFilePath)
        {
            var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(fullFilePath);
            if (workspaceInfo == null) return null;

            var projectCollection = new TfsTeamProjectCollection(workspaceInfo.ServerUri);
            if (projectCollection == null) return null;

            projectCollection.Credentials = System.Net.CredentialCache.DefaultCredentials;
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
            return new DocumentRepoMetaData()
            { // TODO: Populate the branch property
                RepoProvider = nameof(TeamFoundationServiceRepository),
                RelativePath = serverItem,
                LastActioned = DateTime.UtcNow,
                RepoUrl = workspaceInfo.ServerUri.ToString(),
                BeingEdited = serverWorkspace.GetPendingChanges(fullFilePath).Any()
            };
        }
    }
}
