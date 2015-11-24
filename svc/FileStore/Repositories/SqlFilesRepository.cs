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
		internal readonly ISqlConnectionWrapper ConnectionWrapper;

		public SqlFilesRepository()
			 : this(new SqlConnectionWrapper(ConfigRepository.Instance.FileStoreDatabase))
		{
		}

		internal SqlFilesRepository(ISqlConnectionWrapper connectionWrapper)
		{
			ConnectionWrapper = connectionWrapper;
		}

		public async Task<Guid> PostFileHead(File file)
		{
			var prm = new DynamicParameters();
			prm.Add("@FileName", file.FileName);
			prm.Add("@FileType", file.FileType);
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

	    public async void UpdateFileHead(Guid fileId, long fileSize, int chunkCount)
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
			return (await ConnectionWrapper.QueryAsync<File>("GetFileHead", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
		}

		public async Task<FileChunk> GetFileChunk(Guid guid, int num)
		{
			var prm = new DynamicParameters();
			prm.Add("@FileId", guid);
			prm.Add("@Num", num);
			return (await ConnectionWrapper.QueryAsync<FileChunk>("GetFileChunk", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
		}

		public async Task<Guid?> DeleteFile(Guid guid)
		{
			var prm = new DynamicParameters();
			prm.Add("@FileId", guid);
			prm.Add("@DeletedFileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
			await ConnectionWrapper.ExecuteAsync("DeleteFile", prm, commandType: CommandType.StoredProcedure);
			return prm.Get<Guid?>("DeletedFileId");
		}
	}
}
