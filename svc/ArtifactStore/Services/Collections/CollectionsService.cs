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
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly ISearchEngineService _searchEngineService;

        public CollectionsService() : this(
            new SqlCollectionsRepository(),
            new SqlArtifactPermissionsRepository(),
            new SearchEngineService())
        {
        }

        public CollectionsService(
            ICollectionsRepository collectionsRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            ISearchEngineService searchEngineService)
        {
            _collectionsRepository = collectionsRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _searchEngineService = searchEngineService;
        }

        public async Task<CollectionArtifacts> GetArtifactsInCollectionAsync(
            int collectionId, Pagination pagination, int userId)
        {
            if (!await _artifactPermissionsRepository.HasReadPermissions(collectionId, userId))
            {
                throw CollectionsExceptionHelper.NoAccessException(collectionId);
            }

            var searchArtifactsResult = await _searchEngineService.Search(collectionId, pagination, ScopeType.Contents, true, userId);

            var artifacts = await _collectionsRepository.GetArtifactsWithPropertyValues(userId, searchArtifactsResult.ArtifactIds);
            artifacts.ItemsCount = searchArtifactsResult.Total;

            return artifacts;
        }
    }
}
