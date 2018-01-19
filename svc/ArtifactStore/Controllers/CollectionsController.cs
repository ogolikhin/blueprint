using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.Serialization;
using ArtifactStore.Services.Collections;
using Newtonsoft.Json;
using SearchEngineLibrary.Service;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;
using ServiceLibrary.Services.ArtifactListSetting;
using ICollectionsService = ArtifactStore.Services.Collections.ICollectionsService;
using System.IO;
using System.Net;
using System.Net.Http;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("collections")]
    public class CollectionsController : LoggableApiController
    {
        private readonly ISearchEngineService _searchServiceEngine;
        private readonly ICollectionsService _collectionsService;
        private readonly IArtifactListSettingsService _artifactListSettingsService;

        public override string LogSource => "ArtifactStore.Collections";

        internal CollectionsController() : this(
            new CollectionsService(),
            new ArtifactListSettingsService(),
            new SearchEngineService())
        {
        }

        public CollectionsController(
            ICollectionsService collectionsService,
            IArtifactListSettingsService artifactListSettingsService,
            ISearchEngineService searchServiceEngine)
        {
            _collectionsService = collectionsService;
            _artifactListSettingsService = artifactListSettingsService;
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
        /// <param name="id">Artifact id.</param>
        /// <param name="artifactListSettings">ArtifactListSettings model.</param>
        /// <response code="204">NoContent. Artifact Columns Settings were saved.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions to save artifact columns Settings</response>
        /// <response code="404">Not Found. The artifact with current id were not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <returns></returns>
        [HttpPost]
        [Route("{id:int:min(1)}/artifacts/settings/columns"), SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<HttpResponseMessage> SaveArtifactColumnsSettings(int id, [FromBody] ArtifactListSettings artifactListSettings)
        {
            await _artifactListSettingsService.SaveArtifactColumnsSettings(id, Session.UserId, artifactListSettings);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
