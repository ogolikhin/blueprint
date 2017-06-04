using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories.Workflow
{
    public class SqlWorkflowRepository : SqlBaseArtifactRepository, IWorkflowRepository
    {
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;

        public SqlWorkflowRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain),
                  new SqlArtifactVersionsRepository())
        {
        }

        public SqlWorkflowRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactVersionsRepository artifactVersionsRepository)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper), 
                  artifactVersionsRepository)
        {
        }

        public SqlWorkflowRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IArtifactVersionsRepository artifactVersionsRepository) 
            : base(connectionWrapper,artifactPermissionsRepository)
        {
            _artifactVersionsRepository = artifactVersionsRepository;
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

        public async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifact(int userId, WorkflowStateChangeParameter stateChangeParameter)
        {
            //Confirm that the artifact is not deleted
            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(stateChangeParameter.ArtifactId);
            if (isDeleted)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            //Get artifact info
            var artifactInfo =
                await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(stateChangeParameter.ArtifactId,
                    null,
                    userId);

            if (artifactInfo.VersionCount > stateChangeParameter.CurrentVersionId)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            //Check that it is not locked by some other user
            if (artifactInfo.LockedByUser != null && artifactInfo.LockedByUser.Id != userId)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            //Obtain lock if it is not already locked by current user
            if (artifactInfo.LockedByUser == null)
            {
                var isLocked =
                    await _artifactVersionsRepository.LockArtifactAsync(stateChangeParameter.ArtifactId, userId);
                if (!isLocked)
                {
                    throw new ConflictException("Artifact has been updated. Please refresh your view.");
                }
            }

            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, stateChangeParameter.ArtifactId, stateChangeParameter.CurrentVersionId);

            //Get current state and validate current state
            var currentState = await GetCurrentState(userId, stateChangeParameter.ArtifactId);
            if (currentState == null || 
                currentState.ResultCode != QueryResultCode.Success || 
                currentState.Item == null ||
                currentState.Item.Id != stateChangeParameter.FromStateId)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            //Get available transitions and validate the required transition
            var availableTransitions =
                await GetAvailableTransitions(userId, stateChangeParameter.WorkflowId, stateChangeParameter.FromStateId);
            if (availableTransitions.Total == 0 ||
                availableTransitions.ResultCode != QueryResultCode.Success)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            var desiredTransition =
                availableTransitions.Items.FirstOrDefault(tr => tr.WorkflowId == stateChangeParameter.WorkflowId &&
                                                               tr.FromState.Id == stateChangeParameter.FromStateId &&
                                                               tr.ToState.Id == stateChangeParameter.ToStateId &&
                                                               tr.Id == stateChangeParameter.TransitionId);

            if (desiredTransition == null)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            //Change transition
            return await ChangeStateForArtifactInternal(userId, desiredTransition.Id);
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

        private async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactInternal(int userId, int transitionId)
        {
            var param = new DynamicParameters();
            param.Add("@transitionId", transitionId);
            param.Add("@userId", userId);

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
