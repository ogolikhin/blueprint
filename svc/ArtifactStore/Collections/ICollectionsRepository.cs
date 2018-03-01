using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Models;
using ServiceLibrary.Models;

namespace ArtifactStore.Collections
{
    public interface ICollectionsRepository
    {
        Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(int collectionId, int userId, bool addDrafts = true);

        Task<IReadOnlyList<ArtifactPropertyInfo>> GetArtifactsWithPropertyValuesAsync(
            int userId, IEnumerable<int> artifactIds, ProfileColumns profileColumns);

        Task<int> AddArtifactsToCollectionAsync(int collectionId, IEnumerable<int> artifactIds, int userId,
            IDbTransaction transaction = null);

        Task<int> RemoveArtifactsFromCollectionAsync(int collectionId, IEnumerable<int> artifactIds, int userId,
            IDbTransaction transaction = null);

        Task RemoveDeletedArtifactsFromCollectionAsync(int collectionId, int userId, IDbTransaction transaction = null);

        Task<IReadOnlyList<PropertyTypeInfo>> GetPropertyTypeInfosForItemTypesAsync(
            IEnumerable<int> itemTypeIds, string search = null);
    }
}
