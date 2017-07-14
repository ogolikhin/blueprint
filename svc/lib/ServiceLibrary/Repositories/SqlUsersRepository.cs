using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class SqlUsersRepository : IUsersRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlUsersRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlUsersRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds)
        {
            var userInfosPrm = new DynamicParameters();
            var userIdsTable = SqlConnectionWrapper.ToDataTable(userIds, "Int32Collection", "Int32Value");
            userInfosPrm.Add("@userIds", userIdsTable);

            return await _connectionWrapper.QueryAsync<UserInfo>("GetUserInfos", userInfosPrm, commandType: CommandType.StoredProcedure);
        }

        public Task<IEnumerable<UserInfo>> GetUserInfosFromGroupsAsync(IEnumerable<int> groupIds)
        {
            var parameters = new DynamicParameters();
            var groupIdsTable = SqlConnectionWrapper.ToDataTable(groupIds);

            parameters.Add("@groupIds", groupIdsTable);

            return _connectionWrapper.QueryAsync<UserInfo>("GetUserInfosFromGroups", parameters, commandType: CommandType.StoredProcedure);
        }

        public Task<IEnumerable<int>> FindNonExistentUsersAsync(IEnumerable<int> userIds)
        {
            var parameters = new DynamicParameters();
            var userIdsTable = SqlConnectionWrapper.ToDataTable(userIds);

            parameters.Add("@userIds", userIdsTable);

            return _connectionWrapper.QueryAsync<int>("FindNonExistentUsers", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<UserInfo>> GetUsersByEmail(string email, bool? guestsOnly = false)
        {
            var userInfosPrm = new DynamicParameters();
            userInfosPrm.Add("@Email", email);
            userInfosPrm.Add("@GuestsOnly", guestsOnly);

            return await _connectionWrapper.QueryAsync<UserInfo>("GetUsersByEmail", userInfosPrm, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> IsInstanceAdmin(bool contextUser, int sessionUserId)
        {
            var prm = new DynamicParameters();
            prm.Add("@contextUser", contextUser);
            prm.Add("@userId", sessionUserId);

            return (await _connectionWrapper.QueryAsync<bool>("IsInstanceAdmin", prm, commandType: CommandType.StoredProcedure)).SingleOrDefault();            
        }
    }
}
