﻿using System;
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
                Columns = !existingSettings.Columns.IsEmpty()
                    ? ArtifactListHelper.ConvertXmlProfileSettingsToProfileColumns(existingSettings)
                    : null,
                PaginationLimit = ArtifactListHelper.ConvertXmlProfileSettingsToPaginationLimit(existingSettings)
            };

            return profileSettings;
        }

        public async Task SaveProfileSettingsAsync(int itemId,  int userId, ProfileColumns profileColumns, int? paginationLimit)
        {
            var profileSettingsParams = new ProfileSettings { PaginationLimit = paginationLimit, Columns = profileColumns };

            await SaveSettingsAsync(itemId, userId, profileSettingsParams);
        }

        public async Task<int> SavePaginationLimitAsync(int itemId, int? paginationLimit, int userId)
        {
            return await SaveSettingsAsync(itemId, userId, new ProfileSettings { PaginationLimit = paginationLimit });
        }

        public async Task<int> SaveProfileColumnsAsync(int itemId, ProfileColumns profileColumns, int userId)
        {
            return await SaveSettingsAsync(itemId, userId, new ProfileSettings { Columns = profileColumns });
        }

        private async Task<int> SaveSettingsAsync(int itemId, int userId, ProfileSettings profileSettings)
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
