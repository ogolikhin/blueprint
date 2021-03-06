﻿using AdminStore.Helpers;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    public class SqlGroupRepository : IGroupRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

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

            if (!string.IsNullOrWhiteSpace(tabularData.Search))
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
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var userGroups = await _connectionWrapper.QueryAsync<Group>("GetGroups", parameters, commandType: CommandType.StoredProcedure);
            var total = parameters.Get<int?>("Total");
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.UserLoginNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.UserNotExist, ErrorCodes.ResourceNotFound);
                }
            }

            var mappedGroups = GroupMapper.Map(userGroups);

            var queryDataResult = new QueryResult<GroupDto> { Items = mappedGroups, Total = total.Value };

            return queryDataResult;
        }

        public async Task<List<int>> DeleteGroupsAsync(OperationScope body, string search, IDbTransaction transaction)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@GroupsIds", SqlConnectionWrapper.ToDataTable(body.Ids));
            parameters.Add("@Search", search);
            parameters.Add("@SelectAll", body.SelectAll);

            List<int> result;
            if (transaction == null)
            {
                result = (await _connectionWrapper.QueryAsync<int>("DeleteGroups", parameters, commandType: CommandType.StoredProcedure)).ToList();
            }
            else
            {
                result = (await transaction.Connection.QueryAsync<int>("DeleteGroups", parameters, transaction, commandType: CommandType.StoredProcedure)).ToList();
            }
            return result;
        }

        public async Task<int> AddGroupAsync(GroupDto group, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", group.Name);
            parameters.Add("@Email", group.Email);
            parameters.Add("@Source", group.Source);
            parameters.Add("@LicenseId", (int)group.LicenseType);
            parameters.Add("@ProjectId", group.ProjectId);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            int groupId;
            if (transaction == null)
            {
                groupId = await _connectionWrapper.ExecuteScalarAsync<int>("AddGroup", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                groupId = await transaction.Connection.ExecuteScalarAsync<int>("AddGroup", parameters, transaction, commandType: CommandType.StoredProcedure);
            }

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfCreatingGroup);

                    case (int)SqlErrorCodes.GroupWithNameAndScopeExist:
                        throw new BadRequestException(ErrorMessages.GroupAlreadyExist);

                    case (int)SqlErrorCodes.CurrentProjectIsNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.TheProjectDoesNotExist, ErrorCodes.ResourceNotFound);

                    default:
                        return groupId;
                }
            }

            return groupId;
        }

        public async Task UpdateGroupAsync(int groupId, GroupDto group, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@GroupId", groupId);
            parameters.Add("@Name", group.Name);
            parameters.Add("@Email", group.Email);
            parameters.Add("@LicenseId", (int)group.LicenseType);
            parameters.Add("@CurrentVersion", group.CurrentVersion);

            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteScalarAsync<int>("UpdateGroup", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await transaction.Connection.ExecuteScalarAsync<int>("UpdateGroup", parameters, transaction, commandType: CommandType.StoredProcedure);
            }

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfUpdatingGroup);

                    case (int)SqlErrorCodes.GroupWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.GroupNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.GroupVersionsNotEqual:
                        throw new ConflictException(ErrorMessages.UserVersionsNotEqual);

                    case (int)SqlErrorCodes.GroupCanNotBeUpdatedWithExistingScope:
                        throw new BadRequestException(ErrorMessages.ImpossibleChangeLicenseInGroupWithScope);

                    case (int)SqlErrorCodes.GroupWithNameAndLicenseIdExist:
                        throw new BadRequestException(ErrorMessages.GroupAlreadyExist);

                    case (int)SqlErrorCodes.GroupWithNameAndScopeExist:
                        throw new BadRequestException(ErrorMessages.GroupAlreadyExist);
                }
            }
        }

        public async Task<GroupDto> GetGroupDetailsAsync(int groupId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@GroupId", groupId);

            var result =
                await
                    _connectionWrapper.QueryAsync<Group>("GetGroupDetails", parameters,
                        commandType: CommandType.StoredProcedure);
            var enumerable = result as IList<Group> ?? result.ToList();
            var group = enumerable.Any() ? enumerable.First() : new Group();

            return GroupMapper.Map(group);
        }

        public async Task<QueryResult<GroupUser>> GetGroupUsersAsync(int groupId, TabularData tabularData, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && tabularData.Sorting != null)
            {
                orderField = sort(tabularData.Sorting);
            }

            if (!string.IsNullOrWhiteSpace(tabularData.Search))
            {
                tabularData.Search = UsersHelper.ReplaceWildcardCharacters(tabularData.Search);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@GroupId", groupId);
            parameters.Add("@Offset", tabularData.Pagination.Offset);
            parameters.Add("@Limit", tabularData.Pagination.Limit);
            parameters.Add("@OrderField", orderField);
            parameters.Add("@Search", tabularData.Search);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var userGroups = await _connectionWrapper.QueryAsync<GroupUser>("GetUsersAndGroups", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GroupWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.GroupNotExist, ErrorCodes.ResourceNotFound);
                }
            }

            var total = parameters.Get<int?>("Total");

            var queryDataResult = new QueryResult<GroupUser> { Items = userGroups, Total = total.Value };

            return queryDataResult;
        }

        public async Task<QueryResult<GroupUser>> GetGroupMembersAsync(int groupId, TabularData tabularData, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && tabularData.Sorting != null)
            {
                orderField = sort(tabularData.Sorting);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@GroupId", groupId);
            parameters.Add("@Offset", tabularData.Pagination.Offset);
            parameters.Add("@Limit", tabularData.Pagination.Limit);
            parameters.Add("@OrderField", orderField);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var userGroups = await _connectionWrapper.QueryAsync<GroupUser>("GetGroupMembers", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GroupWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.GroupNotExist);
                }
            }

            var total = parameters.Get<int?>("Total");

            var queryDataResult = new QueryResult<GroupUser> { Items = userGroups, Total = total.Value };

            return queryDataResult;
        }

        public async Task<int> DeleteMembersFromGroupAsync(int groupId, AssignScope body)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@GroupId", groupId);
            parameters.Add("@GroupsIds", SqlConnectionWrapper.ToDataTable(GroupsHelper.ParsingTypesToUserTypeArray(body.Members, UserType.Group)));
            parameters.Add("@UsersIds", SqlConnectionWrapper.ToDataTable(GroupsHelper.ParsingTypesToUserTypeArray(body.Members, UserType.User)));
            parameters.Add("@SelectAll", body.SelectAll);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.ExecuteScalarAsync<int>("DeleteMembersFromGroup", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfRemovingMembersFromGroup);

                    case (int)SqlErrorCodes.GroupWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.GroupNotExist, ErrorCodes.ResourceNotFound);
                }
            }

            return result;
        }

        public async Task<int> AssignMembers(int groupId, AssignScope scope, string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@GroupId", groupId);
            parameters.Add("@GroupsIds", SqlConnectionWrapper.ToDataTable(GroupsHelper.ParsingTypesToUserTypeArray(scope.Members, UserType.Group)));
            parameters.Add("@UsersIds", SqlConnectionWrapper.ToDataTable(GroupsHelper.ParsingTypesToUserTypeArray(scope.Members, UserType.User)));
            parameters.Add("@SelectAll", scope.SelectAll);
            parameters.Add("@Search", search);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = await _connectionWrapper.ExecuteScalarAsync<int>("AssignGroupMembers", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GroupWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.GroupNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfUpdatingGroup);
                }
            }

            return result;
        }

        public async Task<QueryResult<GroupDto>> GetProjectGroupsAsync(int projectId, TabularData tabularData,
            Func<Sorting, string> sort = null)
        {
            if (projectId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            var orderField = string.Empty;
            if (sort != null && tabularData.Sorting != null)
            {
                orderField = sort(tabularData.Sorting);
            }

            if (!string.IsNullOrWhiteSpace(tabularData.Search))
            {
                tabularData.Search = UsersHelper.ReplaceWildcardCharacters(tabularData.Search);
            }

            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@Offset", tabularData.Pagination.Offset);
            prm.Add("@Limit", tabularData.Pagination.Limit);
            prm.Add("@OrderField", orderField);
            prm.Add("@Search", tabularData.Search);
            prm.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            prm.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result =
                (await
                    _connectionWrapper.QueryAsync<Group>("GetAvailableGroupsForProject", prm,
                        commandType: CommandType.StoredProcedure)).ToList();

            var errorCode = prm.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.ProjectWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);

                }
            }

            var total = prm.Get<int?>("Total");

            var queryDataResult = new QueryResult<GroupDto> { Items = GroupMapper.Map(result), Total = total ?? 0 };

            return queryDataResult;
        }
    }
}
