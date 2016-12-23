using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class SqlUsersRepository : IUsersRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public SqlUsersRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlUsersRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        public async Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds)
        {
            var userInfosPrm = new DynamicParameters();
            var userIdsTable = SqlConnectionWrapper.ToDataTable(userIds, "Int32Collection", "Int32Value");
            userInfosPrm.Add("@userIds", userIdsTable);
            return await ConnectionWrapper.QueryAsync<UserInfo>("GetUserInfos", userInfosPrm, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<UserInfo>> GetUsersByEmail(string email, bool? guestsOnly = false)
        {
            var userInfosPrm = new DynamicParameters();
            userInfosPrm.Add("@Email", email);
            userInfosPrm.Add("@GuestsOnly", guestsOnly);
            return await ConnectionWrapper.QueryAsync<UserInfo>("GetUsersByEmail", userInfosPrm, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> IsInstanceAdmin(bool contextUser, int sessionUserId)
        {
            //
            var prm = new DynamicParameters();
            prm.Add("@contextUser", contextUser);
            prm.Add("@userId", sessionUserId);
            return (await _connectionWrapper.QueryAsync<bool>("IsInstanceAdmin", prm, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }
    }
}