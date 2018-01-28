using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Helpers;
using ArtifactStore.ArtifactList.Models;

namespace ArtifactStore.ArtifactList
{
    public class ArtifactListService : IArtifactListService
    {
        private readonly IArtifactListSettingsRepository _artifactListSettingsRepository;

        public ArtifactListService() : this(new SqlArtifactListSettingsRepository())
        {
        }

        public ArtifactListService(IArtifactListSettingsRepository artifactListSettingsRepository)
        {
            _artifactListSettingsRepository = artifactListSettingsRepository;
        }

        public async Task<ProfileColumnsSettings> GetColumnSettingsAsync(int itemId, int userId)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            return existingSettings == null
                ? null
                : ArtifactListHelper.ConvertXmlProfileSettingsToProfileColumnSettings(existingSettings);
        }

        public async Task<int> SaveColumnsSettingsAsync(int itemId, ProfileColumnsSettings columnSettings, int userId)
        {
            var settings = ArtifactListHelper.ConvertProfileColumnsSettingsToXmlProfileSettings(columnSettings);
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            if (existingSettings == null)
            {
                return await _artifactListSettingsRepository.CreateSettingsAsync(itemId, userId, settings);
            }

            return await _artifactListSettingsRepository.UpdateSettingsAsync(itemId, userId, settings);
        }
    }
}
