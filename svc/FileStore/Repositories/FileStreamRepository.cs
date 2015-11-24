using System;
using System.IO;
using File = FileStore.Models.File;

namespace FileStore.Repositories
{
    public class FileStreamRepository : IFileStreamRepository
    {
        private readonly IConfigRepository _configRepository;
        private readonly IContentReadStream _contentReadStream;

        public FileStreamRepository() : this(ConfigRepository.Instance, new ContentReadStream())
        {

        }

        internal FileStreamRepository(IConfigRepository configRepository, IContentReadStream contentReadStream)
        {
            _configRepository = configRepository;
            _contentReadStream = contentReadStream;
        }

        public File GetFileHead(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                throw new ArgumentException("fileGuid param is empty.");
            }

            // return a File object with the file info (no content)

            using (_contentReadStream)
            {
                _contentReadStream.Setup(_configRepository.FileStreamDatabase, fileId);

                return new File
                {
                    FileId = fileId,
                    FileSize = _contentReadStream.Length,
                    FileName = _contentReadStream.FileName,
                    FileType = _contentReadStream.FileType ?? "application/octet-stream"
                };
            }
        }

        public Stream GetFileContent(Guid fileId)
        {
            // return a custom stream that retrieves the FileStream content

            Stream fileStream = null;
              
            _contentReadStream.Setup(_configRepository.FileStreamDatabase, fileId);
  
            if (_contentReadStream.Length > 0)
            {
                fileStream = _contentReadStream as Stream;
           
            }
            return fileStream;
        }
    }
}
