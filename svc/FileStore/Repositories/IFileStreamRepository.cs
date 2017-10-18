using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;

namespace FileStore.Repositories
{
    public interface IFileStreamRepository
    {
        DbConnection CreateConnection();

        bool IsDatabaseAvailable();

        FileStore.Models.File GetFileHead(Guid fileId);

        bool FileExists(Guid fileId);

        byte[] ReadChunkContent(DbConnection sqlConnection, Guid fileId, long count, long position);

      
    }
}
