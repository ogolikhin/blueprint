using ArtifactStore.Models;
using ServiceLibrary.Models;
using System.Threading.Tasks;
using ServiceLibrary.Models.VersionControl;
using System.Collections.Generic;

namespace ArtifactStore.Repositories
{
    public interface IArtifactVersionsRepository
    {
        Task<ArtifactHistoryResultSet> GetArtifactVersions(int artifactId, int limit, int offset, int? userId, bool asc, int sessionUserId, bool includeDrafts);

        Task<bool> IsItemDeleted(int itemId);

        Task<DeletedItemInfo> GetDeletedItemInfo(int itemId);

        Task<VersionControlArtifactInfo> GetVersionControlArtifactInfoAsync(int itemId, int? baselineId, int userId);

        Task<bool> LockArtifactAsync(int artifactId, int userId);

        Task<IEnumerable<int>> GetDeletedItems(IEnumerable<int> itemIds);
    }
}