using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileStore.Models;

namespace FileStore.Repositories
{
	public interface IFilesRepository
	{
		Task<Guid> PostFileHead(File file);
		Task<int> PostFileChunk(FileChunk chunk);
		Task<File> GetFileHead(Guid guid);
		Task<FileChunk> GetFileChunk(Guid guid, int num);
		Task<IEnumerable<FileChunk>> GetAllFileChunks(Guid guid);
		Task<Guid?> DeleteFile(Guid guid);

		System.IO.Stream GetFileContent(Guid fileId);
	}
}
