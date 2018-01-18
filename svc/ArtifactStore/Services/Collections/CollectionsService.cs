using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Collection;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services.Collections
{
    public class CollectionsService : ICollectionsService
    {
        private readonly ICollectionsRepository _collectionsRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;

        public CollectionsService(ICollectionsRepository collectionsRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository)
        {
            _collectionsRepository = collectionsRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
        }

        public async Task<CollectionArtifacts> GetArtifactsWithPropertyValues(int userId, int collectionId,
            IEnumerable<int> artifactIds)
        {
            if (!await _artifactPermissionsRepository.HasReadPermissions(collectionId, userId))
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.NoAcessForCollection, collectionId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }

            return await _collectionsRepository.GetArtifactsWithPropertyValues(userId, artifactIds);
        }
    }
}