using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using SearchEngineLibrary.Service;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services.Collections
{
    public class CollectionsService : ICollectionsService
    {
        private readonly ICollectionsRepository _collectionsRepository;
        private readonly ILockArtifactsRepository _lockArtifactsRepository;
        private readonly IItemInfoRepository _itemInfoRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IArtifactRepository _artifactRepository;
        private readonly ISqlHelper _sqlHelper;
        private readonly ISearchEngineService _searchEngineService;

        public CollectionsService() : this(
            new SqlCollectionsRepository(),
            new SqlArtifactRepository(),
            new SqlLockArtifactsRepository(),
            new SqlItemInfoRepository(),
            new SqlArtifactPermissionsRepository(),
            new SqlHelper(),
            new SearchEngineService())
        {
        }

        private CollectionsService(
            ICollectionsRepository collectionsRepository,
            IArtifactRepository artifactRepository,
            ILockArtifactsRepository lockArtifactsRepository,
            IItemInfoRepository itemInfoRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            ISqlHelper sqlHelper,
            ISearchEngineService searchEngineService)
        {
            _collectionsRepository = collectionsRepository;
            _artifactRepository = artifactRepository;
            _lockArtifactsRepository = lockArtifactsRepository;
            _itemInfoRepository = itemInfoRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _sqlHelper = sqlHelper;
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

        public async Task<AddArtifactsResult> AddArtifactsToCollectionAsync(
            int collectionId, ISet<int> artifactIds, int userId)
        {
            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            if (collectionId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(collectionId));
            }

            AddArtifactsResult result = null;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var collection = await _artifactRepository.GetCollectionInfoAsync(userId, collectionId, transaction);

                if (!await _artifactPermissionsRepository.HasEditPermissions(
                    collection.ArtifactId, userId, transaction: transaction))
                {
                    throw ExceptionHelper.CollectionForbiddenException(collection.ArtifactId);
                }

                if (collection.LockedByUserId == null)
                {
                    if (!await _lockArtifactsRepository.LockArtifactAsync(collection.ArtifactId, userId, transaction))
                    {
                        throw ExceptionHelper.ArtifactNotLockedException(collection.ArtifactId, userId);
                    }
                }

                await _collectionsRepository.RemoveDeletedArtifactsFromCollectionAsync(
                    collection.ArtifactId, userId, transaction);

                var artifactsDetails = await _itemInfoRepository.GetItemsDetails(
                    userId, artifactIds, transaction: transaction);
                var validArtifacts = artifactsDetails
                    .Where(i => CanAddArtifactToCollection(i, collection))
                    .ToList();

                var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(
                    validArtifacts.Select(i => i.HolderId), userId, transaction: transaction);
                var artifactsWithReadPermissions = artifactPermissionsDictionary
                    .Where(p => p.Value.HasFlag(RolePermissions.Read))
                    .Select(p => p.Key).ToList();

                result = new AddArtifactsResult
                {
                    AddedCount = await _collectionsRepository.AddArtifactsToCollectionAsync(
                        userId, collection.ArtifactId, artifactsWithReadPermissions, transaction),
                    Total = artifactIds.Count
                };
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);

            return result;
        }

        private static bool CanAddArtifactToCollection(ItemDetails artifact, ArtifactBasicDetails collection)
        {
            return (artifact.PrimitiveItemTypePredefined & (int)ItemTypePredefined.PrimitiveArtifactGroup) != 0 &&
                   (artifact.PrimitiveItemTypePredefined & (int)ItemTypePredefined.BaselineArtifactGroup) == 0 &&
                   (artifact.PrimitiveItemTypePredefined & (int)ItemTypePredefined.CollectionArtifactGroup) == 0 &&
                   artifact.PrimitiveItemTypePredefined != (int)ItemTypePredefined.Project &&
                   artifact.PrimitiveItemTypePredefined != (int)ItemTypePredefined.Baseline &&
                   artifact.VersionProjectId == collection.ProjectId &&
                   (artifact.EndRevision == int.MaxValue || artifact.EndRevision == 1);
        }
    }
}
