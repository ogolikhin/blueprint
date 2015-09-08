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
                file.FileId = (await cxn.QueryAsync<Guid>("PostFile", prm, commandType: CommandType.StoredProcedure)).Single();
				return (file.FileId != Guid.Empty) ? file.FileId : (Guid?)null;
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
				return (await cxn.QueryAsync<int>("DeleteFile", prm, commandType: CommandType.StoredProcedure)).Single() > 0 ? guid : (Guid?)null;
			}
		}

		public async Task<bool> GetStatus()
		{
			using (var cxn = new SqlConnection(WebApiConfig.FileStoreDatabase))
			{
				cxn.Open();
				return (await cxn.QueryAsync<int>("GetStatus", commandType: CommandType.StoredProcedure)).Single() >=0;
			}
		}
	}
}