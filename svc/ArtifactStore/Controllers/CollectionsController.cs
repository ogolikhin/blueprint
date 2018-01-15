using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System.Web.Http.Description;
using ArtifactStore.Models;
using ArtifactStore.Services.Workflow;
using ServiceLibrary.Models;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("collections")]
    [BaseExceptionFilter]
    public class CollectionsController : LoggableApiController
    {
        public override string LogSource { get; } = "ArtifactStore.Collections";

        private readonly ICollectionsService _collectionsService;

        private readonly PrivilegesManager _privilegesManager;

        internal CollectionsController() : this
            (
                new CollectionsService(new SqlCollectionsRepository(),
                                       new SqlArtifactRepository(),
                                       new SqlLockArtifactsRepository(),
                                       new SqlItemInfoRepository(),
                                       new SqlArtifactPermissionsRepository()),
                new SqlPrivilegesRepository())
        {
        }

        public CollectionsController(ICollectionsService collectionsService, IPrivilegesRepository privilegesRepository)
        {
            _collectionsService = collectionsService;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        /// <summary>
        /// Add artifacts to collection.
        /// </summary>
        /// <remarks>
        /// Adds artifacts to the collection with specified id.
        /// </remarks>
        /// <param name="id"> Id of the collection</param>
        /// <param name="add"> 'add' flag </param>
        /// <param name="scope">scope of artifacts to be added</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the collection.</response>
        /// <response code="404">Not found. A collection for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("{id:int:min(1)}/artifacts"), SessionRequired]
        [ResponseType(typeof(AssignArtifactsResult))]
        public async Task<IHttpActionResult> AddArtifactsToCollectionAsync(int id, string add, [FromBody] OperationScope scope)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            if (scope == null)
            {
                return BadRequest(ErrorMessages.InvalidAddArtifactsParameters);
            }

            var result = await _collectionsService.AddArtifactsToCollectionAsync(Session.UserId, id, scope);
            return Ok(result);
        }
    }
}