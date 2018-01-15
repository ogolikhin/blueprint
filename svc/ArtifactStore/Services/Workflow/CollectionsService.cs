using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Services.Workflow
{
    public class CollectionsService : ICollectionsService
    {
        private readonly ICollectionsRepository _collectionsRepository;
        private readonly ILockArtifactsRepository _lockArtifactsRepository;
        private readonly IItemInfoRepository _itemInfoRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IArtifactRepository _artifactRepository;

        public CollectionsService(ICollectionsRepository collectionsRepository, IArtifactRepository artifactRepository,
                                  ILockArtifactsRepository lockArtifactsRepository, IItemInfoRepository itemInfoRepository,
                                  IArtifactPermissionsRepository artifactPermissionsRepository)
        {
            _collectionsRepository = collectionsRepository;
            _artifactRepository = artifactRepository;
            _lockArtifactsRepository = lockArtifactsRepository;
            _itemInfoRepository = itemInfoRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
        }

        public async Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, OperationScope scope)
        {
            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            if (collectionId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(collectionId));
            }

            AssignArtifactsResult assignResult = null;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var collection = await _collectionsRepository.GetCollectionInfoAsync(userId, collectionId);

                if (!(await _artifactPermissionsRepository.HasEditPermissions(collection.ArtifactId, userId)))
                {
                    throw ExceptionHelper.CollectionForbiddenException(collection.ArtifactId);
                }

                if (collection.LockedByUserId == null)
                {
                    if (!await _lockArtifactsRepository.LockArtifactAsync(collection.ArtifactId, userId))
                    {
                        throw ExceptionHelper.ArtifactNotLockedException(collection.ArtifactId, userId);
                    }
                }

                await _collectionsRepository.RemoveDeletedArtifactsFromCollection(collection.ArtifactId, userId);

                var artifactDetails = await _itemInfoRepository.GetItemsDetails(userId, scope.Ids.ToList());
                var validArtifacts =
                    artifactDetails.Where(i => ((i.PrimitiveItemTypePredefined & (int)ItemTypePredefined.PrimitiveArtifactGroup) != 0) &&
                                                ((i.PrimitiveItemTypePredefined & (int)ItemTypePredefined.BaselineArtifactGroup) == 0) &&
                                                ((i.PrimitiveItemTypePredefined & (int)ItemTypePredefined.CollectionArtifactGroup) == 0) &&
                                                (i.PrimitiveItemTypePredefined != (int)ItemTypePredefined.Project) &&
                                                (i.PrimitiveItemTypePredefined != (int)ItemTypePredefined.Baseline) &&
                                                i.VersionProjectId == collection.ProjectId).ToList();

                var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(validArtifacts.Select(i => i.HolderId), userId);
                var artifactsWithReadPermissions = artifactPermissionsDictionary
                    .Where(p => p.Value.HasFlag(RolePermissions.Read))
                    .Select(p => p.Key).ToList();

                assignResult = await _collectionsRepository.AddArtifactsToCollectionAsync(userId, collection.ArtifactId, artifactsWithReadPermissions);
            };

            await _collectionsRepository.RunInTransactionAsync(action);

            return assignResult;
        }
    }
}