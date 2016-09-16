using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models;

namespace ArtifactStore.Repositories
{
    public interface ISqlArtifactRepository
    {
        Task<List<Artifact>> GetProjectOrArtifactChildrenAsync(int projectId, int? artifactId, int userId);

        Task<List<Artifact>> GetExpandedTreeToArtifactAsync(int projectId, int expandedToArtifactId, bool includeChildren, int userId);
    }
}