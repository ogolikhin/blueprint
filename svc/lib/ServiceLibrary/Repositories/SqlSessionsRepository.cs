using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Models;
using System.Collections.Generic;

namespace ServiceLibrary.Repositories
{
	public class SqlSessionsRepository : ISessionsRepository
	{
		private readonly string _cxn;

		public SqlSessionsRepository(string cxn)
		{
			_cxn = cxn;
		}

		public virtual async Task<Session> GetSession(Guid guid)
		{
			using (var cxn = new SqlConnection(_cxn))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@SessionId", guid);
				return (await cxn.QueryAsync<Session>("GetSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
			}
		}

		public virtual async Task<IEnumerable<Session>> SelectSessions(int ps, int pn)
		{
			using (var cxn = new SqlConnection(_cxn))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@ps", ps);
				prm.Add("@pn", pn);
				return (await cxn.QueryAsync<Session>("SelectSessions", prm, commandType: CommandType.StoredProcedure));
			}
		}
		public virtual async Task<Guid?[]> BeginSession(int id)
		{
			using (var cxn = new SqlConnection(_cxn))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@UserId", id);
				prm.Add("@BeginTime", DateTime.UtcNow);
				prm.Add("@NewSessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
				prm.Add("@OldSessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
				await cxn.ExecuteAsync("BeginSession", prm, commandType: CommandType.StoredProcedure);
				return new Guid?[] {prm.Get<Guid?>("NewSessionId"), prm.Get<Guid?>("OldSessionId")};
			}
		}

		public virtual async Task EndSession(Guid guid)
		{
			using (var cxn = new SqlConnection(_cxn))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@SessionId", guid);
				prm.Add("@EndTime", DateTime.UtcNow);
				await cxn.ExecuteAsync("EndSession", prm, commandType: CommandType.StoredProcedure);
			}
		}
	}
}