using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace ServiceLibrary.Services
{
    public class SqlCollectionsService : ICollectionsService
    {
        private readonly ICollectionsRepository _collectionsRepository;
        private readonly IArtifactRepository _sqlArtifactRepository;

        public SqlCollectionsService() : this(new SqlCollectionsRepository(), new SqlArtifactRepository())
        {

        }

        internal SqlCollectionsService(ICollectionsRepository collectionsRepository, IArtifactRepository sqlArtifactRepository)
        {
            _collectionsRepository = collectionsRepository;
            _sqlArtifactRepository = sqlArtifactRepository;
        }

        public async Task<int> SaveArtifactColumnsSettings(int itemId, int userId, string settings)
        {
            var artifactBasicDetails = await _sqlArtifactRepository.GetArtifactBasicDetails(itemId, userId);

            if (artifactBasicDetails == null)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotFound, itemId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            var isExistPrimaryKeyInTable = true; // TODO There must be call of method checking of existing PK (itemId, userId) in ArtifactListSettings table

            if (isExistPrimaryKeyInTable)
            {
                return await _collectionsRepository.UpdateArtifactListSettingsAsync(itemId, userId, settings);
            }
            else
            {
                return await _collectionsRepository.CreateArtifactListSettingsAsync(itemId, userId, settings);
            }
        }
    }
}
