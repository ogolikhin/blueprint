using System;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using AccessControl.Models;
using System.Collections.Generic;
using ServiceLibrary.Repositories;

namespace AccessControl.Repositories
{
    public class SqlSessionsRepository : ISessionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlSessionsRepository(string cxn)
            : this(new SqlConnectionWrapper(cxn))
        {
        }

        internal SqlSessionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public virtual async Task<Session> GetSession(Guid guid)
        {
            var prm = new DynamicParameters();
            prm.Add("@SessionId", guid);
            return (await _connectionWrapper.QueryAsync<Session>("GetSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ps">Page Size</param>
        /// <param name="pn">Page Number</param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<Session>> SelectSessions(int ps, int pn)
        {
            var prm = new DynamicParameters();
            prm.Add("@ps", ps);
            prm.Add("@pn", pn);
            return (await _connectionWrapper.QueryAsync<Session>("SelectSessions", prm, commandType: CommandType.StoredProcedure));
        }
        public virtual async Task<Guid?[]> BeginSession(int id)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserId", id);
            prm.Add("@BeginTime", DateTime.UtcNow);
            prm.Add("@NewSessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
            prm.Add("@OldSessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
            await _connectionWrapper.ExecuteAsync("BeginSession", prm, commandType: CommandType.StoredProcedure);
            return new[] {prm.Get<Guid?>("NewSessionId"), prm.Get<Guid?>("OldSessionId")};
        }

        public virtual async Task EndSession(Guid guid)
        {
            var prm = new DynamicParameters();
            prm.Add("@SessionId", guid);
            prm.Add("@EndTime", DateTime.UtcNow);
            await _connectionWrapper.ExecuteAsync("EndSession", prm, commandType: CommandType.StoredProcedure);
        }
    }
}
