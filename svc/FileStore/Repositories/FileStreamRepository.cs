using System;
using FileStore.Models;

namespace FileStore.Repositories
{
    public class FileStreamRepository : IFileStreamRepository
    {
        public File GetFile(Guid fileGuid)
        {
            if (fileGuid == null || fileGuid == Guid.Empty) throw new ArgumentException("fileGuid param is null or empty.");

            // return a FILE object with the FILESTREAM content 
            // if FILESTREAM data file is not found return null

            File file = null;

            byte[] buffer = null;

            using (var fileStreamReader = new ContentReadStream(WebApiConfig.FileStreamDatabase, fileGuid))
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
                        file = new File
                        {
                            FileId = fileGuid,
                            FileContent = buffer,
                            FileSize = buffer.Length,
                            FileName = fileStreamReader.FileName,
                            FileType = fileStreamReader.FileType ?? "application/octet-stream"
                        };
                    }
                }

            }
            return file;
        }

        public File HeadFile(Guid guid)
        {
            if (guid == null || guid == Guid.Empty)
            {
                throw new ArgumentException("fileGuid param is null or empty.");
            }

            using (var fileStreamReader = new ContentReadStream(WebApiConfig.FileStreamDatabase, guid))
            {
                return new File
                {
                    FileId = guid,
                    FileContent = null,
                    FileSize = fileStreamReader.Length,
                    FileName = fileStreamReader.FileName,
                    FileType = fileStreamReader.FileType ?? "application/octet-stream"
                };
            }
        }
    }
}