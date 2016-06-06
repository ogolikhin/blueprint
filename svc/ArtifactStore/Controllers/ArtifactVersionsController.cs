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

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ArtifactVersionsController : LoggableApiController
    {
        internal readonly ISqlArtifactVersionsRepository ArtifactVersionsRepository;
        public override string LogSource { get; } = "ArtifactStore.ArtifactVersions";

        public ArtifactVersionsController() : this(new SqlArtifactVersionsRepository())
        {
        }
        public ArtifactVersionsController(ISqlArtifactVersionsRepository artifactVersionsRepository) : base()
        {
            ArtifactVersionsRepository = artifactVersionsRepository;
        }

        /// <summary>
        /// Get artifact history
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/version"), NoSessionRequired]
        [ActionName("GetArtifactHistory")]
        public async Task<ArtifactHistoryResultSet> GetArtifactHistory(int artifactId, int limit = 10, int offset = 0, int? userId = null, bool asc = false)
        {
            //var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (limit < 1 || limit > 100 || offset < 0 || userId < 1)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
            }
            var result = await ArtifactVersionsRepository.GetArtifactVersions(artifactId, limit, offset, userId, asc);
            return result;
        }
    }
}