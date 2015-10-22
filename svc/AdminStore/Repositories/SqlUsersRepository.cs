using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using AdminStore.Models;
using System.Collections.Generic;

namespace AdminStore.Repositories
{
	public class SqlUsersRepository : IUsersRepository
	{
		public virtual async Task<User> GetUser(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@UserId", guid);
				return (await cxn.QueryAsync<User>("GetUser", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
			}
		}

		public virtual async Task<Guid?[]> BeginUser(int id)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@UserId", id);
				prm.Add("@BeginTime", DateTime.UtcNow);
				prm.Add("@NewUserId", dbType: DbType.Guid, direction: ParameterDirection.Output);
				prm.Add("@OldUserId", dbType: DbType.Guid, direction: ParameterDirection.Output);
				await cxn.ExecuteAsync("BeginUser", prm, commandType: CommandType.StoredProcedure);
				return new Guid?[] {prm.Get<Guid?>("NewUserId"), prm.Get<Guid?>("OldUserId")};
			}
		}

		public virtual async Task EndUser(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@UserId", guid);
				prm.Add("@EndTime", DateTime.UtcNow);
				await cxn.ExecuteAsync("EndUser", prm, commandType: CommandType.StoredProcedure);
			}
		}

		public virtual async Task<IEnumerable<User>> SelectUsers(int ps, int pn)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@ps", ps);
				prm.Add("@pn", pn);
				return (await cxn.QueryAsync<User>("SelectUsers", prm, commandType: CommandType.StoredProcedure));
			}
		}
	}
}