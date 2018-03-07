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
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models;

namespace ArtifactStore.Collections
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("collections")]
    public class CollectionsController : LoggableApiController
    {
        private const int DefaultPaginationOffset = 0;
        private const int DefaultPaginationLimit = 10;

        private readonly IArtifactListService _artifactListService;
        private readonly ICollectionsService _collectionsService;

        public override string LogSource => "ArtifactStore.Collections";

        public CollectionsController() : this(
            new CollectionsService(),
            new ArtifactListService())
        {
        }

        public CollectionsController(
            ICollectionsService collectionsService,
            IArtifactListService artifactListService)
        {
            _collectionsService = collectionsService;
            _artifactListService = artifactListService;
        }

        /// <summary>
        /// Gets the artifacts in a specified collection
        /// </summary>
        /// <param name="id">Collection id.</param>
        /// <param name="pagination">Limit and offset values to query artifacts.</param>
        /// <response code="200">OK. List of artifacts in collection</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
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
            pagination.Validate(true);

            var userId = Session.UserId;

            var profileSettings = await _artifactListService.GetProfileSettingsAsync(id, userId);

            pagination = pagination ?? new Pagination();
            pagination.Offset = pagination.Offset ?? DefaultPaginationOffset;
            pagination.Limit = pagination.Limit
                ?? profileSettings?.PaginationLimit
                ?? DefaultPaginationLimit;

            var collectionData = await _collectionsService.GetArtifactsInCollectionAsync(id, userId, pagination, profileSettings?.ProfileColumns);
            collectionData.CollectionArtifacts.Pagination = pagination;

            switch (collectionData.CollectionArtifacts.ColumnValidation.Status)
            {
                case ColumnValidationStatus.AllInvalid:
                    await _artifactListService.SaveProfileSettingsAsync(id, userId, new ProfileColumns(new List<ProfileColumn>()), collectionData.CollectionArtifacts.Pagination.Limit);
                    break;
                case ColumnValidationStatus.SomeValid:
                    await _artifactListService.SaveProfileSettingsAsync(id, userId, collectionData.ProfileColumns, collectionData.CollectionArtifacts.Pagination.Limit);
                    break;
                default:
                    await _artifactListService.SavePaginationLimitAsync(id, pagination.Limit, userId);
                    break;
            }

            return Ok(collectionData.CollectionArtifacts);
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
        /// <returns>Amount of added artifacts, total amount of passed artifact to add.</returns>
        [HttpPost]
        [Route("{id:int:min(1)}/artifacts/{addaction:length(0)=}"), SessionRequired]
        [ResponseType(typeof(AddArtifactsToCollectionResult))]
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
        /// Remove artifacts from collection.
        /// </summary>
        /// <remarks>
        /// Removes artifacts from the collection with specified id.
        /// </remarks>
        /// <param name="id">Collection id.</param>
        /// <param name="remove">Operation identifier.</param>
        /// <param name="removalParams">Removal parameters for artifacts to be removed.</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the collection.</response>
        /// <response code="404">Not found. A collection for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <returns>Amount of removed artifacts, total amount of passed artifact to remove.</returns>
        [HttpPost]
        [Route("{id:int:min(1)}/artifacts/{removeaction:length(0)=}"), SessionRequired]
        [ResponseType(typeof(RemoveArtifactsFromCollectionResult))]
        public async Task<IHttpActionResult> RemoveArtifactsFromCollectionAsync(
            int id, string remove, ItemsRemovalParams removalParams)
        {
            removalParams.Validate();

            var result = await _collectionsService.RemoveArtifactsFromCollectionAsync(id, removalParams, Session.UserId);

            return Ok(result);
        }

        /// <summary>
        /// Gets available collection columns.
        /// </summary>
        /// <param name="id">Collection id.</param>
        /// <param name="search">Parameter to match column names.</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the collection.</response>
        /// <response code="404">Not found. A collection for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <returns>Selected and remaning columns.</returns>
        [HttpGet, NoCache]
        [Route("{id:int:min(1)}/settings/columns"), SessionRequired]
        [ResponseType(typeof(GetColumnsDto))]
        public async Task<IHttpActionResult> GetColumnsAsync(int id, string search = null)
        {
            SearchFieldValidator.Validate(search);

            var result = await _collectionsService.GetColumnsAsync(id, Session.UserId, search);

            return Ok(result);
        }

        /// <summary>
        /// Save collection columns settings.
        /// </summary>
        /// <param name="id">Collection id.</param>
        /// <param name="profileColumnsDto">Profile columns to save.</param>
        /// <response code="200">OK. Artifact list columns settings were saved. Returned warning about changing custom properties</response>
        /// <response code="204">NoContent. Artifact list columns settings were saved.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions to save artifact list columns settings.</response>
        /// <response code="404">Not Found. The collection with current id was not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("{id:int:min(1)}/settings/columns"), SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<HttpResponseMessage> SaveColumnsSettingsAsync(
            int id, [FromBody] ProfileColumnsDto profileColumnsDto)
        {
            if (profileColumnsDto == null || profileColumnsDto.Items.IsEmpty())
            {
                throw new BadRequestException(
                    ErrorMessages.Collections.ColumnsSettingsModelIsIncorrect, ErrorCodes.BadRequest);
            }

            var profileColumns = new ProfileColumns(profileColumnsDto.Items);
            var customPropertiesChanged = await _collectionsService.SaveProfileColumnsAsync(id, profileColumns, Session.UserId);

            return customPropertiesChanged
                ? Request.CreateResponse(HttpStatusCode.OK, ErrorMessages.ArtifactList.ColumnsSettings.ChangedCustomProperties)
                : Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
