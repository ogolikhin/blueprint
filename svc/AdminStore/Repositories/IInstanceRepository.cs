﻿using AdminStore.Models;
using AdminStore.Models.DTO;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AdminStore.Models.Enums;

namespace AdminStore.Repositories
{
    public interface IInstanceRepository
    {
        Task<InstanceItem> GetInstanceFolderAsync(int folderId, int userId, bool fromAdminPortal = false);

        Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int folderId, int userId, bool fromAdminPortal = false);

        Task<InstanceItem> GetInstanceProjectAsync(int projectId, int userId, bool fromAdminPortal = false);

        Task DeactivateWorkflowsWithLastAssignmentForDeletedProject(int projectId, IDbTransaction transaction = null);

        Task<List<string>> GetProjectNavigationPathAsync(int userId, int projectId, bool includeProjectItself);

        Task<IEnumerable<AdminRole>> GetInstanceRolesAsync();

        Task<int> CreateFolderAsync(FolderDto folder);

        Task<IEnumerable<InstanceItem>> GetFoldersByName(string name);

        Task<int> DeleteInstanceFolderAsync(int instanceFolderId);

        Task UpdateFolderAsync(int folderId, FolderDto folderDto);

        Task<int> RemoveProject(int userId, int projectId, IDbTransaction transaction = null);

        Task PurgeProject(int projectId, InstanceItem project);
        Task<IEnumerable<ProjectRole>> GetProjectRolesAsync(int projectId);

        Task<RoleAssignmentQueryResult<RoleAssignment>> GetProjectRoleAssignmentsAsync(int projectId, TabularData tabularData,
            Func<Sorting, string> sort = null);

        Task<int> DeleteRoleAssignmentsAsync(int projectId, OperationScope scope, string search);

        Task<int> HasProjectExternalLocksAsync(int userId, int projectId);

        Task<QueryResult<ProjectFolderSearchDto>> GetProjectsAndFolders(int userId, TabularData tabularData, Func<Sorting, string> sort = null);

        Task<int> CreateRoleAssignmentAsync(int projectId, RoleAssignmentDTO roleAssignment);

        Task UpdateRoleAssignmentAsync(int projectId, int roleAssignmentId, RoleAssignmentDTO roleAssignment);

        Task RunInTransactionAsync(Func<IDbTransaction, long, Task> action);

        Task<IEnumerable<int>> GetProjectArtifactIds(int projectId, int revisionId, int userId, bool? addDrafts = false, IDbTransaction transaction = null);
    }
}