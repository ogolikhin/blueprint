using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data.SqlTypes;
 
namespace FileStore.Repositories
{
	public class FileStreamRepository : IFileStreamRepository
	{
        public Models.File GetFile(Guid fileGuid, string contentType)
        {
            if (fileGuid == null || fileGuid == Guid.Empty) throw new ArgumentException("fileGuid param is null or empty.");

            // return a FILE object with the FILESTREAM content 
            // if FILESTREAM data file is not found return null

            Models.File file = null;

            byte[] buffer = null;

            using (var fileStreamReader = new ContentReadStream(WebApiConfig.FileStreamDatabase, fileGuid))
            {
                try
                {
                    // get the length of the FILESTREAM so we can allocate a buffer
                    long len = fileStreamReader.Length;

                    if (len > 0)
                    {
                        buffer = new byte[len];

                        // retrieve the FILESTREAM content

                        int count = fileStreamReader.Read(buffer, 0, buffer.Length);

                        if (count > 0)
                        {
                            file = new Models.File()
                            {
                                FileId = fileGuid,
                                FileContent = buffer,
                                FileSize = buffer.Length,
                                FileType = contentType ?? "application/octet-stream"
                            };
                        }
                    }
                }
                catch
                {
                    // handle all exceptions upstream
                    throw;
                }
            }
            return file;
        }
    }
}