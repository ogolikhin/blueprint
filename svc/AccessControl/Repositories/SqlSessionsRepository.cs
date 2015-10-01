using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using AccessControl.Models;

namespace AccessControl.Repositories
{
	public class SqlSessionsRepository : ISessionsRepository
	{
		public async Task<Guid?> PostFile(Session session)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@FileName", session.FileName);
				prm.Add("@FileType", session.FileType);
				prm.Add("@FileContent", session.FileContent);
                prm.Add("@FileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
                await cxn.ExecuteAsync("PostFile", prm, commandType: CommandType.StoredProcedure);
                return prm.Get<Guid?>("FileId");
			}
		}

		public async Task<Session> HeadFile(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@FileId", guid);
				return (await cxn.QueryAsync<File>("HeadFile", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
			}
		}
         
		public async Task<Session> GetFile(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@FileId", guid);
				return (await cxn.QueryAsync<Session>("GetFile", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
			}
		}

		public async Task<Guid?> DeleteFile(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@FileId", guid);
                prm.Add("@DeletedFileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
                await cxn.ExecuteAsync("DeleteFile", prm, commandType: CommandType.StoredProcedure);
                return prm.Get<Guid?>("DeletedFileId");
			}
		}

		public async Task<bool> GetStatus()
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				return (await cxn.QueryAsync<int>("GetStatus", commandType: CommandType.StoredProcedure)).Single() >=0;
			}
		}
	}
}