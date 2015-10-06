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
		public virtual async Task<Guid?> CreateSession(int ext)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				var session = new Session(ext);
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

		public virtual async Task<Session> ReadSession(Guid guid, int ext)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@SessionId", guid);
				prm.Add("@Ext", ext);
				return (await cxn.QueryAsync<Session>("ReadSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
			}
		}

		public virtual async Task<Guid?> DeleteSession(Guid guid)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStoreDatabase))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@SessionId", guid);
				await cxn.ExecuteAsync("DeleteSession", prm, commandType: CommandType.StoredProcedure);
				return guid;
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