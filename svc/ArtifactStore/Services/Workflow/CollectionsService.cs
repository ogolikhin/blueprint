using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services.Workflow
{
    public class CollectionsService : ICollectionsService
    {
        private readonly ICollectionsRepository _collectionsRepository;

        public CollectionsService(ICollectionsRepository collectionsRepository)
        {
            _collectionsRepository = collectionsRepository;
        }

        public async Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, OperationScope scope)
        {
            return await _collectionsRepository.AddArtifactsToCollectionAsync(userId, collectionId, scope);
        }
    }
}