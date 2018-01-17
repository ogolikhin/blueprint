using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ServiceLibrary.Models;

namespace ArtifactStore.Services.Workflow
{
    public interface ICollectionsService
    {
        Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, ISet<int> ids);

        Task RunInTransactionAsync(Func<IDbTransaction, Task> action);
    }
}