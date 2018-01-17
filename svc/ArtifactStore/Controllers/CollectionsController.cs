using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using ArtifactStore.Helpers;
using SearchEngineLibrary.Service;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("collections")]
    public class CollectionsController : LoggableApiController
    {
        public override string LogSource { get; } = "ArtifactStore.Collections";

        private readonly ICollectionsRepository _collectionsRepository;
        private readonly ISearchEngineService _searchServiceEngine;
        private readonly IArtifactPermissionsRepository _permissionsRepository;

        internal CollectionsController() : this
            (
                new SqlArtifactPermissionsRepository(),
                new SqlCollectionsRepository(),
                new SearchEngineService())
        {
        }

        internal CollectionsController
        (IArtifactPermissionsRepository permissionsRepository, ICollectionsRepository collectionsRepository, ISearchEngineService searchServiceEngine)
        {
            _permissionsRepository = permissionsRepository;
            _collectionsRepository = collectionsRepository;
            _searchServiceEngine = searchServiceEngine;
        }

        public CollectionsController
        (
            IArtifactPermissionsRepository permissionsRepository,
            ICollectionsRepository collectionsRepository,
            IServiceLogRepository log,
            ISearchEngineService searchServiceEngine) : base(log)
        {
            _permissionsRepository = permissionsRepository;
            _collectionsRepository = collectionsRepository;
            _searchServiceEngine = searchServiceEngine;
        }

        /// <summary>
        /// Get list artifacts of a collection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pagination">Limit and offset values to query artifacts</param>
        /// <response code="200">OK. List of artifacts in collection</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">User doesn’t have permission to access list of artifacts in collection.</response>
        /// <response code="404">Not Found. Collection was not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        ///
        [HttpGet, NoCache]
        [Route("{id:int:min(1)}/artifacts"), SessionRequired]
        [ResponseType(typeof(ArtifactsOfCollection))]
        public async Task<IHttpActionResult> GetArtifactsOfCollectionAsync(int id, [FromUri] Pagination pagination)
        {
            if (!await _permissionsRepository.HasReadPermissions(id, Session.UserId))
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.NoAcessForCollection, id);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }

            pagination.Validate();

            var searchArtifactsResult = await _searchServiceEngine.Search(id, pagination, ScopeType.Contents, true, Session.UserId);

            var artifactsOfCollection = await _collectionsRepository.GetArtifactsWithPropertyValues(Session.UserId, searchArtifactsResult.ArtifactIds);
            artifactsOfCollection.ItemsCount = searchArtifactsResult.Total;

            return Ok(artifactsOfCollection);
        }
    }
}