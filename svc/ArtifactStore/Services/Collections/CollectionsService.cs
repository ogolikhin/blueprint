using System.Threading.Tasks;
using ArtifactStore.Helpers;
using SearchEngineLibrary.Service;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services.Collections
{
    public class CollectionsService : ICollectionsService
    {
        private readonly ICollectionsRepository _collectionsRepository;
        private readonly IArtifactRepository _artifactRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly ISearchEngineService _searchEngineService;

        public CollectionsService() : this(
            new SqlCollectionsRepository(),
            new SqlArtifactRepository(),
            new SqlArtifactPermissionsRepository(),
            new SearchEngineService())
        {
        }

        private CollectionsService(
            ICollectionsRepository collectionsRepository,
            IArtifactRepository artifactRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            ISearchEngineService searchEngineService)
        {
            _collectionsRepository = collectionsRepository;
            _artifactRepository = artifactRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _searchEngineService = searchEngineService;
        }

        private async Task<ArtifactBasicDetails> GetCollectionBasicDetailsAsync(int collectionId, int userId)
        {
            var collection = await _artifactRepository.GetArtifactBasicDetails(collectionId, userId);

            if (collection == null || collection.DraftDeleted || collection.LatestDeleted)
            {
                throw CollectionsExceptionHelper.NotFoundException(collectionId);
            }

            if (collection.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactCollection)
            {
                throw CollectionsExceptionHelper.InvalidTypeException(collectionId);
            }

            if (!await _artifactPermissionsRepository.HasReadPermissions(collectionId, userId))
            {
                throw CollectionsExceptionHelper.NoAccessException(collectionId);
            }

            return collection;
        }

        public async Task<CollectionArtifacts> GetArtifactsInCollectionAsync(
            int collectionId, Pagination pagination, int userId)
        {
            var collection = await GetCollectionBasicDetailsAsync(collectionId, userId);

            var searchArtifactsResult =
                await _searchEngineService.Search(collection.ArtifactId, pagination, ScopeType.Contents, true, userId);

            var artifacts =
                await _collectionsRepository.GetArtifactsWithPropertyValues(userId, searchArtifactsResult.ArtifactIds);
            artifacts.ItemsCount = searchArtifactsResult.Total;

            return artifacts;
        }
    }
}
