using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.Workflow;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class WorkflowController : LoggableApiController
    {
        internal readonly ISqlWorkflowRepository WorkflowRepository;

        public override string LogSource { get; } = "ArtifactStore.Workflow";

        public WorkflowController() : this(new SqlWorkflowRepository())
        {
        }

        public WorkflowController(ISqlWorkflowRepository workflowRepository) 
        {
            WorkflowRepository = workflowRepository;
        }

        /// <summary>
        /// Get workflow transitions for current artifact
        /// </summary>
        /// <remarks>
        /// Returns list of all possible workflow transitions based of of the current artifact and the state its in.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>              
        /// <response code="403">Forbidden. The user does not have permissions for the artifact.</response>
        /// <response code="404">Not found. The artifact is not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/transitions"), SessionRequired]
        [ActionName("GetTransitions")]
        public async Task<IEnumerable<Transitions>> GetTransitions(int artifactId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return await WorkflowRepository.GetTransitions(artifactId, session.UserId);
        }
    }


}