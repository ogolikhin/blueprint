using System.Threading.Tasks;

namespace ArtifactStore.ArtifactList
{
    public interface IArtifactListSettingsRepository
    {
        Task<string> GetSettingsAsync(int itemId, int userId);
        Task<int> CreateSettingsAsync(int itemId, int userId, string settings);
        Task<int> UpdateSettingsAsync(int itemId, int userId, string settings);
    }
}
