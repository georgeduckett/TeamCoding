namespace TeamCoding.Documents.SourceControlRepositories
{
    public interface ISourceControlRepository
    {
        DocumentRepoMetaData GetRepoDocInfo(string fullFilePath);
    }
}