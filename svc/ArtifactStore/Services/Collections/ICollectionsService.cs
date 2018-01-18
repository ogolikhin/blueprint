using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Models.Collection;

namespace ArtifactStore.Services.Collections
{
    public interface ICollectionsService
    {
        Task<CollectionArtifacts> GetArtifactsWithPropertyValues(int userId, int collectionId,
            IEnumerable<int> artifactIds);
    }
}