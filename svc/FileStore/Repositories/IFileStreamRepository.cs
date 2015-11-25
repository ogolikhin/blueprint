using System;
using System.IO; 

namespace FileStore.Repositories
{
	public interface IFileStreamRepository
	{
        Stream GetFileContent(Guid fileId);

        FileStore.Models.File GetFileHead(Guid fileId);

        bool FileExists(Guid fileId);
	}
}
