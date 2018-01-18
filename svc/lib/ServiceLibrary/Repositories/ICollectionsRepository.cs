using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface ICollectionsRepository
    {
        Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(int collectionId, int userId, bool addDrafts = true);
        Task<int> CreateArtifactListSettingsAsync(int collectionId, int userId, string settings);
        Task<int> UpdateArtifactListSettingsAsync(int collectionId, int userId, string settings);
    }
}
