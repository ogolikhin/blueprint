using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using FileStore.Models;
using System.Net.Http;
using System.Net;
using System.Web;
using Dapper;
using System.Threading.Tasks;

namespace FileStore.Repositories
{
    public class SqlPushStream : IDisposable 
    {
         
        private SqlConnection _connection = null;
        private Models.File _file = null;

        public SqlPushStream()
        {
            // must call Initialize method to prepare reading routines
        }

        public void Initialize(string connectionString, Guid fileId)
        {
 
            OpenConnection(connectionString);

            _file = ReadFileHead(fileId);

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

            try
            {
                CheckInitialized();

                for (int chunkNum = 1; chunkNum <= _file.ChunkCount; chunkNum++)
                {
                    buffer = ReadChunkContent(_file.FileId, chunkNum);
                    bytesRead = buffer.Length; 
                    await outputStream.WriteAsync(buffer, 0, bytesRead);
                }
   
            }
            catch 
            {
                // log error here
                return;
            }
            finally
            {
                outputStream.Close();
                CloseConnection();
                buffer = null;
            }
        }

        private void OpenConnection(string connectionString)
        {
            try
            {
                if (_connection != null)
                {
                    CloseConnection();
                }

                if (String.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentException("Connection string is null or empty.");
                }

                _connection = new SqlConnection(connectionString);

                _connection.Open();

            }
            catch
            {
                // log connection errors and rethrow 
                throw;
            }
        }

        private void CloseConnection()
        {
            if (_connection != null)
            {
                try
                {
                    if (_connection.State != ConnectionState.Closed)
                    {
                        _connection.Close();
                    }
                }
                catch
                {
                    // ignored
                }
                _connection.Dispose();
                _connection = null;
            }
        }

        private Models.File ReadFileHead(Guid fileId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@FileId", fileId);

            Models.File file =
                _connection.Query<Models.File>(
                    "ReadFileHead",
                    parameters,
                    commandType: CommandType.StoredProcedure).FirstOrDefault<Models.File>();

            return file;
        }

        private byte[] ReadChunkContent(Guid guid, int num)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@FileId", guid);
            parameters.Add("@ChunkNum", num);

            return _connection.ExecuteScalar<byte[]>(
                    "ReadChunkContent",
                    parameters,
                    commandType: CommandType.StoredProcedure);

        }

        private void CheckInitialized()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(
                    "Sql connection is not open. Call Initialize method.");
            }

            if (_file == null)
            {
                throw new InvalidOperationException(
                    "File object is null. Call Initialize method.");
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                CloseConnection();
            }
 
        }
    }

}