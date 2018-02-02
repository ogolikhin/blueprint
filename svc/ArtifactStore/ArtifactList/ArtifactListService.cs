using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Helpers;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.ArtifactList.Models.Xml;
using ServiceLibrary.Helpers;

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

        public async Task<int?> GetPaginationLimitAsync(int itemId, int userId)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            return existingSettings != null
                ? ArtifactListHelper.ConvertXmlProfileSettingsToPaginationLimit(existingSettings)
                : null;
        }

        public async Task<ProfileColumns> GetProfileColumnsAsync(
            int itemId, int userId, ProfileColumns defaultColumns = null)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            return existingSettings == null || existingSettings.Columns.IsEmpty()
                ? defaultColumns
                : ArtifactListHelper.ConvertXmlProfileSettingsToProfileColumns(existingSettings);
        }

        public async Task<int> SavePaginationLimitAsync(int itemId, int? paginationLimit, int userId)
        {
            var paginationLimitSettings = ArtifactListHelper.ConvertPaginationLimitToXmlProfileSettings(paginationLimit);

            return await SaveSettingsAsync(itemId, userId, null, paginationLimitSettings);
        }

        public async Task<int> SaveProfileColumnsAsync(int itemId, ProfileColumns profileColumns, int userId)
        {
            var profileColumnsSettings = ArtifactListHelper.ConvertProfileColumnsToXmlProfileSettings(profileColumns);

            return await SaveSettingsAsync(itemId, userId, profileColumnsSettings, paginationLimitSettings: null);
        }

        private async Task<int> SaveSettingsAsync(int itemId, int userId, XmlProfileSettings profileColumnsSettings, XmlProfileSettings paginationLimitSettings)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            var settings = profileColumnsSettings != null || paginationLimitSettings != null
                ? new XmlProfileSettings
                {
                    Columns = profileColumnsSettings != null ? profileColumnsSettings.Columns : existingSettings?.Columns,
                    PaginationLimit = paginationLimitSettings != null ? paginationLimitSettings.PaginationLimit : existingSettings?.PaginationLimit,
                }
                : null;

            if (existingSettings == null && settings != null)
            {
                return await _artifactListSettingsRepository.CreateSettingsAsync(itemId, userId, settings);
            }
            else if (existingSettings != null)
            {
                return await _artifactListSettingsRepository.UpdateSettingsAsync(itemId, userId, settings);
            }
            else
            {
                return 0;
            }
        }
    }
}
