namespace TeamCoding.Documents.SourceControlRepositories
{
    public interface ISourceControlRepository
    {
        DocumentRepoMetaData GetRepoDocInfo(string fullFilePath);
        (int[] LineAdditions, int[] LineDeletions)? GetDiffWithServer(string fullFilePath);
    }
}