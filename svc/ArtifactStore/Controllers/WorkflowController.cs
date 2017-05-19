using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Repositories.Workflow;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class WorkflowController : LoggableApiController, IWorkflowController
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
        public async Task<WorkflowTransitionResult> GetTransitions(int artifactId)
        {
            return await WorkflowRepository.GetTransitions(artifactId, CurrentSession.UserId);
        }

        public async Task<WorkflowState> GetCurrentState(int itemId, int? revisionId = null, bool addDrafts = true)
        {
            int userId = CurrentSession.UserId;
            return await WorkflowRepository.GetCurrentState(userId, itemId, revisionId ?? int.MaxValue, addDrafts);
        }

        public async Task<WorkflowTransitionResult> GetTransitions(int workflowId, int stateId)
        {
            int userId = CurrentSession.UserId;
            return await WorkflowRepository.GetAvailableTransitions(userId, workflowId, stateId);
        }
    }

    public interface IWorkflowController
    {
        /// <summary>
        /// Gets the current state for the artifact. 
        /// Permission for the artifact is based on user id which is retrieved from the request.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="revisionId"></param>
        /// <param name="addDrafts"></param>
        /// <returns></returns>
        Task<WorkflowState> GetCurrentState(int itemId, int? revisionId = null, bool addDrafts = true);

        /// <summary>
        /// Gets the next available transitions which are available to the user for the workflow and state.
        /// User id is retrieved from the request header.
        /// </summary>
        /// <param name="workflowId">Workflow Id for which we need transitions</param>
        /// <param name="stateId">State Id for which next transitions are required</param>
        /// <returns></returns>
        Task<WorkflowTransitionResult> GetTransitions(int workflowId, int stateId);
    }
}