using AdminStore.Models;
using AdminStore.Models.DTO;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    public interface IInstanceRepository
    {
        Task<InstanceItem> GetInstanceFolderAsync(int folderId, int userId);

        Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int folderId, int userId, bool fromAdminPortal = false);

        Task<InstanceItem> GetInstanceProjectAsync(int projectId, int userId, bool fromAdminPortal = false);

        Task<List<string>> GetProjectNavigationPathAsync(int userId, int projectId, bool includeProjectItself);

        Task<IEnumerable<AdminRole>> GetInstanceRolesAsync();

        Task<int> CreateFolderAsync(FolderDto folder);

        Task<IEnumerable<InstanceItem>> GetFoldersByName(string name);

        Task<int> DeleteInstanceFolderAsync(int instanceFolderId);

        Task UpdateFolderAsync(int folderId, FolderDto folderDto);

        Task UpdateProjectAsync(int projectId, ProjectDto projectDto);

        Task DeleteProject(int userId, int projectId);

        Task<IEnumerable<ProjectRole>> GetProjectRolesAsync(int projectId);

        Task<QueryResult<RolesAssignments>> GetProjectRoleAssignmentsAsync(int projectId, TabularData tabularData,
            Func<Sorting, string> sort = null);
        
        Task<int> DeleteRoleAssignmentsAsync(int projectId, OperationScope scope, string search);

        Task<QueryResult<ProjectFolderSearchDto>> GetProjectsAndFolders(int userId, TabularData tabularData, Func<Sorting, string> sort = null);
    }
}