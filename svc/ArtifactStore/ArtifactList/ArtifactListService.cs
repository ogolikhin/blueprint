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

        public async Task<ProfileColumns> GetProfileColumnsAsync(
            int itemId, int userId, ProfileColumns defaultColumns = null)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            return existingSettings == null
                ? defaultColumns
                : ArtifactListHelper.ConvertXmlProfileSettingsToProfileColumns(existingSettings);
        }

        public async Task<int> SaveProfileColumnsAsync(int itemId, ProfileColumns profileColumns, int userId)
        {
            var settings = ArtifactListHelper.ConvertProfileColumnsToXmlProfileSettings(profileColumns);
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            if (existingSettings == null)
            {
                return await _artifactListSettingsRepository.CreateSettingsAsync(itemId, userId, settings);
            }

            return await _artifactListSettingsRepository.UpdateSettingsAsync(itemId, userId, settings);
        }
    }
}
