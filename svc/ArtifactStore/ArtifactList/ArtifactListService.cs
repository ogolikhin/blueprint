using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Helpers;
using ArtifactStore.ArtifactList.Models;
using ServiceLibrary.Helpers;

namespace ArtifactStore.ArtifactList
{
    public class ArtifactListService : IArtifactListService
    {
        private readonly IArtifactListSettingsRepository _artifactListSettingsRepository;

        public ArtifactListService() : this(new SqlArtifactListSettingsRepository())
        {
        }

        private ArtifactListService(IArtifactListSettingsRepository artifactListSettingsRepository)
        {
            _artifactListSettingsRepository = artifactListSettingsRepository;
        }

        public async Task<ProfileColumnsSettings> GetColumnSettingsAsync(int itemId, int userId)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            if (string.IsNullOrEmpty(existingSettings))
            {
                return null;
            }

            return ArtifactListHelper.ConvertXmlProfileSettingsToProfileColumnSettings(
                SerializationHelper.FromXml<XmlProfileSettings>(existingSettings));
        }

        public async Task<int> SaveColumnsSettingsAsync(int itemId, ProfileColumnsSettings columnSettings, int userId)
        {
            var settings = SerializationHelper.ToXml(
                ArtifactListHelper.ConvertProfileColumnsSettingsToXmlProfileSettings(columnSettings));

            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            if (string.IsNullOrWhiteSpace(existingSettings))
            {
                return await _artifactListSettingsRepository.CreateSettingsAsync(itemId, userId, settings);
            }

            return await _artifactListSettingsRepository.UpdateSettingsAsync(itemId, userId, settings);
        }
    }
}
