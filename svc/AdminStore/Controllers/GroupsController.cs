﻿using System;
using System.Collections.Generic;
using System.Data;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.Enums;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Repositories.ApplicationSettings;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("groups")]
    [BaseExceptionFilter]
    public class GroupsController : BaseApiController
    {
        private readonly IGroupRepository _groupRepository;
        private readonly PrivilegesManager _privilegesManager;
        private readonly IApplicationSettingsRepository _applicationSettingsRepository;
        private readonly IServiceLogRepository _serviceLogRepository;
        private readonly IItemInfoRepository _itemInfoRepository;
        private readonly ISendMessageExecutor _sendMessageExecutor;
        private readonly ISqlHelper _sqlHelper;

        public GroupsController() : this(new SqlGroupRepository(), new SqlPrivilegesRepository(), new ApplicationSettingsRepository(), new ServiceLogRepository(), new SqlItemInfoRepository(), new SendMessageExecutor(), new SqlHelper())
        {
        }

        internal GroupsController(IGroupRepository groupRepository, IPrivilegesRepository privilegesRepository, IApplicationSettingsRepository applicationSettingsRepository, IServiceLogRepository serviceLogRepository, IItemInfoRepository itemInfoRepository, ISendMessageExecutor sendMessageExecutor, ISqlHelper sqlHelper)
        {
            _groupRepository = groupRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
            _applicationSettingsRepository = applicationSettingsRepository;
            _serviceLogRepository = serviceLogRepository;
            _itemInfoRepository = itemInfoRepository;
            _sendMessageExecutor = sendMessageExecutor;
            _sqlHelper = sqlHelper;
        }

        /// <summary>
        /// The method returns all the groups (if no user id is specified), or all the groups except those that are already assigned to the user (if user id is specified).
        /// </summary>
        /// <param name="userId">User's identity</param>
        /// <param name="pagination">Pagination parameters</param>
        /// <param name="sorting">Sorting parameters</param>
        /// <param name="search">The parameter for searching by group name and scope.</param>
        /// <response code="200">OK. The list of groups.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. if used doesn’t have permissions to get groups list.</response>
        [Route("")]
        [SessionRequired]
        [ResponseType(typeof(QueryResult<GroupDto>))]
        public async Task<IHttpActionResult> GetGroups([FromUri]Pagination pagination, [FromUri]Sorting sorting = null, [FromUri] string search = null, int userId = 0)
        {
            pagination.Validate();
            SearchFieldValidator.Validate(search);

            if (userId < 0)
            {
                throw new BadRequestException(ErrorMessages.TheUserIdCanNotBeNegative, ErrorCodes.BadRequest);
            }

            var privileges = userId == 0 ? InstanceAdminPrivileges.ViewGroups : InstanceAdminPrivileges.ManageUsers;
            await _privilegesManager.Demand(Session.UserId, privileges);

            var tabularData = new TabularData { Pagination = pagination, Sorting = sorting, Search = search };
            var result = await _groupRepository.GetGroupsAsync(userId, tabularData, GroupsHelper.SortGroups);

            return Ok(result);
        }

        /// <summary>
        /// The method returns all the groups and users not currently assigned to the group in context.
        /// </summary>
        /// <param name="groupId">Group's identity</param>
        /// <param name="pagination">Pagination parameters</param>
        /// <param name="sorting">Sorting parameters</param>
        /// <param name="search">The parameter for searching by group name and scope.</param>
        /// <response code="200">OK. The list of groups.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. if used doesn’t have permissions to get groups\users list.</response>
        [Route("{groupId:int:min(1)}/usersgroups")]
        [SessionRequired]
        [ResponseType(typeof(QueryResult<GroupUser>))]
        public async Task<IHttpActionResult> GetGroupsAndUsers([FromUri]Pagination pagination, [FromUri]Sorting sorting, string search = null, int groupId = 0)
        {
            pagination.Validate();
            SearchFieldValidator.Validate(search);

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ViewGroups);

            var tabularData = new TabularData { Pagination = pagination, Sorting = sorting, Search = search };
            var result = await _groupRepository.GetGroupUsersAsync(groupId, tabularData, SortingHelper.SortUsergroups);

            return Ok(result);
        }

        /// <summary>
        /// Delete group/groups from the system
        /// </summary>
        /// <param name="scope">list of group ids and selectAll flag</param>
        /// <param name="search">search filter</param>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if used doesn’t have permissions to delete groups</response>
        [HttpPost]
        [SessionRequired]
        [Route("delete")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> DeleteGroups([FromBody] OperationScope scope, string search = null)
        {
            SearchFieldValidator.Validate(search);

            if (scope == null)
            {
                return BadRequest(ErrorMessages.InvalidDeleteGroupsParameters);
            }

            if (scope.IsEmpty())
            {
                return Ok(DeleteResult.Empty);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageGroups);

            var deletedGroupIds = new List<int>();
            Func<IDbTransaction, long, Task> action = async (transaction, transactionId) =>
            {
                var groupIds = await _groupRepository.DeleteGroupsAsync(scope, search, transaction);
                deletedGroupIds.AddRange(groupIds);

                var topRevisionId = await _itemInfoRepository.GetTopRevisionId(transaction);

                var message = new UsersGroupsChangedMessage(new int[0], deletedGroupIds)
                {
                    TransactionId = transactionId,
                    RevisionId = topRevisionId,
                    ChangeType = UsersGroupsChangedType.Delete
                };
                await _sendMessageExecutor.Execute(_applicationSettingsRepository, _serviceLogRepository, message, transaction);
            };
            await RunInTransactionAsync(action);

            return Ok(new DeleteResult { TotalDeleted = deletedGroupIds.Count });
        }

        private async Task RunInTransactionAsync(Func<IDbTransaction, long, Task> action)
        {
            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);
        }

        /// <summary>
        /// Create new group
        /// </summary>
        /// <remarks>
        /// Returns id of the created group.
        /// </remarks>
        /// <response code="201">OK. The group is created.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for creating the group.</response>
        [HttpPost]
        [SessionRequired]
        [ResponseType(typeof(int))]
        [Route("")]
        public async Task<HttpResponseMessage> CreateGroup([FromBody] GroupDto group)
        {
            if (group == null)
            {
                throw new BadRequestException(ErrorMessages.GroupModelIsEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageGroups);

            GroupValidator.ValidateModel(group, OperationMode.Create);

            int groupId = 0;
            Func<IDbTransaction, long, Task> action = async (transaction, transactionId) =>
            {
                groupId = await _groupRepository.AddGroupAsync(group, transaction);
                var topRevisionId = await _itemInfoRepository.GetTopRevisionId(transaction);

                var groupIds = new[]
                {
                    groupId
                };
                var message = new UsersGroupsChangedMessage(new int[0], groupIds)
                {
                    TransactionId = transactionId,
                    RevisionId = topRevisionId,
                    ChangeType = UsersGroupsChangedType.Create
                };
                await _sendMessageExecutor.Execute(_applicationSettingsRepository, _serviceLogRepository, message, transaction);
            };
            await RunInTransactionAsync(action);

            return Request.CreateResponse(HttpStatusCode.Created, groupId);
        }

        /// <summary>
        /// Get group details by group identifier
        /// </summary>
        /// <param name="groupId">Group's identity</param>
        /// <returns>
        /// <response code="200">OK. Returns the specified group.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="404">Not Found. The group with the provided Id was not found.</response>
        /// <response code="403">User doesn’t have permission to view groups.</response>
        /// </returns>
        [SessionRequired]
        [Route("{groupId:int:min(1)}")]
        [ResponseType(typeof(GroupDto))]
        public async Task<IHttpActionResult> GetGroup(int groupId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ViewGroups);

            var groupDetails = await _groupRepository.GetGroupDetailsAsync(groupId);
            if (groupDetails.Id == 0)
            {
                throw new ResourceNotFoundException(ErrorMessages.GroupNotExist, ErrorCodes.ResourceNotFound);
            }

            return Ok(groupDetails);
        }

        /// <summary>
        /// Update group
        /// </summary>
        /// <param name="groupId">Group's identity</param>
        /// <param name="group">Group's model</param>
        /// <remarks>
        /// Returns Ok result.
        /// </remarks>
        /// <response code="200">OK. The group is updated.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for updating the group.</response>
        /// <response code="404">NotFound. The group with the current groupId doesn’t exist or removed from the system.</response>
        /// <response code="409">Conflict. The current version from the request doesn’t match the current version in DB.</response>
        [HttpPut]
        [SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        [Route("{groupId:int:min(1)}")]
        public async Task<IHttpActionResult> UpdateGroup(int groupId, [FromBody] GroupDto group)
        {
            if (group == null)
            {
                throw new BadRequestException(ErrorMessages.GroupModelIsEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageGroups);

            var existingGroup = await _groupRepository.GetGroupDetailsAsync(groupId);
            if (existingGroup == null || existingGroup.Id == 0)
            {
                throw new ResourceNotFoundException(ErrorMessages.GroupNotExist, ErrorCodes.ResourceNotFound);
            }

            GroupValidator.ValidateModel(group, OperationMode.Edit, existingGroup.ProjectId);

            Func<IDbTransaction, long, Task> action = async (transaction, transactionId) =>
            {
                await _groupRepository.UpdateGroupAsync(groupId, group, transaction);
                var topRevisionId = await _itemInfoRepository.GetTopRevisionId(transaction);

                var groupIds = new[]
                {
                    groupId
                };
                var message = new UsersGroupsChangedMessage(new int[0], groupIds)
                {
                    TransactionId = transactionId,
                    RevisionId = topRevisionId,
                    ChangeType = UsersGroupsChangedType.Update
                };
                await _sendMessageExecutor.Execute(_applicationSettingsRepository, _serviceLogRepository, message, transaction);
            };
            await RunInTransactionAsync(action);

            return Ok();
        }

        /// <summary>
        /// Get group's members
        /// </summary>
        /// <param name="groupId">Group's identity</param>
        /// <param name="pagination">Pagination parameters</param>
        /// <param name="sorting">Sorting parameters</param>
        /// <response code="200">OK. The list of members.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. If used doesn’t have permissions to get group's members.</response>
        /// <response code="404">NotFound. If group with grouId doesn’t exists or removed from the system.</response>
        [Route("{groupId:int:min(1)}/members")]
        [SessionRequired]
        [ResponseType(typeof(QueryResult<GroupUser>))]
        public async Task<IHttpActionResult> GetGroupMembers(int groupId, [FromUri]Pagination pagination, [FromUri]Sorting sorting)
        {
            pagination.Validate();

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ViewGroups);

            var tabularData = new TabularData { Pagination = pagination, Sorting = sorting };
            var result = await _groupRepository.GetGroupMembersAsync(groupId, tabularData, SortingHelper.SortUsergroups);

            return Ok(result);
        }

        /// <summary>
        /// Remove members from a group
        /// </summary>
        /// <param name="groupId">group's id</param>
        /// <param name="scope">list of groups and users ids, selectAll flag</param>
        /// <response code="200">OK. Count of deleted members from a group.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if used doesn’t have permissions to remove members from a group</response>
        /// <response code="404">NotFound. if the group with groupId doesn’t exists or removed from the system.</response>
        [HttpPost]
        [SessionRequired]
        [Route("{groupId:int:min(1)}/members")]
        [ResponseType(typeof(DeleteResult))]
        public async Task<IHttpActionResult> RemoveMembersFromGroup(int groupId, [FromBody] AssignScope scope)
        {
            if (scope == null)
            {
                throw new BadRequestException(ErrorMessages.InvalidGroupMembersParameters, ErrorCodes.BadRequest);
            }

            if (scope.IsEmpty())
            {
                return Ok(DeleteResult.Empty);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageGroups);

            var result = await _groupRepository.DeleteMembersFromGroupAsync(groupId, scope);

            return Ok(new DeleteResult { TotalDeleted = result });
        }

        /// <summary>
        /// Assign group/user to the group
        /// </summary>
        /// <param name="groupId">Group's identity</param>
        /// <param name="scope">Group's model</param>
        /// <param name="search">search by displayName or group name</param>
        /// <remarks>
        /// Returns Ok result.
        /// </remarks>
        /// <response code="200">OK. The group's/user's were assigned to the Group which Id = groupId .</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions to assign group's/users's</response>
        /// <response code="404">NotFound. The group with the current groupId doesn’t exist or removed from the system.</response>
        [HttpPost]
        [SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        [Route("{groupId:int:min(1)}/assign")]
        public async Task<IHttpActionResult> AssignMembers(int groupId, AssignScope scope, string search = null)
        {
            SearchFieldValidator.Validate(search);

            if (scope == null || scope.IsEmpty())
            {
                throw new BadRequestException(ErrorMessages.AssignMemberScopeEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageGroups);
            var result = await _groupRepository.AssignMembers(groupId, scope, search);

            return Ok(new AssignResult() { TotalAssigned = result });
        }


        /// <summary>
        /// Get the list of groups for the project
        /// </summary>
        /// <remarks>
        /// Get the list of groups for the project's id and also instance level groups that have no assignments
        /// </remarks>
        /// <response code="200">OK list groups returned</response>
        /// <response code="400">Parameters are invalid</response>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if doesn’t have permissions to get the list of groups (instance and project)</response>
        /// <response code="404">NotFound. If groups with projectId don’t exists or removed from the system.</response>
        [HttpGet, NoCache]
        [Route("{projectId:int:min(1)}/available"), SessionRequired]
        [ResponseType(typeof(QueryResult<GroupDto>))]
        public async Task<IHttpActionResult> GetProjectGroupsAsync(int projectId, [FromUri] Pagination pagination,
            [FromUri] Sorting sorting, string search = null)
        {
            pagination.Validate();
            SearchFieldValidator.Validate(search);

            await _privilegesManager.DemandAny(Session.UserId, projectId, InstanceAdminPrivileges.AccessAllProjectsAdmin,
                     ProjectAdminPrivileges.ViewGroupsAndRoles);

            var tabularData = new TabularData { Pagination = pagination, Sorting = sorting, Search = search };
            var result =
                await _groupRepository.GetProjectGroupsAsync(projectId, tabularData, SortingHelper.SortProjectGroups);

            return Ok(result);
        }

    }
}
