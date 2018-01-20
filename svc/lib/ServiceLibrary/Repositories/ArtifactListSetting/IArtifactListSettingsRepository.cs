using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ArtifactListSetting
{
    public interface IArtifactListSettingsRepository
    {
        Task<string> GetSettingsAsync(int itemId, int userId);
        Task<int> CreateSettingsAsync(int itemId, int userId, string settings);
        Task<int> UpdateSettingsAsync(int itemId, int userId, string settings);
    }
}
