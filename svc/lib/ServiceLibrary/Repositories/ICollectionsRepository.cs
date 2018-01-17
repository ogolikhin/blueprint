using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public interface ICollectionsRepository
    {
        Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, List<int> artifactIds, IDbTransaction transaction = null);

        Task RemoveDeletedArtifactsFromCollection(int collectionId, int userId, IDbTransaction transaction = null);
        Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(int collectionId, int userId, bool addDrafts = true);
    }
}
