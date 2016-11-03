using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ArtifactVersionsController : LoggableApiController
    {
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_OFFSET = 0;
        private const int MIN_LIMIT = 1;
        private const int MAX_LIMIT = 100;

        internal readonly IArtifactVersionsRepository ArtifactVersionsRepository;
        internal readonly IArtifactPermissionsRepository ArtifactPermissionsRepository;
        public override string LogSource { get; } = "ArtifactStore.ArtifactVersions";

        public ArtifactVersionsController() : this(new SqlArtifactVersionsRepository(), new SqlArtifactPermissionsRepository())
        {
        }
        public ArtifactVersionsController(IArtifactVersionsRepository artifactVersionsRepository, IArtifactPermissionsRepository artifactPermissionsRepository) : base()
        {
            ArtifactVersionsRepository = artifactVersionsRepository;
            ArtifactPermissionsRepository = artifactPermissionsRepository;
        }

        /// <summary>
        /// Get artifact history
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token or parameters are missing or malformed</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/version"), SessionRequired]
        [ActionName("GetArtifactHistory")]
        public async Task<ArtifactHistoryResultSet> GetArtifactHistory(int artifactId, int limit = DEFAULT_LIMIT, int offset = DEFAULT_OFFSET, int? userId = null, bool asc = false)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (limit < MIN_LIMIT || offset < 0 || userId < 1)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            if (limit > MAX_LIMIT)
            {
                limit = MAX_LIMIT;
            }

            var revisionId = int.MaxValue;
            var isDeleted = await ArtifactVersionsRepository.IsItemDeleted(artifactId);
            if (isDeleted)
            {
                var deletedInfo = await ArtifactVersionsRepository.GetDeletedItemInfo(artifactId);
                revisionId = deletedInfo.VersionId;
            }

            var artifactIds = new [] { artifactId };
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(artifactIds, session.UserId, false, revisionId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(artifactId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            var result = await ArtifactVersionsRepository.GetArtifactVersions(artifactId, limit, offset, userId, asc, session.UserId);
            return result;
        }

        /// <summary>
        /// Get version control information of an artifact or sub-artifact.
        /// </summary>
        /// <remarks>
        /// Returns version control information of the specified artifact or sub-artifact.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact or sub-artifact.</response>
        /// <response code="404">Not found. An artifact or sub-artifact for the specified id does not exist.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/versionControlInfo/{itemId:int:min(1)}"), SessionRequired]
        [ActionName("GetVersionControlArtifactInfo")]
        public async Task<VersionControlArtifactInfo> GetVersionControlArtifactInfoAsync(int itemId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return await ArtifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, session.UserId);
        }
    }
}
