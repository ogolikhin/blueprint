using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public interface ICollectionsRepository
    {
        Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, OperationScope scope);
        Task<ArtifactBasicDetails> GetCollectionInfoAsync(int userId, int collectionId);

        Task RemoveDeletedArtifactsFromCollection(int collectionId, int userId);

        Task RunInTransactionAsync(Func<IDbTransaction, Task> action);
    }
}
