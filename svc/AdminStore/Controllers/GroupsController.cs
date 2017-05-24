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
        /// Get groups list according to the input parameters 
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
        public async Task<IHttpActionResult> GetGroups([FromUri]Pagination pagination, [FromUri]Sorting sorting, [FromUri] string search = null, int userId = 0)
        {
            PaginationValidator.ValidatePaginationModel(pagination);

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ViewGroups);
            var tabularData = new TabularData { Pagination = pagination, Sorting = sorting, Search = search };

            var result = await _groupRepository.GetGroupsAsync(userId, tabularData, GroupsHelper.SortGroups);
            return Ok(result);
        }

        /// <summary>
        /// Delete group/groups from the system
        /// </summary>
        /// <param name="body">list of group ids and selectAll flag</param>
        /// <param name="search">search filter</param>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if used doesn’t have permissions to delete groups</response>
        [HttpPost]
        [SessionRequired]
        [Route("delete")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> DeleteGroups([FromBody] OperationScope body, string search = null)
        {
            if (body == null)
            {
                return BadRequest(ErrorMessages.InvalidDeleteGroupsParameters);
            }
            //No scope for deletion is provided
            if (body.IsSelectionEmpty())
            {
                return Ok(new DeleteResult() { TotalDeleted = 0 });
            }
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageGroups);

            var result = await _groupRepository.DeleteGroupsAsync(body, search);
            return Ok(new DeleteResult() { TotalDeleted = result });
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

            GroupValidator.ValidateModel(group);

            var groupId = await _groupRepository.AddGroupAsync(group);
            return Request.CreateResponse(HttpStatusCode.Created, groupId);
        }
    }
}