using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IArtifactPermissionsRepository
    {
        Task<Dictionary<int, RolePermissions>> GetArtifactPermissions(IEnumerable<int> itemIds, int userId, bool contextUser = false, int? revisionId = null);
    }
}