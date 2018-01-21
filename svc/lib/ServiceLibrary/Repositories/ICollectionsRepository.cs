using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;

namespace ServiceLibrary.Repositories
{
    public interface ICollectionsRepository
    {
        Task<int> AddArtifactsToCollectionAsync(int userId, int collectionId, List<int> artifactIds, IDbTransaction transaction = null);

        Task RemoveDeletedArtifactsFromCollectionAsync(int collectionId, int userId, IDbTransaction transaction = null);
        Task<CollectionArtifacts> GetArtifactsWithPropertyValues(int userId, IEnumerable<int> artifactIds);

        Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(int collectionId, int userId, bool addDrafts = true);
    }
}
