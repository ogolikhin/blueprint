using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ArtifactStore.Services.Collections;
using SearchEngineLibrary.Service;
using System.Web.Http;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System.Web.Http.Description;
using ArtifactStore.Models;
using ArtifactStore.Services.Workflow;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("collections")]
    public class CollectionsController : LoggableApiController
    {
        private readonly ISearchEngineService _searchServiceEngine;
        private readonly ICollectionsService _collectionsService;

        private readonly ICollectionsService _collectionsService;
        internal CollectionsController() : this
            (
                new CollectionsService(new SqlCollectionsRepository(),
                                       new SqlArtifactRepository(),
                                       new SqlLockArtifactsRepository(),
                                       new SqlItemInfoRepository(),
                                       new SqlArtifactPermissionsRepository(),
                                       new SqlHelper()))
        {
        }

        public CollectionsController(ICollectionsService collectionsService)
        {
            _collectionsService = collectionsService;
        }

        /// <summary>
        /// Add artifacts to collection.
        /// </summary>
        /// <remarks>
        /// Adds artifacts to the collection with specified id.
        /// </remarks>
        /// <param name="id"> Id of the collection</param>
        /// <param name="add"> 'add' flag </param>
        /// <param name="ids"> ids of artifacts to be added</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the collection.</response>
        /// <response code="404">Not found. A collection for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("{id:int:min(1)}/artifacts"), SessionRequired]
        [ResponseType(typeof(AssignArtifactsResult))]
        public async Task<IHttpActionResult> AddArtifactsToCollectionAsync(int id, string add, [FromBody] ISet<int> ids)
        {
            if (ids == null)
            {
                throw new BadRequestException(ErrorMessages.InvalidAddArtifactsParameters, ErrorCodes.BadRequest);
            }

            var result = await _collectionsService.AddArtifactsToCollectionAsync(Session.UserId, id, ids);
            return Ok(result);
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
    }
}
