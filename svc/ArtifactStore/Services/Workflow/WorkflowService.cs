﻿using System.Threading.Tasks;
using ArtifactStore.Executors;
using ArtifactStore.Helpers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services.Workflow
{
    public interface IWorkflowService
    {
        Task<WorkflowTransitionResult> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId);

        Task<QuerySingleResult<WorkflowState>> GetStateForArtifactAsync(int userId, int artifactId, int? versionId = null,
            bool addDrafts = true);

        Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactAsync(int userId, string userName, int artifactId,
            WorkflowStateChangeParameter stateChangeParameter);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IItemInfoRepository _itemInfoRepository;
        private readonly ISqlHelper _sqlHelper;
        private readonly IStateChangeExecutorRepositories _stateChangeExecutorRepositories;
        private readonly IWorkflowEventsMessagesHelper _workflowEventsMessagesHelper;

        public WorkflowService(ISqlHelper sqlHelper,
            IItemInfoRepository itemInfoRepository,
            IStateChangeExecutorRepositories stateChangeExecutorRepositories,
            IWorkflowEventsMessagesHelper workflowEventsMessagesHelper)
        {
            _sqlHelper = sqlHelper;
            _itemInfoRepository = itemInfoRepository;
            _stateChangeExecutorRepositories = stateChangeExecutorRepositories;
            _workflowEventsMessagesHelper = workflowEventsMessagesHelper;

        }

        public async Task<WorkflowTransitionResult> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId)
        {
            var transitions = await _stateChangeExecutorRepositories.WorkflowRepository.GetTransitionsAsync(userId, artifactId, workflowId, stateId);

            return new WorkflowTransitionResult
            {
                ResultCode = QueryResultCode.Success,
                Total = transitions.Count,
                Count = transitions.Count,
                Items = transitions
            };
        }

        public async Task<QuerySingleResult<WorkflowState>> GetStateForArtifactAsync(int userId, int artifactId, int? versionId = null, bool addDrafts = true)
        {
            var revisionId = await _itemInfoRepository.GetRevisionId(artifactId, userId, versionId);
            var state = await _stateChangeExecutorRepositories.WorkflowRepository.GetStateForArtifactAsync(userId, artifactId, revisionId, addDrafts);
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

        public async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactAsync(int userId, string userName, int artifactId, WorkflowStateChangeParameter stateChangeParameter)
        {
            // We will be getting state information and then will construct the property constraints and post operation actions over here
            var stateChangeExecutor = new StateChangeExecutor(userId,
                new WorkflowStateChangeParameterEx(stateChangeParameter)
                {
                    ArtifactId = artifactId,
                    UserName = userName
                },
                _sqlHelper,
                _stateChangeExecutorRepositories,
                new StateChangeExecutorHelper(_stateChangeExecutorRepositories),
                _workflowEventsMessagesHelper);

            return await stateChangeExecutor.Execute();
        }


    }
}