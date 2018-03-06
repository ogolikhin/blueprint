using System;
using System.Collections.Generic;
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

        public async Task<ProfileSettings> GetProfileSettingsAsync(int itemId, int userId)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            if (existingSettings == null)
            {
                return null;
            }

            var profileSettings = new ProfileSettings
            {
                ProfileColumns = !existingSettings.Columns.IsEmpty()
                    ? ArtifactListHelper.ConvertXmlProfileSettingsToProfileColumns(existingSettings)
                    : null,
                PaginationLimit = ArtifactListHelper.ConvertXmlProfileSettingsToPaginationLimit(existingSettings)
            };

            return profileSettings;
        }

        public async Task<int?> GetPaginationLimitAsync(int itemId, int userId)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            return existingSettings != null
                ? ArtifactListHelper.ConvertXmlProfileSettingsToPaginationLimit(existingSettings)
                : null;
        }

        public async Task<ProfileColumns> GetProfileColumnsAsync(
            int itemId, int userId, ProfileColumns defaultColumns)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            return existingSettings == null || existingSettings.Columns.IsEmpty()
                ? defaultColumns
                : ArtifactListHelper.ConvertXmlProfileSettingsToProfileColumns(existingSettings);
        }


        public async Task<ProfileColumns> GetProfileColumnsAsync(int itemId, int userId)
        {
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            if (existingSettings == null || existingSettings.Columns.IsEmpty())
            {
                return null;
            }

            return ArtifactListHelper.ConvertXmlProfileSettingsToProfileColumns(existingSettings);
        }

        public async Task SaveProfileSettingsAsync(int itemId,  int userId, ProfileColumns profileColumns, int? paginationLimit)
        {
            var profileSettingsParams = new ProfileSettingsParams { PaginationLimit = paginationLimit };

            if (profileColumns != null)
            {
                profileSettingsParams.Columns = profileColumns;
            }

            await SaveSettingsAsync(itemId, userId, profileSettingsParams);
        }

        public async Task<int> SavePaginationLimitAsync(int itemId, int? paginationLimit, int userId)
        {
            return await SaveSettingsAsync(itemId, userId, new ProfileSettingsParams { PaginationLimit = paginationLimit });
        }

        public async Task<int> SaveProfileColumnsAsync(int itemId, ProfileColumns profileColumns, int userId)
        {
            return await SaveSettingsAsync(itemId, userId, new ProfileSettingsParams { Columns = profileColumns });
        }

        private async Task<int> SaveSettingsAsync(int itemId, int userId, ProfileSettingsParams profileSettings)
        {
            if (profileSettings == null)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant(
                    ErrorMessages.ArtifactList.SaveProfileSettingsProfileSettingsNull));
            }

            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            var settings =
                new XmlProfileSettings
                {
                    Columns = !profileSettings.ColumnsUndefined
                        ? ArtifactListHelper.ConvertProfileColumnsToXmlProfileSettings(profileSettings.Columns)
                        : existingSettings?.Columns,
                    PaginationLimit = !profileSettings.PaginationLimitUndefined ? profileSettings.PaginationLimit : existingSettings?.PaginationLimit,
                };

            if (existingSettings == null)
            {
                return await _artifactListSettingsRepository.CreateSettingsAsync(itemId, userId, settings);
            }
            else
            {
                return await _artifactListSettingsRepository.UpdateSettingsAsync(itemId, userId, settings);
            }
        }
    }
}
