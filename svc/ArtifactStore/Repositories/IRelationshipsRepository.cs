using ArtifactStore.Models;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IRelationshipsRepository
    {
        Task<RelationshipResultSet> GetRelationships(int artifactId, int userId, int? subArtifactId = null, bool addDrafts = true, int? versionId = null);
        Task<RelationshipExtendedInfo> GetRelationshipExtendedInfo(int artifactId, int userId, int? subArtifactId = null, bool isDeleted = false);
        Task<SqlRelationshipsRepository.ReviewRelationshipsResultSet> GetReviewRelationships(int artifactId, int userId, int? subArtifactId = null, bool addDrafts = true, int? versionId = null);
    }
}