﻿using System;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data.Common;
using Dapper;
using FileStore.Models;

namespace FileStore.Repositories
{
    public class SqlFilesRepository : IFilesRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public SqlFilesRepository()
             : this(new SqlConnectionWrapper(ConfigRepository.Instance.FileStoreDatabase))
        {
        }

        internal SqlFilesRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        public DbConnection CreateConnection()
        {
            // create a connection for operations that require holding an open connection to the db
            return ConnectionWrapper.CreateConnection();
        }

        private DateTime? GetPostFileHeadExpirationTime(DateTime? dateTime)
        {
            DateTime? dateTimeUtc= null;
            if (dateTime.HasValue)
            {
                // Convert to UTC if required
                dateTimeUtc = dateTime.Value.Kind != DateTimeKind.Utc ? dateTime.Value.ToUniversalTime() : dateTime.Value;
                if (dateTimeUtc < DateTime.UtcNow)
                {
                    dateTimeUtc = DateTime.UtcNow;
                }
            }

            return dateTimeUtc;
        }

        private DateTime GetDeleteFileExpirationTime(DateTime? dateTime)
        {
            // if the expiry time is null make the expiry time equal to today 
            // if the expiry time is before today make the expiry time equal to today 
            // if the expiry time is in the future leave it as a future expiration time

            DateTime dateTimeUtc;

            if (dateTime.HasValue)
            {
                // Convert to UTC if required
                dateTimeUtc = dateTime.Value.Kind != DateTimeKind.Utc ? dateTime.Value.ToUniversalTime() : dateTime.Value;
                
                if (dateTimeUtc < DateTime.UtcNow)
                {
                    dateTimeUtc = DateTime.UtcNow;
                }
            }
            else
            {
                dateTimeUtc = DateTime.UtcNow;
            }

            return dateTimeUtc;
        }

        public async Task<Guid> PostFileHead(File file)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileName", file.FileName);
            prm.Add("@FileType", file.FileType);
            prm.Add("@ExpiredTime", GetPostFileHeadExpirationTime(file.ExpiredTime));
            prm.Add("@ChunkCount", file.ChunkCount);
            prm.Add("@FileSize", file.FileSize);
            prm.Add("@FileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
            await ConnectionWrapper.ExecuteAsync("InsertFileHead", prm, commandType: CommandType.StoredProcedure);
            return file.FileId = prm.Get<Guid>("FileId");
        }

        public async Task<int> PostFileChunk(FileChunk chunk)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", chunk.FileId);
            prm.Add("@ChunkNum", chunk.ChunkNum);
            prm.Add("@ChunkSize", chunk.ChunkSize);
            prm.Add("@ChunkContent", chunk.ChunkContent);
            await ConnectionWrapper.ExecuteAsync("InsertFileChunk", prm, commandType: CommandType.StoredProcedure);
            return chunk.ChunkNum + 1;
        }

        public async Task UpdateFileHead(Guid fileId, long fileSize, int chunkCount)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", fileId);
            prm.Add("@ChunkCount", chunkCount);
            prm.Add("@FileSize", fileSize);
            await ConnectionWrapper.ExecuteAsync("UpdateFileHead", prm, commandType: CommandType.StoredProcedure);
        }
        public async Task<File> GetFileHead(Guid guid)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", guid);

            var file = (await ConnectionWrapper.QueryAsync<File>("ReadFileHead", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            if (file != null)
            {
                if (file.ExpiredTime.HasValue)
                {
                    file.ExpiredTime = DateTime.SpecifyKind(file.ExpiredTime.Value, DateTimeKind.Utc);
                }
                file.StoredTime = DateTime.SpecifyKind(file.StoredTime, DateTimeKind.Utc);
            }

            return file;
        }

        public async Task<FileChunk> GetFileChunk(Guid guid, int num)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", guid);
            prm.Add("@ChunkNum", num);
            return (await ConnectionWrapper.QueryAsync<FileChunk>("ReadFileChunk", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<Guid?> DeleteFile(Guid guid, DateTime? expired)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", guid);
            prm.Add("@ExpiredTime", GetDeleteFileExpirationTime(expired));
            return (await ConnectionWrapper.ExecuteScalarAsync<int>("DeleteFile", prm, commandType: CommandType.StoredProcedure)) > 0 ? guid : (Guid?)null;
        }
        public async Task<int> DeleteFileChunk(Guid guid, int chunkNumber)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", guid);
            prm.Add("@ChunkNumber", chunkNumber);
            return (await ConnectionWrapper.ExecuteScalarAsync<int>("DeleteFileChunk", prm, commandType: CommandType.StoredProcedure));
        }
        public Models.File GetFileInfo(Guid fileId)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", fileId);

            return ConnectionWrapper.Query<Models.File>("ReadFileHead", prm, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }

        public byte[] ReadChunkContent(DbConnection dbConnection, Guid guid, int num)
        {
            // Note: this method may be called hundreds of times to retrieve chunk records if the 
            // stored file is large. It will reuse the open database connection that is passed 
            // in as a parameter.

            // Note: After all the read operations are finished the dbConnection object must be closed
            // and disposed by the calling procedure.

            var prm = new DynamicParameters();
            prm.Add("@FileId", guid);
            prm.Add("@ChunkNum", num);

            if (dbConnection == null || dbConnection.State == ConnectionState.Closed)
            {
                throw new ArgumentNullException("The database connection must be open prior to use.");
            }

            return dbConnection.ExecuteScalar<byte[]>("ReadChunkContent", prm, commandType: CommandType.StoredProcedure);

        }

    }
}
