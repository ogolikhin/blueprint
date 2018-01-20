using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Collection;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ArtifactListSetting;

namespace ArtifactStore.Services.ArtifactListSettings
{
    public class ArtifactListSettingsService : IArtifactListSettingsService
    {
        private readonly IArtifactListSettingsRepository _artifactListSettingsRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IArtifactRepository _sqlArtifactRepository;

        public ArtifactListSettingsService() : this(
            new ArtifactListSettingsRepository(),
            new SqlArtifactPermissionsRepository(),
            new SqlArtifactRepository())
        {
        }

        internal ArtifactListSettingsService(
            IArtifactListSettingsRepository artifactListSettingsRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IArtifactRepository sqlArtifactRepository)
        {
            _artifactListSettingsRepository = artifactListSettingsRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _sqlArtifactRepository = sqlArtifactRepository;
        }

        public async Task<int> SaveArtifactListColumnsSettings(
            int itemId, int userId, ArtifactListColumnsSettings artifactListColumnsSettings)
        {
            if (!await _artifactPermissionsRepository.HasReadPermissions(itemId, userId))
            {
                throw CollectionsExceptionHelper.NoAccessException(itemId);
            }

            var artifactBasicDetails = await _sqlArtifactRepository.GetArtifactBasicDetails(itemId, userId);

            if (artifactBasicDetails == null)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotFound, itemId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            var settings =
                SerializationHelper.ToXml(artifactListColumnsSettings.ConvertToArtifactListColumnsSettingsXmlModel());

            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            if (string.IsNullOrWhiteSpace(existingSettings))
            {
                return await _artifactListSettingsRepository.CreateSettingsAsync(itemId, userId, settings);
            }

            return await _artifactListSettingsRepository.UpdateSettingsAsync(itemId, userId, settings);
        }
    }
}
