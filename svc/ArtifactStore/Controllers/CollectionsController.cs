using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ArtifactStore.Services.Collections;
using SearchEngineLibrary.Service;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("collections")]
    public class CollectionsController : LoggableApiController
    {
        public override string LogSource { get; } = "ArtifactStore.Collections";

        private readonly ISearchEngineService _searchServiceEngine;
        private readonly ICollectionsService _collectionsService;

        internal CollectionsController() : this
            (
                new CollectionsService(new SqlCollectionsRepository(),
                    new SqlArtifactPermissionsRepository()),
                new SearchEngineService())
        {
        }

        public CollectionsController(ICollectionsService collectionsService, ISearchEngineService searchServiceEngine)
        {
            _collectionsService = collectionsService;
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
        [ResponseType(typeof(CollectionArtifacts))]
        public async Task<IHttpActionResult> GetArtifactsOfCollectionAsync(int id, [FromUri] Pagination pagination)
        {
            pagination.Validate();

            var searchArtifactsResult = await _searchServiceEngine.Search(id, pagination, ScopeType.Contents, true, Session.UserId);

            var artifactsOfCollection = await _collectionsService.GetArtifactsWithPropertyValues(Session.UserId, id, searchArtifactsResult.ArtifactIds);
            artifactsOfCollection.ItemsCount = searchArtifactsResult.Total;

            return Ok(artifactsOfCollection);
        }
    }
}