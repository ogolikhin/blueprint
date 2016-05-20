using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models;

namespace ArtifactStore.Repositories
{
    public interface ISqlArtifactRepository
    {
        Task<List<Artifact>> GetProjectOrGetChildrenAsync(int projectId, int? artifactId, int userId);
    }
}