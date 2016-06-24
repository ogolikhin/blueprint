using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models;

namespace ArtifactStore.Repositories
{
    public interface ISqlArtifactRepository
    {
        Task<ArtifactDetailsResultSet> GeArtifactAsync(int artifactId, int userId);
        Task<List<Artifact>> GetProjectOrArtifactChildrenAsync(int projectId, int? artifactId, int userId);

    }
}