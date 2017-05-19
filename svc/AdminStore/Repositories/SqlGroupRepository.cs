using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    public class SqlGroupRepository : IGroupRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlGroupRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlGroupRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<QueryResult<GroupDto>> GetGroupsAsync(int userId, TabularData tabularData, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && tabularData.Sorting != null)
            {
                orderField = sort(tabularData.Sorting);
            }
            var parameters = new DynamicParameters();
            if (userId > 0)
            {
                parameters.Add("@UserId", userId);
            }
            parameters.Add("@Offset", tabularData.Pagination.Offset);
            parameters.Add("@Limit", tabularData.Pagination.Limit);
            parameters.Add("@OrderField", orderField);
            parameters.Add("@Search", tabularData.Search);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var userGroups = await _connectionWrapper.QueryAsync<Group>("GetGroups", parameters, commandType: CommandType.StoredProcedure);
            var total = parameters.Get<int?>("Total");

            var mappedGroups = GroupMapper.Map(userGroups);

            var queryDataResult = new QueryResult<GroupDto>() { Items = mappedGroups, Total = total.Value };
            return queryDataResult;
        }

        public async Task<int> DeleteGroupsAsync(OperationScope body, string search)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@GroupsIds", SqlConnectionWrapper.ToDataTable(body.Ids));
            parameters.Add("@Search", search);
            parameters.Add("@SelectAll", body.SelectAll);
            var result = await _connectionWrapper.ExecuteScalarAsync<int>("DeleteGroups", parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<GroupDto> GetGroupDetailsAsync(int groupId)
        {
            var group = await GetInternalGroupDetails(groupId);
            return GroupMapper.Map(group);
        }

        private async Task<Group> GetInternalGroupDetails(int groupId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@GroupId", groupId);

            var result =
                await
                    _connectionWrapper.QueryAsync<Group>("GetGroupDetails", parameters,
                        commandType: CommandType.StoredProcedure);
            var enumerable = result as IList<Group> ?? result.ToList();
            return enumerable.Any() ? enumerable.First() : new Group();
        }


    }
}