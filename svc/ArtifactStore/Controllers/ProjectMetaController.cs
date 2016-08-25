using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ProjectMetaController : LoggableApiController
    {
        internal readonly ISqlProjectMetaRepository ProjectMetaRepository;

        public override string LogSource { get; } = "ArtifactStore.ProjectMeta";

        public ProjectMetaController() : this(new SqlProjectMetaRepository())
        {
        }

        public ProjectMetaController(ISqlProjectMetaRepository projectMetaRepository) : base()
        {
            ProjectMetaRepository = projectMetaRepository;
        }

        public ProjectMetaController(ISqlProjectMetaRepository projectMetaRepository, IServiceLogRepository log) : base(log)
        {
            ProjectMetaRepository = projectMetaRepository;
        }

        /// <summary>
        /// Get artifact, sub-artifact and property types of the project.
        /// </summary>
        /// <remarks>
        /// Returns artifact, sub-artifact and property types of the project with the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/meta/customtypes"), SessionRequired]
        [ActionName("GetProjectTypes")]
        public async Task<ProjectTypes> GetProjectTypesAsync(int projectId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return await ProjectMetaRepository.GetCustomProjectTypesAsync(projectId, session.UserId);
        }
    }
}
