using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ArtifactListSetting;
using ServiceLibrary.Models.Collection;
using System.Linq;

namespace ServiceLibrary.Services.ArtifactListSetting
{
    public class ArtifactListSettingsService : IArtifactListSettingsService
    {
        private readonly IArtifactListSettingsRepository _artifactListSettingsRepository;
        private readonly IArtifactRepository _sqlArtifactRepository;

        public ArtifactListSettingsService() : this(new ArtifactListSettingsRepository(), new SqlArtifactRepository())
        {

        }

        internal ArtifactListSettingsService(IArtifactListSettingsRepository artifactListSettingsRepository, IArtifactRepository sqlArtifactRepository)
        {
            _artifactListSettingsRepository = artifactListSettingsRepository;
            _sqlArtifactRepository = sqlArtifactRepository;
        }

        public async Task<int> SaveArtifactColumnsSettings(int itemId, int userId, ArtifactListSettings artifactListSettings)
        {
            var artifactBasicDetails = await _sqlArtifactRepository.GetArtifactBasicDetails(itemId, userId);

            if (artifactBasicDetails == null)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotFound, itemId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            var isExistPrimaryKeyInTable = true; // TODO There must be call of method checking of existing PK (itemId, userId) in ArtifactListSettings table
            var settings = SerializationHelper.ToXml(ArtifactListSettingsXml.ConvertFromJsonModel(artifactListSettings));

            if (isExistPrimaryKeyInTable)
            {
                return await _artifactListSettingsRepository.UpdateArtifactListSettingsAsync(itemId, userId, settings);
            }
            else
            {
                return await _artifactListSettingsRepository.CreateArtifactListSettingsAsync(itemId, userId, settings);
            }
        }
    }
}
