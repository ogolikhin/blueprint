using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Models;
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
        internal readonly ISqlGroupRepository _sqlGroupRepository;
        internal readonly PrivilegesManager _privilegesManager;
        public GroupsController() : this(new SqlGroupRepository(), new SqlPrivilegesRepository())
        {
        }

        internal GroupsController(ISqlGroupRepository sqlGroupRepository, IPrivilegesRepository privilegesRepository)
        {
            _sqlGroupRepository = sqlGroupRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        /// <summary>
        /// Get groups list according to the input parameters 
        /// </summary>
        /// <param name="userId">User's identity</param>
        /// <param name="pagination">Pagination parameters</param>
        /// <param name="sorting">Sorting parameters</param>
        /// <param name="search">The parameter for searching by group name</param>
        /// <response code="200">OK. The list of groups.</response>
        /// <response code="400">BadRequest. Some errors. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. if used doesn’t have permissions to get groups list.</response>
        [Route("")]
        [SessionRequired]
        [ResponseType(typeof(QueryResult<GroupDto>))]
        public async Task<IHttpActionResult> GetGroups(int userId, [FromUri]Pagination pagination, [FromUri]Sorting sorting, [FromUri] string search = null)
        {
            PaginationValidator.ValidatePaginationModel(pagination);

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ViewGroups);
            var tabularData = new TabularData { Pagination = pagination, Sorting = sorting, Search = search };

            var result = await _sqlGroupRepository.GetGroupsAsync(userId, tabularData, GroupsHelper.SortGroups);
            return Ok(result);
        }
    }
}