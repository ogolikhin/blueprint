using ArtifactStore.Models;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IRelationshipsRepository
    {
        Task<RelationshipResultSet> GetRelationships(
            int artifactId, 
            int userId, 
            int? subArtifactId = null, 
            bool addDrafts = true, 
            bool allLinks = false,
            int? versionId = null,
            int? baselineId = null);
        Task<RelationshipExtendedInfo> GetRelationshipExtendedInfo(int artifactId, int userId, int? subArtifactId = null, bool isDeleted = false);
        Task<ReviewRelationshipsResultSet> GetReviewRelationships(int artifactId, int userId, bool addDrafts = true, int? versionId = null);
    }
}