using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;

namespace ServiceLibrary.Repositories
{
    public interface ICollectionsRepository
    {
        Task<ArtifactsOfCollection> GetArtifactsOfCollectionAsync(int userId, IEnumerable<int> artifactIds);
    }
}
