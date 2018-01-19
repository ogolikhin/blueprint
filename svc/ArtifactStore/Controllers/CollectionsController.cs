using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ArtifactStore.Services.Collections;
using SearchEngineLibrary.Service;
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
    [RoutePrefix("collections")]
    public class CollectionsController : LoggableApiController
    {
        private readonly ISearchEngineService _searchServiceEngine;
        private readonly ICollectionsService _collectionsService;

        private readonly ICollectionsRepository _collectionsRepository;
        private readonly ICollectionsService _collectionsService;

        internal CollectionsController() : this(
            new CollectionsService(),
            new SearchEngineService())
        {
        }

        public CollectionsController(
            ICollectionsService collectionsService,
            ISearchEngineService searchServiceEngine)
        internal CollectionsController
        (
            ICollectionsRepository collectionsRepository,
            IPrivilegesRepository privilegesRepository,
            ICollectionsService collectionsService)
        {
            _collectionsService = collectionsService;
            _searchServiceEngine = searchServiceEngine;
        }

        /// <summary>
        /// Gets the artifacts in a specified collection
        /// </summary>
        /// <param name="id">Collection id.</param>
        /// <param name="pagination">Limit and offset values to query artifacts.</param>
        /// <response code="200">OK. List of artifacts in collection</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">User doesn’t have permission to access list of artifacts in collection.</response>
        /// <response code="404">Not Found. Collection was not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        ///
        [HttpGet, NoCache]
        [Route("{id:int:min(1)}/artifacts"), SessionRequired]
        [ResponseType(typeof(CollectionArtifacts))]
        public async Task<IHttpActionResult> GetArtifactsInCollectionAsync(int id, [FromUri] Pagination pagination)
        {
            pagination.Validate();

            var artifacts = await _collectionsService.GetArtifactsInCollectionAsync(id, pagination, Session.UserId);

            return Ok(artifacts);
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
