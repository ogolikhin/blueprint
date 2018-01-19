using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;

namespace ServiceLibrary.Repositories
{
    public interface ICollectionsRepository
    {
        Task<CollectionArtifacts> GetArtifactsWithPropertyValues(int userId, IEnumerable<int> artifactIds);

        Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(int collectionId, int userId, bool addDrafts = true);
    }
}
