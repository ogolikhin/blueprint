using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;

namespace ArtifactStore.Services.Collections
{
    public interface ICollectionsService
    {
        Task<CollectionArtifacts> GetArtifactsInCollectionAsync(int collectionId, Pagination pagination, int userId);
    }
}
