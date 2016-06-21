using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceLibrary.Attributes;
using ServiceLibrary.Filters;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System.Net.Http;
using System.Net;

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

        internal readonly ISqlArtifactVersionsRepository ArtifactVersionsRepository;
        internal readonly IArtifactPermissionsRepository ArtifactPermissionsRepository;
        public override string LogSource { get; } = "ArtifactStore.ArtifactVersions";

        public ArtifactVersionsController() : this(new SqlArtifactVersionsRepository(), new SqlArtifactPermissionsRepository())
        {
        }
        public ArtifactVersionsController(ISqlArtifactVersionsRepository artifactVersionsRepository, IArtifactPermissionsRepository artifactPermissionsRepository) : base()
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
                throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
            }
            if (limit > MAX_LIMIT)
            {
                limit = MAX_LIMIT;
            }

            var artifactIds = new List<int> { artifactId };
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(artifactIds, session.UserId);

            if (!permissions.ContainsKey(artifactId))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            else
            {
                RolePermissions permission = RolePermissions.None;
                permissions.TryGetValue(artifactId, out permission);

                if (!permission.HasFlag(RolePermissions.Read))
                {
                    throw new HttpResponseException(HttpStatusCode.Forbidden);
                }
            }
            var result = await ArtifactVersionsRepository.GetArtifactVersions(artifactId, limit, offset, userId, asc, session.UserId);
            return result;
        }
    }
}