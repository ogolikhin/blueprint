using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models.Xml;

namespace ArtifactStore.ArtifactList
{
    public interface IArtifactListSettingsRepository
    {
        Task<XmlProfileSettings> GetSettingsAsync(int itemId, int userId);

        Task<int> CreateSettingsAsync(int itemId, int userId, XmlProfileSettings settings);

        Task<int> UpdateSettingsAsync(int itemId, int userId, XmlProfileSettings settings);
    }
}
