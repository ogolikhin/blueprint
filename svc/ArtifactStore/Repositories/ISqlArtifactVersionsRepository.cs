using ArtifactStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ArtifactStore.Repositories
{
    public interface ISqlArtifactVersionsRepository
    {
        Task<ArtifactHistoryResultSet> GetArtifactVersions(int artifactId, int limit, int offset, int? userId, bool asc);
    }
}