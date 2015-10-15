namespace FileStore.Repositories
{
    public interface IFileMapperRepository
    {
        string GetMappedOutputContentType(string fileType);
    }
}
