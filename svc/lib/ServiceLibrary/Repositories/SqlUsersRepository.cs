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
        private readonly ISqlConnectionWrapper ConnectionWrapper;

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

        public Task<IEnumerable<UserInfo>> GetUserInfosFromGroupsAsync(IEnumerable<int> groupIds)
        {
            var parameters = new DynamicParameters();
            var groupIdsTable = SqlConnectionWrapper.ToDataTable(groupIds);

            parameters.Add("@groupIds", groupIdsTable);

            return ConnectionWrapper.QueryAsync<UserInfo>("GetUserInfosFromGroups", parameters, commandType: CommandType.StoredProcedure);
        }

        public Task<IEnumerable<int>> FindNonExistentUsersAsync(IEnumerable<int> userIds)
        {
            var parameters = new DynamicParameters();
            var userIdsTable = SqlConnectionWrapper.ToDataTable(userIds);

            parameters.Add("@userIds", userIdsTable);

            return ConnectionWrapper.QueryAsync<int>("FindNonExistentUsers", parameters, commandType: CommandType.StoredProcedure);
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
            var prm = new DynamicParameters();
            prm.Add("@contextUser", contextUser);
            prm.Add("@userId", sessionUserId);

            return (await ConnectionWrapper.QueryAsync<bool>("IsInstanceAdmin", prm, commandType: CommandType.StoredProcedure)).SingleOrDefault();            
        }
    }
}
