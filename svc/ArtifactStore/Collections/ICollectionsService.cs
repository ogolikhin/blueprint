using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Collections.Models;
using ServiceLibrary.Models;

namespace ArtifactStore.Collections
{
    public interface ICollectionsService
    {
        Task<CollectionArtifacts> GetArtifactsInCollectionAsync(int collectionId, Pagination pagination, int userId);

        Task<AddArtifactsResult> AddArtifactsToCollectionAsync(int collectionId, ISet<int> artifactIds, int userId);
    }
}
