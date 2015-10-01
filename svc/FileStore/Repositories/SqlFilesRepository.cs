using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using FileStore.Models;

namespace FileStore.Repositories
{
	public class SqlFilesRepository : IFilesRepository
	{
		public async Task<Guid?> PostFile(File file)
		{
			using (var cxn = new SqlConnection(WebApiConfig.FileStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@FileName", file.FileName);
				prm.Add("@FileType", file.FileType);
				prm.Add("@FileContent", file.FileContent);
                prm.Add("@FileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
                await cxn.ExecuteAsync("PostFile", prm, commandType: CommandType.StoredProcedure);
                return prm.Get<Guid?>("FileId");
			}
		}

		public async Task<File> HeadFile(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.FileStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@FileId", guid);
				return (await cxn.QueryAsync<File>("HeadFile", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
			}
		}
         
		public async Task<File> GetFile(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.FileStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@FileId", guid);
				return (await cxn.QueryAsync<File>("GetFile", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
			}
		}

		public async Task<Guid?> DeleteFile(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.FileStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@FileId", guid);
                prm.Add("@DeletedFileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
                await cxn.ExecuteAsync("DeleteFile", prm, commandType: CommandType.StoredProcedure);
                return prm.Get<Guid?>("DeletedFileId");
			}
		}
	}
}