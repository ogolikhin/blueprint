using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ArtifactStore.ArtifactList;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Models;
using ArtifactStore.Models.Review;
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
        private readonly IArtifactListService _artifactListService;
        private readonly ICollectionsService _collectionsService;
        private const int _defaultPaginationLimit = 10;
        private const int _defaultPaginationOffset = 0;

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

            pagination = pagination ?? new Pagination();
            pagination.Offset = pagination.Offset ?? _defaultPaginationOffset;
            pagination.Limit = pagination.Limit
                ?? await _artifactListService.GetPaginationLimitAsync(id, userId)
                ?? _defaultPaginationLimit;

            var artifacts = await _collectionsService.GetArtifactsInCollectionAsync(id, pagination, userId);
            artifacts.Pagination = pagination;

            await _artifactListService.SavePaginationLimitAsync(id, pagination.Limit, userId);

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
        /// <returns>Amount of added artifacts, total amount of passed artifact to add.</returns>
        [HttpPost]
        [Route("{id:int:min(1)}/artifacts"), SessionRequired]
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
        [Route("{id:int:min(1)}/artifacts"), SessionRequired]
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
            try
            {
                await _collectionsService.SaveProfileColumnsAsync(id, profileColumns, Session.UserId);
            }
            catch (InvalidColumnsException e)
            {
                throw new BadRequestException(GetInvalidColumnsErrorMessage((IEnumerable<ProfileColumn>)e.Content), e.ErrorCode);
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        private static string GetInvalidColumnsErrorMessage(IEnumerable<ProfileColumn> profileColumns)
        {
            if (profileColumns == null) return null;
            switch (profileColumns.Count())
            {
                case 1:
                    return I18NHelper.FormatInvariant(ErrorMessages.Collections.SingleInvalidColumn,
                        profileColumns.Take(1).SingleOrDefault()?.PropertyName);
                case 2:
                case 3:
                    return I18NHelper.FormatInvariant(ErrorMessages.Collections.SomeInvalidColumns,
                        string.Join(", ", profileColumns.Select(q => q.PropertyName)));
                default:
                    return I18NHelper.FormatInvariant(ErrorMessages.Collections.MultipleInvalidColumns,
                        string.Join(", ", profileColumns.Take(3).Select(q => q.PropertyName)));
            }
        }
    }
}
