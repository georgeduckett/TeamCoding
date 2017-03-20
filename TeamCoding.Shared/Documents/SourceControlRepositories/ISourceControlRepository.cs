namespace TeamCoding.Documents.SourceControlRepositories
{
    public interface ISourceControlRepository
    {
        DocumentRepoMetaData GetRepoDocInfo(string fullFilePath);
        string[] GetRemoteFileLines(string fullFilePath);
    }
}