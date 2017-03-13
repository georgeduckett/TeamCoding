namespace TeamCoding.Documents.SourceControlRepositories
{
    public interface ISourceControlRepository
    {
        DocumentRepoMetaData GetRepoDocInfo(string fullFilePath);
        int? GetLineNumber(string fullFilePath, int fileLineNumber, FileNumberBasis targetBasis);
    }
}