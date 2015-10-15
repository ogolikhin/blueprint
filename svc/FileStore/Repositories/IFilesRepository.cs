using System;
using System.Threading.Tasks;
using FileStore.Models;

namespace FileStore.Repositories
{
	public interface IFilesRepository
	{
		Task<Guid?> PostFile(File file);
		Task<File> HeadFile(Guid guid);
		Task<File> GetFile(Guid guid);
		Task<Guid?> DeleteFile(Guid guid);
	}
}
