using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ServiceLibrary.Models;

namespace ArtifactStore.Services.Workflow
{
    public interface ICollectionsService
    {
        Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, OperationScope scope);
    }
}