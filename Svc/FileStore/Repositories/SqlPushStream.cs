using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Threading.Tasks;
using System.Data.Common;
using FileStore.Models;

namespace FileStore.Repositories
{
    public class SqlPushStream 
    {
        private Models.File _file = null;
        private IFilesRepository _filesRepository = null;

        public SqlPushStream()
        {
            // must call Initialize method to prepare reading routines
        }

        public void Initialize(IFilesRepository fr, Guid fileId)
        {
            if (fr == null)
            {
                throw new ArgumentException("File repository param is null.");
            }

            _filesRepository = fr; 

            _file = _filesRepository.GetFileInfo(fileId);

            if (_file == null)
            {
                throw new InvalidOperationException(
                   String.Format("Fatal. File '{0}' not found in FileStore", fileId.ToString()));
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
                throw ;
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