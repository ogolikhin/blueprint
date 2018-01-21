using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ArtifactStore.ArtifactList;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Models;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ArtifactStore.Collections
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("collections")]
    public class CollectionsController : LoggableApiController
    {
        private readonly ICollectionsService _collectionsService;
        private readonly IArtifactListSettingsService _artifactListSettingsService;

        public override string LogSource => "ArtifactStore.Collections";

        public CollectionsController() : this(
            new CollectionsService(),
            new ArtifactListSettingsService())
        {
        }

        public CollectionsController(
            ICollectionsService collectionsService,
            IArtifactListSettingsService artifactListSettingsService)
        {
            _collectionsService = collectionsService;
            _artifactListSettingsService = artifactListSettingsService;
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
        /// Add artifacts to collection.
        /// </summary>
        /// <remarks>
        /// Adds artifacts to the collection with specified id.
        /// </remarks>
        /// <param name="id">Collection id.</param>
        /// <param name="add">Operation identifier.</param>
        /// <param name="artifactIds">Ids of artifacts to be added.</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the collection.</response>
        /// <response code="404">Not found. A collection for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("{id:int:min(1)}/artifacts"), SessionRequired]
        [ResponseType(typeof(AddArtifactsResult))]
        public async Task<IHttpActionResult> AddArtifactsToCollectionAsync(
            int id, string add, [FromBody] ISet<int> artifactIds)
        {
            if (artifactIds.IsEmpty())
            {
                throw new BadRequestException(
                    ErrorMessages.Collections.AddArtifactsInvalidParameters, ErrorCodes.BadRequest);
            }

            var result = await _collectionsService.AddArtifactsToCollectionAsync(id, artifactIds, Session.UserId);

            return Ok(result);
        }

        /// <summary>
        /// Save artifact list columns settings.
        /// </summary>
        /// <param name="id">Collection id.</param>
        /// <param name="profileColumnSettings">Columns settings to save.</param>
        /// <response code="204">NoContent. Artifact list columns settings were saved.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions to save artifact list columns settings.</response>
        /// <response code="404">Not Found. The collection with current id was not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <returns></returns>
        [HttpPost]
        [Route("{id:int:min(1)}/artifacts/settings/columns"), SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<HttpResponseMessage> SaveArtifactListColumnsSettingsAsync(
            int id, [FromBody] ProfileColumnsSettings profileColumnSettings)
        {
            await _artifactListSettingsService.SaveArtifactListColumnsSettingsAsync(
                id, profileColumnSettings, Session.UserId);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
