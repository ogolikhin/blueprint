using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Collections.Models;

namespace ArtifactStore.Collections
{
    public interface ICollectionsRepository
    {
        Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(int collectionId, int userId, bool addDrafts = true);

        Task<IReadOnlyList<CollectionArtifact>> GetArtifactsWithPropertyValuesAsync(
            int userId, IEnumerable<int> artifactIds);

        Task<int> AddArtifactsToCollectionAsync(int collectionId, IEnumerable<int> artifactIds, int userId,
            IDbTransaction transaction = null);

        Task RemoveDeletedArtifactsFromCollectionAsync(int collectionId, int userId, IDbTransaction transaction = null);
    }
}
