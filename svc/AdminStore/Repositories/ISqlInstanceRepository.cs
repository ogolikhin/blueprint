using AdminStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    public interface ISqlInstanceRepository
    {
        Task<InstanceItem> GetInstanceFolderAsync(int folderId, int userId);

        Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int folderId, int userId);

        Task<InstanceItem> GetInstanceProjectAsync(int projectId, int userId);

        Task<List<string>> GetProjectNavigationPathAsync(int userId, int projectId, bool includeProjectItself);
    }
}