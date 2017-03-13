using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Documents.SourceControlRepositories
{
    public class CachedSourceControlRepository : ISourceControlRepository // TODO: Catch source-control events like undoing pending changes
    {
        private readonly ISourceControlRepository[] Repositories;
        public CachedSourceControlRepository(params ISourceControlRepository[] repositories)
        {
            Repositories = repositories;
        }
        protected readonly Dictionary<string, DocumentRepoMetaData> RepoData = new Dictionary<string, DocumentRepoMetaData>();
        public void RemoveCachedRepoData(string docFilePath) => RepoData.Remove(docFilePath);
        public DocumentRepoMetaData GetRepoDocInfo(string fullFilePath)
        {
            if (string.IsNullOrEmpty(fullFilePath))
                return null;

            if (RepoData.ContainsKey(fullFilePath))
            {
                var fileRepoData = RepoData[fullFilePath];

                fileRepoData.LastActioned = DateTime.UtcNow;

                return fileRepoData;
            }

            foreach(var repository in Repositories)
            {
                var data = repository.GetRepoDocInfo(fullFilePath);
                if(data != null)
                {
                    RepoData.Add(fullFilePath, data);
                    return data;
                }
            }

            return null;
        }

        public int? GetLineNumber(string fullFilePath, int fileLineNumber, FileNumberBasis targetBasis)
        {
            if (string.IsNullOrEmpty(fullFilePath))
                return null;

            foreach (var repository in Repositories)
            {
                var data = repository.GetLineNumber(fullFilePath, fileLineNumber, targetBasis);
                if (data != null)
                {
                    return data;
                }
            }

            return null;
        }
    }
}
