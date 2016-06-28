using ArtifactStore.Models;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IRelationshipsRepository
    {
        Task<RelationshipResultSet> GetRelationships(int artifactId, int? subArtifactId = null, bool addDrafts = true);
    }
}