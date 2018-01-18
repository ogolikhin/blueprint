using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using AdminStore.Models;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Services;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class CollectionsController : LoggableApiController
    {
        public override string LogSource { get; } = "ArtifactStore.Collections";

        private readonly ICollectionsRepository _collectionsRepository;
        private readonly ICollectionsService _collectionsService;

        private readonly PrivilegesManager _privilegesManager;

        internal CollectionsController() : this
            (
                new SqlCollectionsRepository(),
                new SqlPrivilegesRepository(),
                new SqlCollectionsService())
        {
        }

        internal CollectionsController
        (
            ICollectionsRepository collectionsRepository,
            IPrivilegesRepository privilegesRepository,
            ICollectionsService collectionsService)
        {
            _collectionsRepository = collectionsRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
            _collectionsService = collectionsService;
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
        /// Save Artifact Columns Settings
        /// </summary>
        /// <param name="collectionId"></param>
        /// <response code="200">OK. Artifact Columns Settings were saved.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions to save Artifact Columns Settings</response>
        /// <response code="404">Not Found. The artifact with current id were not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <returns></returns>
        [HttpPost]
        [FeatureActivation(FeatureTypes.Storyteller)]
        [Route("artifactstore/collections/{id:int:min(1)}/artifacts/settings/columns"), SessionRequired]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> SaveArtifactColumnsSettings(int collectionId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            var result = await _collectionsService.SaveArtifactColumnsSettings(collectionId, Session.UserId, "123");

            return Ok(result);
        }
    }
}