namespace FileStore.Repositories
{
    public interface IConfigRepository
    {
        string FileStoreDatabase { get; }

        string FileStreamDatabase { get; }

        int FileChunkSize { get; }

        int LegacyFileChunkSize { get; }

        int CommandTimeout { get; }
    }
}
