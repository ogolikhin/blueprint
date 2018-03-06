using ArtifactStore.Models;
using ServiceLibrary.Models;
using System.Threading.Tasks;
using ServiceLibrary.Models.VersionControl;
using System.Collections.Generic;
using System.Data;

namespace ArtifactStore.Repositories
{
    public interface IArtifactVersionsRepository
    {
        Task<ArtifactHistoryResultSet> GetArtifactVersions(int artifactId, int limit, int offset, int? userId, bool asc, int sessionUserId, bool includeDrafts);

        Task<bool> IsItemDeleted(int itemId);

        Task<DeletedItemInfo> GetDeletedItemInfo(int itemId);

        Task<VersionControlArtifactInfo> GetVersionControlArtifactInfoAsync(int itemId, int? baselineId, int userId, IDbTransaction transaction = null);

        Task<IEnumerable<int>> GetDeletedAndNotInProjectItems(IEnumerable<int> itemIds, int projectId);
    }
}
