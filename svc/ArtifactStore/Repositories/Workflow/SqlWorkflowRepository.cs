using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IList<WorkflowTransition>> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId)
        {
            //Do not return transitions if the user does not have edit permissions
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Edit);
            
            return await GetTransitionsForStateInternalAsync(userId, workflowId, stateId);
        }

        public async Task<WorkflowTransition> GetTransitionForAssociatedStatesAsync(int userId, int artifactId, int workflowId, int fromStateId, int toStateId)
        {
            //Do not return transitions if the user does not have edit permissions
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Edit);

            return await GetTransitionForAssociatedStatesInternalAsync(userId, workflowId, fromStateId, toStateId);
        }

        public async Task<WorkflowState> GetStateForArtifactAsync(int userId, int artifactId, int revisionId, bool addDrafts)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, artifactId, revisionId);
            
            return await GetCurrentStateInternal(userId, artifactId, revisionId, addDrafts);
        }

        public async Task<WorkflowState> ChangeStateForArtifactAsync(
            int userId, 
            int artifactId, 
            WorkflowStateChangeParameterEx stateChangeParameter, 
            IDbTransaction transaction = null)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Edit);

            return await ChangeStateForArtifactInternal(
                userId, 
                artifactId, 
                stateChangeParameter.RevisionId, 
                stateChangeParameter.ToStateId,
                transaction);
        }

        #endregion

        #region Private methods

        private async Task<WorkflowState> GetCurrentStateInternal(int userId, int artifactId, int revisionId, bool addDrafts)
        {
            return (await GetCurrentStatesInternal(userId, 
                new [] { artifactId }, revisionId, addDrafts)).FirstOrDefault();
        }

        private async Task<IList<WorkflowState>> GetCurrentStatesInternal(int userId, IEnumerable<int>  artifactIds, int revisionId, bool addDrafts)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);

            return ToWorkflowStates(
                await 
                    ConnectionWrapper.QueryAsync<SqlWorkFlowState>("GetWorkflowStatesForArtifacts", param, 
                        commandType: CommandType.StoredProcedure));
        }

        private async Task<IList<WorkflowTransition>> GetTransitionsForStateInternalAsync(int userId, int workflowId, int stateId)
        {
            var param = new DynamicParameters();
            param.Add("@workflowId", workflowId);
            param.Add("@stateId", stateId);
            param.Add("@userId", userId);

            return ToWorkflowTransitions(
                    await
                        ConnectionWrapper.QueryAsync<SqlWorkflowTransition>("GetTransitionsForState", param,
                            commandType: CommandType.StoredProcedure));
        }

        private async Task<WorkflowTransition> GetTransitionForAssociatedStatesInternalAsync(int userId, int workflowId, int fromStateId, int toStateId)
        {
            var param = new DynamicParameters();
            param.Add("@workflowId", workflowId);
            param.Add("@fromStateId", fromStateId);
            param.Add("@toStateId", toStateId);
            param.Add("@userId", userId);

            return ToWorkflowTransitions(
                    await
                        ConnectionWrapper.QueryAsync<SqlWorkflowTransition>("GetTransitionAssociatedWithStates", param,
                            commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        private IList<WorkflowTransition> ToWorkflowTransitions(IEnumerable<SqlWorkflowTransition> sqlWorkflowTransitions)
        {
            return sqlWorkflowTransitions.Select(wt => new WorkflowTransition
            {
                Id = wt.WorkflowEventId,
                ToState = new WorkflowState
                {
                    WorkflowId = wt.WorkflowId,
                    Id = wt.ToStateId,
                    Name = wt.ToStateName
                },
                FromState = new WorkflowState
                {
                    WorkflowId = wt.WorkflowId,
                    Id = wt.FromStateId,
                    Name = wt.FromStateName
                },
                Name = wt.WorkflowEventName,
                WorkflowId = wt.WorkflowId
            }).ToList();
        }

        private async Task<WorkflowState> ChangeStateForArtifactInternal(int userId, int artifactId, int revisionId, int desiredStateId, IDbTransaction transaction = null)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@artifactId", artifactId);
            param.Add("@revisionId", revisionId);
            param.Add("@desiredStateId", desiredStateId);
            param.Add("@result");

            if (transaction == null)
            {
                return
                    ToWorkflowStates(await
                        ConnectionWrapper.QueryAsync<SqlWorkFlowState>("ChangeStateForArtifact", param,
                            commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
            return
                ToWorkflowStates(await
                    transaction.Connection.QueryAsync<SqlWorkFlowState>("ChangeStateForArtifact", param, transaction,
                            commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        private IList<WorkflowState> ToWorkflowStates(IEnumerable<SqlWorkFlowState> sqlWorkFlowStates)
        {
            return sqlWorkFlowStates.Select(workflowState => new WorkflowState
            {
                Id = workflowState.WorkflowStateId,
                Name = workflowState.WorkflowStateName,
                WorkflowId = workflowState.WorkflowId
            }).ToList();
        }

        #endregion
    }
}
