using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.Enums;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("groups")]
    [BaseExceptionFilter]
    public class GroupsController : BaseApiController
    {
        internal readonly IGroupRepository _groupRepository;
        internal readonly PrivilegesManager _privilegesManager;

        public GroupsController() : this(new SqlGroupRepository(), new SqlPrivilegesRepository())
        {
        }

        internal GroupsController(IGroupRepository groupRepository, IPrivilegesRepository privilegesRepository)
        {
            _groupRepository = groupRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
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

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ViewGroups);

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

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageGroups);

            var tabularData = new TabularData { Pagination = pagination, Sorting = sorting, Search = search };
            var result = await _groupRepository.GetGroupUsersAsync(groupId, tabularData, GroupsHelper.SortGroups);

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
            if (scope == null)
            {
                return BadRequest(ErrorMessages.InvalidDeleteGroupsParameters);
            }

            if (scope.IsEmpty())
            {
                return Ok(DeleteResult.Empty);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageGroups);

            var result = await _groupRepository.DeleteGroupsAsync(scope, search);

            return Ok(new DeleteResult { TotalDeleted = result });
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

            var groupId = await _groupRepository.AddGroupAsync(group);
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
                throw new ResourceNotFoundException(ErrorMessages.GroupDoesNotExist, ErrorCodes.ResourceNotFound);
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

            GroupValidator.ValidateModel(group, OperationMode.Edit);

            await _groupRepository.UpdateGroupAsync(groupId, group);

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
            var result = await _groupRepository.GetGroupMembersAsync(groupId, tabularData, UserGroupHelper.SortUsergroups);

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
            if (scope == null || scope.IsEmpty())
            {
                throw new BadRequestException(ErrorMessages.AssignMemberScopeEmpty, ErrorCodes.BadRequest);
            }
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageGroups);
            await _groupRepository.AssignMembers(groupId, scope, search);
            return Ok();
        }
    }
}