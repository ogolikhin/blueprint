using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using AccessControl.Models;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace AccessControl.Repositories
{
	public class SqlSessionsRepository : ISessionsRepository
	{
		public virtual async Task<Guid?> BeginSession(int id)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@UserId", id);
				prm.Add("@BeginTime", DateTime.UtcNow);
				prm.Add("@SessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
				await cxn.ExecuteAsync("BeginSession", prm, commandType: CommandType.StoredProcedure);
				return prm.Get<Guid?>("SessionId");
			}
		}

		public virtual async Task EndSession(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@SessionId", guid);
				prm.Add("@EndTime", DateTime.UtcNow);
				await cxn.ExecuteAsync("EndSession", prm, commandType: CommandType.StoredProcedure);
			}
		}

		public virtual async Task<IEnumerable<Session>> SelectSessions(int ps, int pn)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@ps", ps);
				prm.Add("@pn", pn);
				return (await cxn.QueryAsync<Session>("SelectSessions", prm, commandType: CommandType.StoredProcedure));
			}
		}
	}
}