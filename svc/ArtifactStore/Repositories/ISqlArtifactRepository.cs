using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models;

namespace ArtifactStore.Repositories
{
    public interface ISqlArtifactRepository
    {
        Task<List<Artifact>> GetProjectChildrenAsync(int projectId);

        Task<List<Artifact>> GetArtifactChildrenAsync(int projectId, int artifactId);
    }
}