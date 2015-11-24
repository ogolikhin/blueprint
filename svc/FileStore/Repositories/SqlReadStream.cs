using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using FileStore.Models;
using Dapper;

namespace FileStore.Repositories
{
    public class SqlReadStream : Stream, IDisposable 
    {         
        private SqlConnection _connection = null;
        private int _chunkNumber = 1;
        private Models.File _file = null;
         
        public SqlReadStream ()
        {
            // must call Initialize method to prepare reading routines
        }

        public void Initialize (string connectionString, Guid fileId)
        {
            _chunkNumber = 1;

            OpenConnection(connectionString);

            _file = ReadFileHead(fileId);

            if (_file == null)
            {
                throw new InvalidOperationException(
                   String.Format("Fatal. File '{0}' not found in FileStore", fileId.ToString()));
            }
        }

        public override long Length
        {
            get
            {
                CheckInitialized();

                return  _file.FileSize;
            }
        }
    
        public override long Position
        {
            get
            {
                // Note: the position will actually be the number of the chunk record
                // stored in the FileChunks table instead of a byte position in the stream 

                return _chunkNumber;
            }
            set
            {
                _chunkNumber = (int)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Read chunks of file content from the FileChunks table in 
            // the Filestore database into the stream's read buffer
            int bytesRead = 0;

            try
            {
                 CheckInitialized();

                if (_chunkNumber <= _file.ChunkCount)
                {
                    FileChunk chunk = ReadFileChunk(_file.FileId, _chunkNumber);

                    if (chunk == null)
                    {
                        // there has been a database error or a sequencing error
                        // if the chunk record is not found 

                        throw new DataException(
                            String.Format("Attempt to read file '{0}' chunk {1} failed.", _file.FileId, _chunkNumber));
                    }
                    if (buffer.Length > chunk.ChunkSize)
                    {
                        count = chunk.ChunkSize;
                    }
                    Buffer.BlockCopy(chunk.ChunkContent, 0, buffer, offset, count);
               
                    _chunkNumber++;

                    bytesRead = chunk.ChunkSize;
                }
                else
                {
                    // close the connection and return 0 if no more chunks 

                    bytesRead = 0;
                    CloseConnection();
                  
                }
             
                return bytesRead;
            }
            catch 
            {
                // log errors here and rethrow the exception 
                throw;
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        #region [ Unimplemented Methods ]

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }
    
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public override void Write(byte[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        #endregion unimplemented methods


        #region [ Private Methods ]

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

        private FileChunk ReadFileChunk(Guid guid, int num)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@FileId", guid);
            parameters.Add("@ChunkNum", num);

            Models.FileChunk fileChunk =
                  _connection.Query<Models.FileChunk>(
                    "ReadFileChunk",
                    parameters,
                    commandType: CommandType.StoredProcedure).FirstOrDefault<Models.FileChunk>();

            return fileChunk;
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
        #endregion

        protected override void Dispose(bool disposing)
        {
            CloseConnection();
        }
    }
}