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
        private readonly IArtifactRepository _artifactRepository;

        public CollectionsService(ICollectionsRepository collectionsRepository, IArtifactRepository artifactRepository,
                                  ILockArtifactsRepository lockArtifactsRepository)
        {
            _collectionsRepository = collectionsRepository;
            _artifactRepository = artifactRepository;
            _lockArtifactsRepository = lockArtifactsRepository;
        }

        public async Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, OperationScope scope)
        {
            AssignArtifactsResult assignResult = null;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var collection = await _collectionsRepository.GetCollectionInfoAsync(userId, collectionId);

                if (collection.LockedByUserId == null)
                {
                    if (!await _lockArtifactsRepository.LockArtifactAsync(collection.ArtifactId, userId))
                    {
                        throw ExceptionHelper.ArtifactNotLockedException(collection.ArtifactId, userId);
                    }
                }



                assignResult = await _collectionsRepository.AddArtifactsToCollectionAsync(userId, collectionId, scope);
            };

            await _collectionsRepository.RunInTransactionAsync(action);

            return assignResult;
        }
    }
}