using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		Task<bool> GetStatus();
	}
}
