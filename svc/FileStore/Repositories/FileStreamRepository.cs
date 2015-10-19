using System;
using System.IO;
using File = FileStore.Models.File;

namespace FileStore.Repositories
{
    public class FileStreamRepository : IFileStreamRepository
    {
        private readonly IConfigRepository _configRepository;
        private readonly IContentReadStream _contentReadStream;

        public FileStreamRepository() : this(new ConfigRepository(), new ContentReadStream())
        {
            
        }

        internal FileStreamRepository(IConfigRepository configRepository, IContentReadStream contentReadStream)
        {
            _configRepository = configRepository;
            _contentReadStream = contentReadStream;
        }

        public File GetFile(Guid fileGuid)
        {
            if (fileGuid == Guid.Empty)
            {
                throw new ArgumentException("fileGuid param is empty.");
            }

            // return a FILE object with the FILESTREAM content 
            // if FILESTREAM data file is not found return null

            File file = null;

            _contentReadStream.Setup(_configRepository.FileStreamDatabase, fileGuid);

            // get the length of the FILESTREAM so we can allocate a buffer
            long len = _contentReadStream.Length;

            if (len > 0)
            {
                // retrieve the FILESTREAM content

                file = new File
                {
                    FileId = fileGuid,
                    FileStream = _contentReadStream as Stream,
                    FileSize = len,
                    FileName = _contentReadStream.FileName,
                    FileType = _contentReadStream.FileType ?? "application/octet-stream"
                };
            }
            return file;
        }

        public File HeadFile(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                throw new ArgumentException("fileGuid param is empty.");
            }

            using (_contentReadStream)
            {
                _contentReadStream.Setup(_configRepository.FileStreamDatabase, guid);

                return new File
                {
                    FileId = guid,
                    FileSize = _contentReadStream.Length,
                    FileName = _contentReadStream.FileName,
                    FileType = _contentReadStream.FileType ?? "application/octet-stream"
                };
            }
        }
    }
}