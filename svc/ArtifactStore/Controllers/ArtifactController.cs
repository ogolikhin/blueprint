using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System.Linq;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ArtifactController : LoggableApiController
    {
        internal readonly ISqlArtifactRepository ArtifactRepository;

        public override string LogSource { get; } = "ArtifactStore.Artifact";

        public ArtifactController() : this(new SqlArtifactRepository())
        {
        }

        public ArtifactController(ISqlArtifactRepository instanceRepository) : base()
        {
            ArtifactRepository = instanceRepository;
        }

        public ArtifactController(ISqlArtifactRepository instanceRepository, IServiceLogRepository log) : base(log)
        {
            ArtifactRepository = instanceRepository;
        }

        /// <summary>
        /// Get child artifacts of the project.
        /// </summary>
        /// <remarks>
        /// Returns child artifacts of the project with the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/children"), SessionRequired]
        [ActionName("GetProjectChildren")]
        public async Task<List<Artifact>> GetProjectChildrenAsync(int projectId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return await ArtifactRepository.GetProjectOrArtifactChildrenAsync(projectId, null, session.UserId);
        }

        /// <summary>
        /// Get child artifacts of the artifact.
        /// </summary>
        /// <remarks>
        /// Returns child artifacts of the artifact with the specified project and artifact ids.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact.</response>
        /// <response code="404">Not found. A project or an artifact for the specified ids is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/artifacts/{artifactId:int:min(1)}/children"), SessionRequired]
        [ActionName("GetArtifactChildren")]
        public async Task<List<Artifact>> GetArtifactChildrenAsync(int projectId, int artifactId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return await ArtifactRepository.GetProjectOrArtifactChildrenAsync(projectId, artifactId, session.UserId);
        }

        /// <summary>
        /// Get sub artifact tree of the artifact.
        /// </summary>
        /// <remarks>
        /// Returns tree of subartifacts
        /// </remarks>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/subartifacts"), SessionRequired]
        [ActionName("GetSubArtifactTreeAsync")]
        public async Task<List<SubArtifact>> GetSubArtifactTreeAsync(int artifactId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return (await ArtifactRepository.GetSubArtifactTreeAsync(artifactId, session.UserId)).ToList();
        }

        /// <summary>
        /// Get the artifact tree expended to the artifact.
        /// </summary>
        /// <remarks>
        /// Returns the tree of artifacts expended to the artifact with the specified project and artifact ids.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact.</response>
        /// <response code="404">Not found. A project or an artifact for the specified ids is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/artifacts/{expandedToArtifactId=expandedToArtifactId}/{includeChildren=includeChildren?}"), SessionRequired]
        [ActionName("GetExpandedTreeToArtifact")]
        public async Task<List<Artifact>> GetExpandedTreeToArtifactAsync(int projectId, int expandedToArtifactId, bool includeChildren = false)
        {
            if(expandedToArtifactId < 1)
                throw new BadRequestException(string.Format("Parameter {0} must be greater than 0.", nameof(expandedToArtifactId)), ErrorCodes.OutOfRangeParameter);

            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return await ArtifactRepository.GetExpandedTreeToArtifactAsync(projectId, expandedToArtifactId, includeChildren, session.UserId);
        }
    }
}
