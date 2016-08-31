using System;
using System.Data.Common;
using System.Threading.Tasks;
using FileStore.Models;

namespace FileStore.Repositories
{
    public interface IFilesRepository
    {
        DbConnection CreateConnection();

        Task<Guid> PostFileHead(File file);

        /// <summary>
        /// Returns the next chunk number
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        Task<int> PostFileChunk(FileChunk chunk);

        Task UpdateFileHead(Guid fileId, long fileSize, int chunkCount);

        Task<File> GetFileHead(Guid guid);

        File GetFileInfo(Guid fileId);

        Task<FileChunk> GetFileChunk(Guid guid, int num);

        byte[] ReadChunkContent(DbConnection dbConnection, Guid guid, int num);

        Task<Guid?> DeleteFile(Guid guid, DateTime? expired);

        Task<int> DeleteFileChunk(Guid guid, int chunkNumber);
    }
}
