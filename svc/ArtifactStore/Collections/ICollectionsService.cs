using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Models;
using ServiceLibrary.Models;

namespace ArtifactStore.Collections
{
    public interface ICollectionsService
    {
        Task<CollectionArtifacts> GetArtifactsInCollectionAsync(int collectionId, Pagination pagination, int userId);

        Task<AddArtifactsToCollectionResult> AddArtifactsToCollectionAsync(int collectionId, ISet<int> artifactIds, int userId);

        Task<GetColumnsDto> GetColumnsAsync(int collectionId, int userId, string search = null);

        Task SaveProfileColumnsAsync(int collectionId, ProfileColumns profileColumns, int userId);
    }
}
