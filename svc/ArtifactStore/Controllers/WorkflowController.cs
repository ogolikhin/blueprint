using ArtifactStore.Repositories;
using ArtifactStore.Services.VersionControl;
using ArtifactStore.Services.Workflow;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ArtifactStore.Executors;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Reuse;
using ServiceLibrary.Repositories.Workflow;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class WorkflowController : LoggableApiController
    {
        private readonly IWorkflowService _workflowService;

        public override string LogSource { get; } = "ArtifactStore.Workflow";

        public WorkflowController() : this(
            new WorkflowService(
                new SqlHelper(),
                new SqlItemInfoRepository(),
                new StateChangeExecutorRepositories(
                    new SqlArtifactVersionsRepository(),
                    new SqlWorkflowRepository(),
                    new VersionControlService(),
                    new ReuseRepository(),
                    new SqlSaveArtifactRepository(),
                    new ApplicationSettingsRepository(),
                    new ServiceLogRepository(),
                    new SqlUsersRepository())))
        {
        }

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        /// <summary>
        /// Gets the next available transitions which are available to the user for the workflow and state.
        /// User id is retrieved from the request header.
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
        [FeatureActivation(FeatureTypes.Workflow)]
        [Route("artifacts/{artifactId:int:min(1)}/transitions"), SessionRequired]
        [ActionName("GetTransitions")]
        [ResponseType(typeof(WorkflowTransitionResult))]
        public async Task<IHttpActionResult> GetTransitionsAsync(int artifactId, int workflowId = 0, int stateId = 0)
        {
            if (workflowId <= 0 || stateId <= 0)
            {
                throw new BadRequestException("Please provide valid workflow id and state id");
            }
            //We assume that Session always exist as we have SessionRequired attribute
            return Ok(await _workflowService.GetTransitionsAsync(Session.UserId, artifactId, workflowId, stateId));
        }

        /// <summary>
        /// Gets the current state for the artifact. 
        /// Permission for the artifact is based on user id which is retrieved from the request.
        /// </summary>
        /// <param name="artifactId"></param>
        /// <param name="versionId"></param>
        /// <param name="addDrafts"></param>
        /// <returns></returns>
        [HttpGet, NoCache]
        [FeatureActivation(FeatureTypes.Workflow)]
        [Route("artifacts/{artifactId:int:min(1)}/state"), SessionRequired]
        [ActionName("GetStateForArtifact")]
        [ResponseType(typeof(QuerySingleResult<WorkflowState>))]
        public async Task<IHttpActionResult> GetStateForArtifactAsync(int artifactId, int? versionId = null, bool addDrafts = true)
        {
            //We assume that Session always exist as we have SessionRequired attribute
            return Ok(await _workflowService.GetStateForArtifactAsync(Session.UserId, artifactId, versionId, addDrafts));
        }

        /// <summary>
        /// Modifies the current state to the new desired state.
        /// Permission for the artifact is based on user id which is retrieved from the request.
        /// </summary>
        /// <param name="artifactId"></param>
        /// <param name="stateChangeParameter"></param>
        /// <returns></returns>
        [HttpPost]
        [FeatureActivation(FeatureTypes.Workflow)]
        [Route("artifacts/{artifactId:int:min(1)}/state"), SessionRequired]
        [ActionName("ChangeStateForArtifact")]
        [ResponseType(typeof(QuerySingleResult<WorkflowState>))]
        public async Task<IHttpActionResult> ChangeStateForArtifactAsync([FromUri] int artifactId, [FromBody] WorkflowStateChangeParameter stateChangeParameter)
        {
            //We assume that Session always exist as we have SessionRequired attribute
            if (artifactId <= 0 || stateChangeParameter == null || !ModelState.IsValid || stateChangeParameter.FromStateId == stateChangeParameter.ToStateId)
            {
                throw new BadRequestException("Please provide valid state change parameters");
            }
            
            return Ok(await _workflowService.ChangeStateForArtifactAsync(Session.UserId, Session.UserName, artifactId, stateChangeParameter));
        }
    }
}