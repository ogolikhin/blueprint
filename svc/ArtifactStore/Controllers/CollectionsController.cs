using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using SearchEngineLibrary.Service;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
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

        private readonly PrivilegesManager _privilegesManager;

        internal CollectionsController() : this
            (
                new SearchEngineService())
        {
        }

        internal CollectionsController(ISearchEngineService searchServiceEngine) : this
            (
                new CollectionsRepository(),
                new SqlPrivilegesRepository(), searchServiceEngine)
        {
        }

        internal CollectionsController
        (
            ICollectionsRepository collectionsRepository,
            IPrivilegesRepository privilegesRepository,
            ISearchEngineService searchServiceEngine)
        {
            _collectionsRepository = collectionsRepository;
            _searchServiceEngine = searchServiceEngine;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        public CollectionsController
        (
            ICollectionsRepository collectionsRepository,
            IPrivilegesRepository privilegesRepository,
            IServiceLogRepository log,
            ISearchEngineService searchServiceEngine) : base(log)
        {
            _collectionsRepository = collectionsRepository;
            _searchServiceEngine = searchServiceEngine;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        /// <summary>
        /// Get list of artifacts of a collection
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
        [ResponseType(typeof(ArtifactsOfCollectionDto))]
        public async Task<IHttpActionResult> GetArtifactsOfCollectionAsync(int id, [FromUri] Pagination pagination)
        {
            pagination.Validate();

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            var searchArtifactsResult = await _searchServiceEngine.Search(scopeId: id, pagination: pagination, scopeType: ScopeType.Contents, includeDrafts: true, userId: Session.UserId);

            var artifactsOfCollectionDto = await _collectionsRepository.GetArtifactsOfCollectionAsync(Session.UserId, searchArtifactsResult.ArtifactIds);

            artifactsOfCollectionDto.ItemsCount = searchArtifactsResult.Total;

            return Ok(artifactsOfCollectionDto);
        }
    }
}