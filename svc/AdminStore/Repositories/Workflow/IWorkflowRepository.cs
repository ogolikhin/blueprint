﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.DTO;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.Files;

namespace AdminStore.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        IFileRepository FileRepository { get; set; }


        // Only the name and the description of DWorkflow are used.
        Task<IEnumerable<SqlWorkflow>> CreateWorkflowsAsync(IEnumerable<SqlWorkflow> workflows, int publishRevision, IDbTransaction transaction = null);

        Task<IEnumerable<SqlState>> CreateWorkflowStatesAsync(IEnumerable<SqlState> workflowStates, int publishRevision,
            IDbTransaction transaction = null);

        Task<IEnumerable<SqlState>> UpdateWorkflowStatesAsync(IEnumerable<SqlState> workflowStates, int publishRevision,
            IDbTransaction transaction = null);

        Task<IEnumerable<int>> DeleteWorkflowStatesAsync(IEnumerable<int> workflowStateIds, int publishRevision,
            IDbTransaction transaction = null);

        Task<IEnumerable<SqlWorkflowEvent>> CreateWorkflowEventsAsync(IEnumerable<SqlWorkflowEvent> workflowEvents,
            int publishRevision, IDbTransaction transaction = null);

        Task<IEnumerable<SqlWorkflowEvent>> UpdateWorkflowEventsAsync(IEnumerable<SqlWorkflowEvent> workflowEvents,
            int publishRevision, IDbTransaction transaction = null);

        Task<IEnumerable<int>> DeleteWorkflowEventsAsync(IEnumerable<int> workflowEventIds,
            int publishRevision, IDbTransaction transaction = null);

        Task CreateWorkflowArtifactAssociationsAsync(IEnumerable<KeyValuePair<int, string>> projectArtifactTypePair,
            int workflowId, int publishRevision, IDbTransaction transaction = null);

        Task UpdateWorkflowArtifactAssignmentsAsync(IEnumerable<KeyValuePair<int, string>> artifactTypeToAddKvPairs, IEnumerable<KeyValuePair<int, string>> artifactTypeToDeleteKvPairs,
            int workflowId, IDbTransaction transaction = null);

        Task DeleteWorkflowArtifactAssociationsAsync(IEnumerable<KeyValuePair<int, string>> projectArtifactTypePair,
            int publishRevision, IDbTransaction transaction = null);

        Task<IEnumerable<SqlProjectPathPair>> GetProjectIdsByProjectPathsAsync(IEnumerable<string> projectPaths);

        Task<IEnumerable<SqlArtifactTypesWorkflowDetails>> GetExistingStandardArtifactTypesForWorkflowsAsync(IEnumerable<string> artifactTypes, IEnumerable<int> projectIds);

        Task<IEnumerable<string>> GetExistingPropertyTypesByName(IEnumerable<string> propertyTypeNames);

        Task<IEnumerable<int>> GetExistingProjectsByIdsAsync(IEnumerable<int> projectIds);

        Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string description);

        Task<IEnumerable<string>> CheckLiveWorkflowsForNameUniquenessAsync(IEnumerable<string> names, int? exceptWorkflowId = null);

        Task RunInTransactionAsync(Func<IDbTransaction, long, Task> action);

        Task<QueryResult<WorkflowDto>> GetWorkflows(Pagination pagination, Sorting sorting = null, string search = null,
            Func<Sorting, string> sort = null);

        Task<SqlWorkflow> GetWorkflowDetailsAsync(int workflowId);

        Task<IEnumerable<SqlWorkflowArtifactTypes>> GetWorkflowArtifactTypesAsync(int workflowId);

        Task<int> DeleteWorkflowsAsync(OperationScope body, string search, int revision, IDbTransaction transaction = null);

        Task<int> UpdateWorkflowsAsync(IEnumerable<SqlWorkflow> workflows, int revision,
            IDbTransaction transaction = null);

        Task UpdateWorkflowAsync(SqlWorkflow workflow, int revision,
            IDbTransaction transaction = null);

        Task<IEnumerable<SqlState>> GetWorkflowStatesAsync(int workflowId);

        Task<IEnumerable<SqlWorkflowEventData>> GetWorkflowEventsAsync(int workflowId);
        Task UpdateWorkflowsChangedWithRevisionsAsync(int workflowId, int revisionId, IDbTransaction transaction = null);
        Task<int> CreateWorkflow(SqlWorkflow workflow, int revision, IDbTransaction transaction);

        Task<List<InstanceItem>> GetWorkflowAvailableProjectsAsync(int workflowId, int folderId);
        Task<QueryResult<WorkflowProjectArtifactTypesDto>> GetProjectArtifactTypesAssignedtoWorkflowAsync(int workflowId, Pagination pagination, string search = null);

        Task<AssignProjectsResult> AssignProjectsAndArtifactTypesToWorkflow(int workFlowId, WorkflowAssignScope scope);

        Task<SyncResult> AssignArtifactTypesToProjectInWorkflow(int workFlowId, int projectId, OperationScope scope);
        Task<int> UnassignProjectsAndArtifactTypesFromWorkflowAsync(int workflowId, OperationScope scope, string search = null);

        Task<IEnumerable<WorkflowProjectSearch>> SearchProjectsByName(int workflowId, string search = null);
        Task<int> CopyWorkflowAsync(int workflowId, int userId, CopyWorkflowDto copyWorkfloDto);
    }
}