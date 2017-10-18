using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Data.Common;
using ServiceLibrary.Helpers;

namespace FileStore.Repositories
{
    public class FileStreamPushStream : IPushStream
    {
        private Models.File _file = null;
        private IFileStreamRepository _filesRepository = null;
        private IConfigRepository _configRepository = null;

        public FileStreamPushStream()
        {
            // must call Initialize method to prepare reading routines
        }

        public void Initialize(IFileStreamRepository fsr, IConfigRepository config, Guid fileId)
        {
            ThrowIf.ArgumentNull(fsr, nameof(fsr));
            ThrowIf.ArgumentNull(config, nameof(config));

            _filesRepository = fsr;
            _configRepository = config;

            _file = _filesRepository.GetFileHead(fileId);

            if (_file == null)
            {
                throw new InvalidOperationException(
                   I18NHelper.FormatInvariant("Fatal. File '{0}' not found in legacy database", fileId));
            }
        }

        public async Task WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            int bytesRead = 0;
            byte[] buffer = null;
            DbConnection dbConnection = null;

            // In the WriteToStream method, we proceed to read the file chunks progressively from the db
            // and flush these bits to the output stream.

            try
            {
                dbConnection = _filesRepository.CreateConnection();
                dbConnection.Open();

                CheckInitialized();

                long position = 0;
                long count = _configRepository.LegacyFileChunkSize;

                do
                {
                    if (count > (_file.FileSize - position))
                    {
                        count = (int)(_file.FileSize - position);
                    }
                    buffer = _filesRepository.ReadChunkContent(dbConnection, _file.FileId, count, position);
                    bytesRead = buffer.Length;
                    await outputStream.WriteAsync(buffer, 0, bytesRead);
                    position += bytesRead;

                } while (position < _file.FileSize);

            }
            catch
            {
                // log error here
                throw;
            }
            finally
            {
                outputStream.Close();
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
                dbConnection = null;
                buffer = null;
            }
        }

        private void CheckInitialized()
        {
            if (_file == null)
            {
                throw new InvalidOperationException(
                    "File object is null. Call Initialize method first.");
            }
        }

    }
}