using AdminStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models.DTO;

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

        Task<IEnumerable<FolderDto>> GetFoldersByName(string name);

        Task<int> DeleteInstanceFolderAsync(int instanceFolderId);

        Task UpdateFolderAsync(int folderId, FolderDto folderDto);

        Task UpdateProjectAsync(int projectId, ProjectDto projectDto);
    }
}