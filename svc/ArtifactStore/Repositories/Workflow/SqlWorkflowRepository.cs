using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories.Workflow
{
    public class SqlWorkflowRepository : SqlBaseArtifactRepository, IWorkflowRepository
    {
        

        public SqlWorkflowRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlWorkflowRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlWorkflowRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository) 
            : base(connectionWrapper,artifactPermissionsRepository)
        {
        }

        #region artifact workflow

        public async Task<WorkflowTransitionResult> GetTransitions(int userId, int artifactId, int workflowId, int stateId)
        {
            //Do not return transitions if the 
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Edit);
            
            return await GetAvailableTransitions(userId, workflowId, stateId);
        }

        public async Task<QuerySingleResult<WorkflowState>> GetState(int userId, int artifactId, int revisionId, bool addDrafts)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, artifactId, revisionId);
            
            return await GetCurrentStateInternal(userId, artifactId, revisionId, addDrafts);
        }

        public async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifact(int userId, WorkflowStateChangeParameter stateChangeParameter)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, stateChangeParameter.ArtifactId);

            return await ChangeStateForArtifactInternal(userId, stateChangeParameter.ArtifactId, stateChangeParameter.ToStateId);
        }

        #endregion

        #region Private methods

        private async Task<WorkflowTransitionResult> GetAvailableTransitions(int userId, int workflowId, int stateId)
        {
            return await GetAvailableTransitionsInternal(userId, workflowId, stateId);
        }

        

        private async Task<QuerySingleResult<WorkflowState>> GetCurrentStateInternal(int userId, int artifactId, int revisionId, bool addDrafts)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@artifactId", artifactId);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);
            
            var result = (await ConnectionWrapper.QueryAsync<SqlWorkFlowState>("GetCurrentWorkflowState", param, commandType: CommandType.StoredProcedure))
                .Select(workflowState => new WorkflowState
                {
                    Id = workflowState.WorkflowStateId,
                    Name = workflowState.WorkflowStateName,
                    WorkflowId = workflowState.WorkflowId
                }).FirstOrDefault();

            if (result == null)
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
                Item = result
            };
        }

        private async Task<WorkflowTransitionResult> GetAvailableTransitionsInternal(int userId, int workflowId, int stateId)
        {
            var param = new DynamicParameters();
            param.Add("@workflowId", workflowId);
            param.Add("@stateId", stateId);
            param.Add("@userId", userId);

            var workflowTransitions = (await ConnectionWrapper.QueryAsync<SqlWorkflowTransition>("GetAvailableTransitions", param, commandType: CommandType.StoredProcedure)).Select(wt => new WorkflowTransition
            {
                Id = wt.TriggerId,
                ToState = new WorkflowState
                {
                    WorkflowId = workflowId,
                    Id = wt.StateId,
                    Name = wt.StateName
                },
                FromState = new WorkflowState
                {
                    WorkflowId = workflowId,
                    Id = wt.CurrentStateId
                },
                Name = wt.TriggerName,
                WorkflowId = workflowId
            }).ToList();
            
            return new WorkflowTransitionResult
            {
                ResultCode = QueryResultCode.Success,
                Total = workflowTransitions.Count,
                Count = workflowTransitions.Count,
                Items = workflowTransitions
            };
        }

        private async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactInternal(int userId, int artifactId, int desiredStateId)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@artifactId", artifactId);
            param.Add("@desiredStateId", desiredStateId);

            var result = (await ConnectionWrapper.QueryAsync<SqlWorkFlowState>("ChangeStateForArtifact", param, commandType: CommandType.StoredProcedure))
                .Select(workflowState => new WorkflowState
                {
                    Id = workflowState.WorkflowStateId,
                    Name = workflowState.WorkflowStateName,
                    WorkflowId = workflowState.WorkflowId
                }).FirstOrDefault();

            if (result == null)
            {
                return new QuerySingleResult<WorkflowState>
                {
                    ResultCode = QueryResultCode.Failure,
                    Message = I18NHelper.FormatInvariant("State could not be modified")
                };
            }
            return new QuerySingleResult<WorkflowState>
            {
                ResultCode = QueryResultCode.Success,
                Item = result
            };
        }

        #endregion
    }
}
