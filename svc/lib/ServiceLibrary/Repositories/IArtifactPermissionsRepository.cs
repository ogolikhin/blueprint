using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface IArtifactPermissionsRepository
    {
        Task<Dictionary<int, RolePermissions>> GetArtifactPermissions(IEnumerable<int> itemIds, int sessionUserId, bool contextUser = false, int revisionId = int.MaxValue, bool addDrafts = true);
        Task<Dictionary<int, RolePermissions>> GetArtifactPermissionsInChunks(List<int> itemIds, int sessionUserId, bool contextUser = false, int revisionId = int.MaxValue, bool addDrafts = true);
        Task<ProjectPermissions> GetProjectPermissions(int projectId);
        Task<ItemInfo> GetItemInfo(int itemId, int userId, bool addDrafts = true, int revisionId = int.MaxValue);
    }
}
