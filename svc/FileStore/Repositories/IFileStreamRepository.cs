using System;
using FileStore.Models;

namespace FileStore.Repositories
{
	public interface IFileStreamRepository
	{
        File GetFile(Guid guid);

	    File HeadFile(Guid guid);
	}
}
