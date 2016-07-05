using ArtifactStore.Models;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IRelationshipsRepository
    {
        Task<RelationshipResultSet> GetRelationships(int itemId, int userId, bool addDrafts = true);
        Task<RelationshipExtendedInfo> GetRelationshipExtendedInfo(int artifactId, int userId, bool addDraft = true, int revisionId = int.MaxValue);
    }
}