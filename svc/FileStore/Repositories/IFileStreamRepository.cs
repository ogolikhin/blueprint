using System;
using System.IO;
using FileStore.Models;

namespace FileStore.Repositories
{
	public interface IFileStreamRepository
	{
        Stream GetFileContent(Guid fileId);

	    Models.File GetFileHead(Guid fileId);
	}
}
