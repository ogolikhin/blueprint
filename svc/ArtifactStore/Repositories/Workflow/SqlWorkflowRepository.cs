using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
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
        public async Task<WorkflowTransitionResult> GetTransitions(int artifactId, int userId)
        {
            CheckForArtifactPermissions(userId, artifactId);
            
            return await GetTransitionsInternal(artifactId, userId);
        }

        public Task<WorkflowState> GetCurrentState(int userId, int itemId, int revisionId = int.MaxValue, bool addDrafts = true)
        {
            //Need to access code for artifact permissions for revision
            CheckForArtifactPermissions(userId, itemId, revisionId);
            
            return GetCurrentStateInternal(userId, itemId, revisionId, addDrafts);
        }

        public async Task<WorkflowTransitionResult> GetAvailableTransitions(int userId, int workflowId, int stateId)
        {
            return await GetAvailableTransitionsInternal(userId, workflowId, stateId);
        }

        private async void CheckForArtifactPermissions(int userId, int artifactId, int revisionId = int.MaxValue)
        {
            var artifactBasicDetails = await GetArtifactBasicDetails(ConnectionWrapper, artifactId, userId);
            if (artifactBasicDetails == null || artifactBasicDetails.RevisionId > revisionId)
            {
                ExceptionHelper.ThrowArtifactNotFoundException(artifactId);
            }

            var artifactsPermissions =
                await ArtifactPermissionsRepository.GetArtifactPermissions(new List<int> { artifactId }, userId);

            if (!artifactsPermissions.ContainsKey(artifactId) ||
                !artifactsPermissions[artifactId].HasFlag(RolePermissions.Read))
            {
                ExceptionHelper.ThrowArtifactForbiddenException(artifactId);
            }

        }

        private async Task<WorkflowTransitionResult> GetTransitionsInternal(int artifactId, int userId)
        {
            var param = new DynamicParameters();
            param.Add("@artifactId", artifactId);
            param.Add("@userId", userId);

            var workflowTransitions = (await ConnectionWrapper.QueryAsync<WorkflowTransition>("GetAvailableTransitions", param, commandType: CommandType.StoredProcedure)).ToList();
            return new WorkflowTransitionResult()
            {
                Items = workflowTransitions,
                Count = workflowTransitions.Count,
                Total = workflowTransitions.Count
            };
        }

        private async Task<WorkflowState> GetCurrentStateInternal(int userId, int itemId, int revisionId, bool addDrafts)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@itemId", itemId);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);

            return (await ConnectionWrapper.QueryAsync<WorkflowState>("GetCurrentWorkflowState", param, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        private async Task<WorkflowTransitionResult> GetAvailableTransitionsInternal(int userId, int workflowId, int stateId)
        {
            var param = new DynamicParameters();
            param.Add("@workflowId", workflowId);
            param.Add("@stateId", stateId);
            param.Add("@userId", userId);

            var workflowTransitions = (await ConnectionWrapper.QueryAsync<WorkflowTransition>("GetNextAccessibleStates", param, commandType: CommandType.StoredProcedure)).ToList();
            return new WorkflowTransitionResult
            {
                Items = workflowTransitions,
                Count = workflowTransitions.Count,
                Total = workflowTransitions.Count
            };
        }
        #endregion
    }
}
