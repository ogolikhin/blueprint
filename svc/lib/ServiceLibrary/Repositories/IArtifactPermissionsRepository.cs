using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface IArtifactPermissionsRepository
    {
        Task<Dictionary<int, RolePermissions>> GetArtifactPermissions(IEnumerable<int> itemIds, int sessionUserId, bool contextUser = false, int revisionId = int.MaxValue, bool addDrafts = true, IDbTransaction transaction = null);

        Task<Dictionary<int, RolePermissions>> GetArtifactPermissionDirectly(int itemId, int userId, int projectId);

        Task<bool> HasReadPermissions(int artifactId, int sessionUserId, bool contextUser = false, int revisionId = int.MaxValue, bool addDrafts = true);

        Task<bool> HasEditPermissions(int artifactId, int sessionUserId, bool contextUser = false, int revisionId = int.MaxValue, bool addDrafts = true, IDbTransaction transaction = null);

        Task<ProjectPermissions> GetProjectPermissions(int projectId);

        Task<ItemInfo> GetItemInfo(int itemId, int userId, bool addDrafts = true, int revisionId = int.MaxValue);
    }
}
