using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;

namespace ArtifactStore.Services.Collections
{
    public interface ICollectionsService
    {
        Task<CollectionArtifacts> GetArtifactsInCollectionAsync(int collectionId, Pagination pagination, int userId);

        Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, ISet<int> ids);

        Task RunInTransactionAsync(Func<IDbTransaction, Task> action);
    }
}
