﻿using System;
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
        internal readonly ISqlConnectionWrapper _connectionWrapper;

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
            return (await _connectionWrapper.QueryAsync<Session>("GetSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<Session> GetUserSession(int uid)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserId", uid);
            return (await _connectionWrapper.QueryAsync<Session>("GetUserSession", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
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
            return (await _connectionWrapper.QueryAsync<Session>("SelectSessions", prm, commandType: CommandType.StoredProcedure));
        }

        public async Task<Guid?[]> BeginSession(int userId, string userName, int licenseLevel, bool isSso)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserId", userId);
            prm.Add("@BeginTime", DateTime.UtcNow);
            prm.Add("@UserName", userName);
            prm.Add("@LicenseLevel", licenseLevel);
            prm.Add("@IsSso", isSso);
            prm.Add("@NewSessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
            prm.Add("@OldSessionId", dbType: DbType.Guid, direction: ParameterDirection.Output);
            prm.Add("@licenseLockTimeMinutes", WebApiConfig.LicenseHoldTime);
            await _connectionWrapper.ExecuteAsync("BeginSession", prm, commandType: CommandType.StoredProcedure);
            return new[] {prm.Get<Guid?>("NewSessionId"), prm.Get<Guid?>("OldSessionId")};
        }

        public async Task EndSession(Guid guid, bool timeout)
        {
            var prm = new DynamicParameters();
            prm.Add("@SessionId", guid);
            prm.Add("@EndTime", DateTime.UtcNow);
            prm.Add("@Timeout", timeout ? 1 : 0);
            prm.Add("@licenseLockTimeMinutes", WebApiConfig.LicenseHoldTime);
            await _connectionWrapper.ExecuteAsync("EndSession", prm, commandType: CommandType.StoredProcedure);
        }
    }
}
