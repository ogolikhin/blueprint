using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.VersionControl;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class VersionControlController : LoggableApiController
    {
        private readonly IVersionControlService _versionControlService;

        public override string LogSource { get; } = "ArtifactStore.VersionControl";

        public VersionControlController() : this(new VersionControlService())
        {
        }

        public VersionControlController(IVersionControlService versionControlService)
        {
            _versionControlService = versionControlService;
        }

        /// <summary>
        /// Publish artifacts.
        /// </summary>
        /// <remarks>
        /// Publish all or specified artifacts.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid or missing.</response>
        /// <response code="400">Bad Request. The input list of artifacts Ids is missing or incorrect when query parameter 'all' is not specified or false.</response>
        /// <response code="409">Conflict. There are dependent artifacts that are needed to be published.
        /// Or some properties at least of one artifact are invalid
        /// Or at least one of the specified artifacts is already published.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("artifacts/publish"), SessionRequired]
        [ActionName("PublishArtifacts")]
        [ResponseType(typeof(ArtifactResultSet))]
        public async Task<IHttpActionResult> PublishArtifacts([FromBody] IEnumerable<int> artifactIds, [FromUri] bool? all = null)
        {
            return Ok(await _versionControlService.PublishArtifacts(new PublishParameters
            {
                All = all,
                ArtifactIds = artifactIds,
                UserId = Session.UserId
            }));
        }
    }
}