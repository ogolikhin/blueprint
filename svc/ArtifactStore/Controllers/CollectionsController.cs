using System;
using System.Collections.Generic;
using System.Linq;
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
using ServiceLibrary.Models;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("collections")]
    [BaseExceptionFilter]
    public class CollectionsController : LoggableApiController
    {
        public override string LogSource { get; } = "ArtifactStore.Collections";

        private readonly ICollectionsRepository _collectionsRepository;

        private readonly PrivilegesManager _privilegesManager;

        internal CollectionsController() : this
            (
                new CollectionsRepository(),
                new SqlPrivilegesRepository())
        {
        }

        internal CollectionsController
        (
            ICollectionsRepository collectionsRepository,
            IPrivilegesRepository privilegesRepository)
        {
            _collectionsRepository = collectionsRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        public CollectionsController
        (
            ICollectionsRepository collectionsRepository,
            IPrivilegesRepository privilegesRepository,
            IServiceLogRepository log) : base(log)
        {
            _collectionsRepository = collectionsRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        /// <summary>
        /// Add artifacts to collection.
        /// </summary>
        /// <remarks>
        /// Adds artifacts to the collection with specified id.
        /// </remarks>
        /// <param name="id"> Id of the collection</param>
        /// <param name="scope">scope of artifacts to be added</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the collection.</response>
        /// <response code="404">Not found. A collection for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("{id:int:min(1)}/artifacts?add"), SessionRequired]
        [ResponseType(typeof(AssignArtifactsResult))]
        public async Task<IHttpActionResult> AddArtifactsToCollectionAsync(int id, [FromBody] OperationScope scope)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            if (scope == null)
            {
                return BadRequest(ErrorMessages.InvalidAddArtifactsParameters);
            }

            var result = await _collectionsRepository.AddArtifactsToCollectionAsync(Session.UserId, id, scope);
            return Ok(result);
        }
    }
}