using System.Collections.Generic;
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
    public class SqlWorkflowRepository : SqlBaseArtifactRepository, ISqlWorkflowRepository
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

        public async Task<QuerySingleResult<WorkflowState>> GetCurrentState(int userId, int artifactId, int revisionId = int.MaxValue, bool addDrafts = true)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, artifactId, revisionId);
            
            return await GetCurrentStateInternal(userId, artifactId, revisionId, addDrafts);
        }

        #endregion

        #region Private methods

        private async Task<WorkflowTransitionResult> GetAvailableTransitions(int userId, int workflowId, int stateId)
        {
            return await GetAvailableTransitionsInternal(userId, workflowId, stateId);
        }

        /// <summary>
        /// Checks whether the user has permission for this artifact. 
        /// if a revision Id is provided, the artifact's revision has to be less than the current revision.
        /// If the artifact is not a regular artifact type then we throw a non-supported exception.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="artifactId"></param>
        /// <param name="revisionId"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        private async Task CheckForArtifactPermissions(int userId, int artifactId, int revisionId = int.MaxValue, RolePermissions permissions = RolePermissions.Read)
        {
            var artifactBasicDetails = await GetArtifactBasicDetails(ConnectionWrapper, artifactId, userId);
            if (artifactBasicDetails == null || artifactBasicDetails.RevisionId > revisionId)
            {
                ExceptionHelper.ThrowArtifactNotFoundException(artifactId);
            }

            if (!((ItemTypePredefined) artifactBasicDetails.PrimitiveItemTypePredefined).IsRegularArtifactType())
            {
                ExceptionHelper.ThrowArtifactDoesNotSupportOperation(artifactId);
            }

            var artifactsPermissions =
                await ArtifactPermissionsRepository.GetArtifactPermissions(new List<int> { artifactId }, userId);

            if (!artifactsPermissions.ContainsKey(artifactId) ||
                !artifactsPermissions[artifactId].HasFlag(permissions))
            {
                ExceptionHelper.ThrowArtifactForbiddenException(artifactId);
            }
        }

        private async Task<QuerySingleResult<WorkflowState>> GetCurrentStateInternal(int userId, int artifactId, int revisionId, bool addDrafts)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@artifactId", artifactId);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);
            
            var result = (await ConnectionWrapper.QueryAsync<dynamic>("GetCurrentWorkflowState", param, commandType: CommandType.StoredProcedure))
                .Select(workflowState => new WorkflowState
                {
                    StateId = workflowState.WorkflowStateId,
                    StateName = workflowState.WorkflowStateName,
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

            var workflowTransitions = (await ConnectionWrapper.QueryAsync<dynamic>("GetAvailableTransitions", param, commandType: CommandType.StoredProcedure)).Select(workflowTransition => new WorkflowTransition
            {
                TransitionId = workflowTransition.TriggerId,
                WorkflowToStateId = workflowTransition.StateId,
                WorkflowToStateName = workflowTransition.StateName,
                WorkflowFromStateId = workflowTransition.CurrentStateId,
                TransitionName = workflowTransition.TriggerName,
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

        #endregion
    }
}
