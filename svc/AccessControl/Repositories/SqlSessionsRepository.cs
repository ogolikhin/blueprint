using System;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using System.Collections.Generic;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models;

namespace AccessControl.Repositories
{
    public class SqlSessionsRepository : ISessionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlSessionsRepository()
            : this(new SqlConnectionWrapper(WebApiConfig.AdminStorage))
        {
        }

        internal SqlSessionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<Session> GetSession(Guid guid)
        {
            var prm = new DynamicParameters();
            prm.Add("@SessionId", guid);

            return (await _connectionWrapper.QueryAsync<Session>("[AdminStore].GetSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<Session> GetUserSession(int uid)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserId", uid);

            return (await _connectionWrapper.QueryAsync<Session>("[AdminStore].GetUserSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ps">Page Size</param>
        /// <param name="pn">Page Number</param>
        /// <returns></returns>
        public async Task<IEnumerable<Session>> SelectSessions(int ps, int pn)
        {
            var prm = new DynamicParameters();
            prm.Add("@ps", ps);
            prm.Add("@pn", pn);

            return await _connectionWrapper.QueryAsync<Session>("[AdminStore].SelectSessions", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<Session> BeginSession(int userId, string userName, int licenseLevel, bool isSso, Action<Guid> oldSessionIdAction)
        {
            var now = DateTime.UtcNow;
            var prm = new DynamicParameters();
            prm.Add("@UserId", userId);
            prm.Add("@BeginTime", now);
            prm.Add("@EndTime", now.AddSeconds(WebApiConfig.SessionTimeoutInterval));
            prm.Add("@UserName", userName);
            prm.Add("@LicenseLevel", licenseLevel);
            prm.Add("@IsSso", isSso);
            prm.Add("@LicenseLockTimeMinutes", WebApiConfig.LicenseHoldTime);
            prm.Add("@OldSessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
            var result = (await _connectionWrapper.QueryAsync<Session>("[AdminStore].BeginSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            var oldSessionId = prm.Get<Guid?>("OldSessionId");
            if (oldSessionId.HasValue)
            {
                oldSessionIdAction(oldSessionId.Value);
            }

            return result;
        }

        public async Task<Session> ExtendSession(Guid guid)
        {
            var now = DateTime.UtcNow;
            var prm = new DynamicParameters();
            prm.Add("@SessionId", guid);
            prm.Add("@EndTime", now.AddSeconds(WebApiConfig.SessionTimeoutInterval));

            return (await _connectionWrapper.QueryAsync<Session>("[AdminStore].ExtendSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<Session> EndSession(Guid guid, DateTime? timeoutTime = null)
        {
            var prm = new DynamicParameters();
            prm.Add("@SessionId", guid);
            prm.Add("@EndTime", DateTime.UtcNow);
            prm.Add("@TimeoutTime", timeoutTime);
            prm.Add("@LicenseLockTimeMinutes", WebApiConfig.LicenseHoldTime);

            return (await _connectionWrapper.QueryAsync<Session>("[AdminStore].EndSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }
    }
}
