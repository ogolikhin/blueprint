using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
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
            if (tabularData.Search != null)
            {
                tabularData.Search = UsersHelper.ReplaceWildcardCharacters(tabularData.Search);
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


        public async Task<int> AddGroupAsync(GroupDto group)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", group.Name);
            parameters.Add("@Email", group.Email);
            parameters.Add("@Source", group.GroupSource);
            parameters.Add("@LicenseId", (int)group.License);
            parameters.Add("@ProjectId", group.ProjectId);

            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var groupId = await _connectionWrapper.ExecuteScalarAsync<int>("AddGroup", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfCreatingGroup);

                    case (int)SqlErrorCodes.GroupWithNameAndLicenseIdExist:
                        throw new BadRequestException(ErrorMessages.GroupAlreadyExist);

                    case (int)SqlErrorCodes.GroupWithNameAndScopeExist:
                        throw new BadRequestException(ErrorMessages.GroupAlreadyExist);

                    default:
                        return groupId;
                }
            }
            return groupId;
        }


        public async Task<Group> GetGroupDetailsAsync(int groupId)
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

        public async Task<QueryResult<GroupUser>> GetGroupUsersAsync(int groupId, TabularData tabularData, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && tabularData.Sorting != null)
            {
                orderField = sort(tabularData.Sorting);
            }
            var parameters = new DynamicParameters();
            if (groupId > 0)
            {
                parameters.Add("@GroupId", groupId);
            }
            parameters.Add("@Offset", tabularData.Pagination.Offset);
            parameters.Add("@Limit", tabularData.Pagination.Limit);
            parameters.Add("@OrderField", orderField);
            parameters.Add("@Search", tabularData.Search);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var userGroups = await _connectionWrapper.QueryAsync<GroupUser>("GetUsersAndGroups", parameters, commandType: CommandType.StoredProcedure);
            var total = parameters.Get<int?>("Total");


            var queryDataResult = new QueryResult<GroupUser>() { Items = userGroups, Total = total.Value };
            return queryDataResult;
        }
    }
}