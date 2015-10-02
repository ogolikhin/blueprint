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
		public async Task<Guid?> CreateSession(Session session)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@UserId", session.UserId);
				prm.Add("@BeginTime", session.BeginTime);
				prm.Add("@EndTime", session.EndTime);
				prm.Add("@SessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
				await cxn.ExecuteAsync("CreateSession", prm, commandType: CommandType.StoredProcedure);
				session.SessionId = prm.Get<Guid?>("SessionId") ?? default(Guid);
            return session.SessionId;
			}
		}

		public async Task<Guid?> UpdateSession(Session session)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@SessionId", session.SessionId);
				prm.Add("@EndTime", session.EndTime);
				await cxn.ExecuteAsync("UpdateSession", prm, commandType: CommandType.StoredProcedure);
				return session.SessionId;
			}
		}

		public async Task<Session> ReadSession(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@SessionId", guid);
				return (await cxn.QueryAsync<Session>("HeadFile", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
			}
		}

		public async Task<Guid?> DeleteSession(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@SessionId", guid);
				prm.Add("@DeletedSessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
				await cxn.ExecuteAsync("DeleteSession", prm, commandType: CommandType.StoredProcedure);
				return prm.Get<Guid?>("DeletedSessionId");
			}
		}

		public async Task<IEnumerable<Session>> SelectSessions(int ps, int pn)
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