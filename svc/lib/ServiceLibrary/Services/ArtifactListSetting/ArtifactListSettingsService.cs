using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ArtifactListSetting;
using ServiceLibrary.Models.Collection;

namespace ServiceLibrary.Services.ArtifactListSetting
{
    public class ArtifactListSettingsService : IArtifactListSettingsService
    {
        private readonly IArtifactListSettingsRepository _artifactListSettingsRepository;
        private readonly IArtifactRepository _sqlArtifactRepository;

        public ArtifactListSettingsService() : this(
            new ArtifactListSettingsRepository(),
            new SqlArtifactRepository())
        {
        }

        internal ArtifactListSettingsService(
            IArtifactListSettingsRepository artifactListSettingsRepository,
            IArtifactRepository sqlArtifactRepository)
        {
            _artifactListSettingsRepository = artifactListSettingsRepository;
            _sqlArtifactRepository = sqlArtifactRepository;
        }

        public async Task<int> SaveArtifactColumnsSettings(
            int itemId, int userId, ArtifactListSettings artifactListSettings)
        {
            var artifactBasicDetails = await _sqlArtifactRepository.GetArtifactBasicDetails(itemId, userId);

            if (artifactBasicDetails == null)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotFound, itemId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            var settings =
                SerializationHelper.ToXml(ArtifactListSettingsXml.ConvertFromJsonModel(artifactListSettings));
            var existingSettings = await _artifactListSettingsRepository.GetSettingsAsync(itemId, userId);

            if (string.IsNullOrWhiteSpace(existingSettings))
            {
                return await _artifactListSettingsRepository.UpdateSettingsAsync(itemId, userId, settings);
            }

            return await _artifactListSettingsRepository.CreateSettingsAsync(itemId, userId, settings);
        }
    }
}
