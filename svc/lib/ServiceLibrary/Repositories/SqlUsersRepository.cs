using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Repositories
{
    public class SqlUsersRepository : IUsersRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private const int FakeUserId = -10;
        private const int FakeGroupId = -20;

        public SqlUsersRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlUsersRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds, IDbTransaction transaction = null)
        {
            var userInfosPrm = new DynamicParameters();
            var userIdsTable = SqlConnectionWrapper.ToDataTable(userIds, "Int32Collection", "Int32Value");
            userInfosPrm.Add("@userIds", userIdsTable);

            if (transaction != null)
            {
                return await transaction.Connection.QueryAsync<UserInfo>("GetUserInfos", userInfosPrm, transaction, commandType: CommandType.StoredProcedure);
            }
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


        public async Task<IEnumerable<SqlGroup>> GetExistingGroupsByNamesAsync(IEnumerable<string> groupNames, bool instanceOnly)
        {
            var prm = new DynamicParameters();
            prm.Add("@groupNames", SqlConnectionWrapper.ToStringDataTable(groupNames));
            prm.Add("@instanceOnly", instanceOnly);

            return await _connectionWrapper.QueryAsync<SqlGroup>("GetExistingGroupsByNames", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<SqlGroup>> GetExistingGroupsByIds(IEnumerable<int> groupIds, bool instanceOnly, IDbTransaction transaction = null)
        {
            var prm = new DynamicParameters();
            prm.Add("@groupIds", SqlConnectionWrapper.ToDataTable(groupIds));
            prm.Add("@instanceOnly", instanceOnly);

            if (transaction != null)
            {
                return await transaction.Connection.QueryAsync<SqlGroup>("GetExistingGroupsByIds", prm, transaction, commandType: CommandType.StoredProcedure);
            }
            return await _connectionWrapper.QueryAsync<SqlGroup>("GetExistingGroupsByIds", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<SqlUser>> GetExistingUsersByNamesAsync(IEnumerable<string> userNames)
        {
            var prm = new DynamicParameters();
            prm.Add("@userNames", SqlConnectionWrapper.ToStringDataTable(userNames));

            return await _connectionWrapper.QueryAsync<SqlUser>("GetExistingUsersByNames", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<SqlUser>> GetExistingUsersByIdsAsync(IEnumerable<int> userIds)
        {
            var prm = new DynamicParameters();
            prm.Add("@userIds", SqlConnectionWrapper.ToDataTable(userIds));

            return await _connectionWrapper.QueryAsync<SqlUser>("GetExistingUsersByids", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<UserInfo>> GetUserInfoForWorkflowArtifactForAssociatedUserProperty(int artifactId,
            int instancePropertyTypeId,
            int revisionId,
            IDbTransaction transaction = null)
        {
            var userInfos = new List<UserInfo>();

            var prm = new DynamicParameters();
            prm.Add("@artifactId", artifactId);
            prm.Add("@instancePropertyTypeId", instancePropertyTypeId);
            prm.Add("@revisionId", revisionId);

            if (transaction != null)
            {
                var propertyInfos = (await transaction.Connection.QueryAsync<SqlPropertyInfo>("GetPropertyInfoForWorkflowArtifact",
                prm, 
                transaction,
                commandType: CommandType.StoredProcedure)).ToList();
                userInfos.AddRange(await GetUserInfos(propertyInfos, transaction));
                return userInfos;
            }
            else
            {
                var propertyInfos = (await _connectionWrapper.QueryAsync<SqlPropertyInfo>("GetPropertyInfoForWorkflowArtifact",
                    prm,
                    commandType: CommandType.StoredProcedure)).ToList();
                userInfos.AddRange(await GetUserInfos(propertyInfos));
                return userInfos;
            }
        }

        private async Task<IEnumerable<UserInfo>> GetUserInfos(List<SqlPropertyInfo> propertyInfos, IDbTransaction transaction = null)
        {
            var userInfos = new List<UserInfo>();
            if (propertyInfos.Count > 0)
            {
                foreach (var sqlPropertyInfo in propertyInfos)
                {
                    if (sqlPropertyInfo.PrimitiveType == (int)PropertyPrimitiveType.User)
                    {
                        var userGroups = PropertyHelper.ParseUserGroups(sqlPropertyInfo.PropertyValue);
                        if (userGroups != null)
                        {
                            var userIds =
                                userGroups.Where(g => g != null && (g.IsGroup == null || !g.IsGroup.Value))
                                    .Select(g => g.Id.GetValueOrDefault()).ToArray();
                            if (userIds.Length > 0)
                            {
                                userInfos.AddRange((await GetUserInfos(userIds, transaction)).ToList());
                            }

                            var groupIds =
                                userGroups.Where(g => g != null && (g.IsGroup.GetValueOrDefault(false)))
                                    .Select(g => g.Id.GetValueOrDefault()).ToArray();

                            if (groupIds.Length > 0)
                            {
                                var groups = await GetExistingGroupsByIds(groupIds, false);
                                userInfos.AddRange(groups.Select(g => new UserInfo
                                {
                                    UserId = FakeGroupId,
                                    Email = g.Email
                                }));
                            }
                        }
                    }
                    else if (sqlPropertyInfo.PrimitiveType == (int)PropertyPrimitiveType.Text)
                    {
                        var emails = PropertyHelper.ParseEmails(sqlPropertyInfo.PropertyValue);
                        userInfos.AddRange(emails.Select(email => new UserInfo
                        {
                            UserId = FakeUserId,
                            Email = string.IsNullOrWhiteSpace(email) ? email : email.Trim()
                        }));
                    }
                }
            }
            return userInfos;
        }
    }
}
