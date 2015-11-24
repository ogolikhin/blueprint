using System;
using System.Threading.Tasks;
using FileStore.Models;

namespace FileStore.Repositories
{
	public interface IFilesRepository
	{
		Task<Guid> PostFileHead(File file);
        /// <summary>
        /// Returns the next chunk number
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
		Task<int> PostFileChunk(FileChunk chunk);
	    void UpdateFileHead(Guid fileId, long fileSize, int chunkCount);
		Task<File> GetFileHead(Guid guid);
		Task<FileChunk> GetFileChunk(Guid guid, int num);
		Task<Guid?> DeleteFile(Guid guid);
	}
}
