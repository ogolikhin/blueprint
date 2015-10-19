namespace FileStore.Repositories
{
    public interface IConfigRepository
    {
        string FileStoreDatabase { get; }
        string FileStreamDatabase { get; }
    }
}
