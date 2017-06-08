using System;
using System.Threading.Tasks;
using ArtifactStore.Executors;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Services.Workflow
{
    public interface IWorkflowService
    {
        Task<WorkflowTransitionResult> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId);

        Task<QuerySingleResult<WorkflowState>> GetStateForArtifactAsync(int userId, int artifactId, int revisionId = int.MaxValue,
            bool addDrafts = true);

        Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactAsync(int userId, int artifactId,
            WorkflowStateChangeParameter stateChangeParameter);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;

        public WorkflowService(IWorkflowRepository workflowRepository,
            IArtifactVersionsRepository artifactVersionsRepository)
        {
            _workflowRepository = workflowRepository;
            _artifactVersionsRepository = artifactVersionsRepository;
        }

        public async Task<WorkflowTransitionResult> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId)
        {
            var transitions = await _workflowRepository.GetTransitionsAsync(userId, artifactId, workflowId, stateId);

            return new WorkflowTransitionResult
            {
                ResultCode = QueryResultCode.Success,
                Total = transitions.Count,
                Count = transitions.Count,
                Items = transitions
            };
        }

        public async Task<QuerySingleResult<WorkflowState>> GetStateForArtifactAsync(int userId, int artifactId, int revisionId = Int32.MaxValue, bool addDrafts = true)
        {
            var state = await _workflowRepository.GetStateForArtifactAsync(userId, artifactId, revisionId, addDrafts);
            if (state == null || state.WorkflowId <= 0 || state.Id <= 0)
            {
                return new QuerySingleResult<WorkflowState>
                {
                    ResultCode = QueryResultCode.Failure,
                    Message = I18NHelper.FormatInvariant("State information could not be retrieved for Artifact (Id:{0}).", artifactId)
                };
            }
            return new QuerySingleResult<WorkflowState>
            {
                ResultCode = QueryResultCode.Success,
                Item = state
            };
        }

        public async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactAsync(int userId, int artifactId, WorkflowStateChangeParameter stateChangeParameter)
        {
            //We will be getting state information and then will construct the property constraints and post operation actions over here
            var stateChangeExecutor = new StateChangeExecutor(null, 
                null, 
                new WorkflowStateChangeParameterEx(stateChangeParameter)
                {
                    ArtifactId = artifactId
                },
                userId, 
                _artifactVersionsRepository, 
                _workflowRepository);

            return await stateChangeExecutor.Execute();
        }

       
    }
}