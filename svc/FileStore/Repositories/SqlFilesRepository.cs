using System;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using FileStore.Models;
using ServiceLibrary.Repositories;

namespace FileStore.Repositories
{
	public class SqlFilesRepository : IFilesRepository
	{
		internal readonly ISqlConnectionWrapper _connectionWrapper;

		public SqlFilesRepository()
			 : this(new SqlConnectionWrapper(new ConfigRepository().FileStoreDatabase))
		{
		}

		internal SqlFilesRepository(ISqlConnectionWrapper connectionWrapper)
		{
			_connectionWrapper = connectionWrapper;
		}

		public async Task<Guid> PostFileHead(File file)
		{
			var prm = new DynamicParameters();
			prm.Add("@FileName", file.FileName);
			prm.Add("@FileType", file.FileType);
			prm.Add("@ChunkCount", file.ChunkCount);
			prm.Add("@FileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
			await _connectionWrapper.ExecuteAsync("PostFileHead", prm, commandType: CommandType.StoredProcedure);
			return file.FileId = prm.Get<Guid>("FileId");
		}

		public async Task<int> PostFileChunk(FileChunk chunk)
		{
			var prm = new DynamicParameters();
			prm.Add("@FileId", chunk.FileId);
			prm.Add("@ChunkNum", chunk.ChunkNum);
			prm.Add("@ChunkSize", chunk.ChunkSize);
			prm.Add("@ChunkContent", chunk.ChunkContent);
			await _connectionWrapper.ExecuteAsync("PostFileChunk", prm, commandType: CommandType.StoredProcedure);
			return chunk.ChunkNum + 1;
		}

		public async Task<File> GetFileHead(Guid guid)
		{
			var prm = new DynamicParameters();
			prm.Add("@FileId", guid);
			return (await _connectionWrapper.QueryAsync<File>("GetFileHead", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
		}

		public async Task<FileChunk> GetFileChunk(Guid guid, int num)
		{
			var prm = new DynamicParameters();
			prm.Add("@FileId", guid);
			prm.Add("@Num", num);
			return (await _connectionWrapper.QueryAsync<FileChunk>("GetFileChunk", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
		}

		public async Task<Guid> DeleteFile(Guid guid)
		{
			var prm = new DynamicParameters();
			prm.Add("@FileId", guid);
			prm.Add("@DeletedFileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
			await _connectionWrapper.ExecuteAsync("DeleteFile", prm, commandType: CommandType.StoredProcedure);
			return prm.Get<Guid>("DeletedFileId");
		}

        public System.IO.Stream GetFileContent(Guid fileId)
        {
            // return a custom stream reader that retrieves content from the
            // FileChunks table in the Filestore database 

            SqlReadStream sqlReadStream = null;
 
            ConfigRepository configRepository = new ConfigRepository();
           
            sqlReadStream = new SqlReadStream();
            sqlReadStream.Initialize(configRepository.FileStoreDatabase, fileId);
             
            return sqlReadStream;
        }
    }
}
