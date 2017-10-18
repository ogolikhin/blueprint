using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Data.Common;
using ServiceLibrary.Helpers;

namespace FileStore.Repositories
{
    public class SqlPushStream : IPushStream
    {
        private Models.File _file = null;
        private IFilesRepository _filesRepository = null;

        public SqlPushStream()
        {
            // must call Initialize method to prepare reading routines
        }

        public void Initialize(IFilesRepository fr, Guid fileId)
        {
            ThrowIf.ArgumentNull(fr, nameof(fr));

            _filesRepository = fr;

            _file = _filesRepository.GetFileInfo(fileId);

            if (_file == null)
            {
                throw new InvalidOperationException(
                   I18NHelper.FormatInvariant("Fatal. File '{0}' not found in FileStore", fileId));
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
                CheckInitialized();
                dbConnection = _filesRepository.CreateConnection();
                dbConnection.Open();

                for (int chunkNum = 1; chunkNum <= _file.ChunkCount; chunkNum++)
                {
                    buffer = _filesRepository.ReadChunkContent(dbConnection, _file.FileId, chunkNum);
                    bytesRead = buffer.Length;
                    await outputStream.WriteAsync(buffer, 0, bytesRead);
                }
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
                buffer = null;
            }
        }

        private void CheckInitialized()
        {
            if (_file == null)
            {
                throw new InvalidOperationException(
                    "File object is null. Call Initialize method.");
            }
        }

    }
}