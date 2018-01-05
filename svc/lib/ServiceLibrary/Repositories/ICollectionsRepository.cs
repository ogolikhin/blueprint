using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public interface ICollectionsRepository
    {
        Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, OperationScope scope);
    }
}
