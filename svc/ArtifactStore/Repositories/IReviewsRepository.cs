using ArtifactStore.Models.Review;
using ServiceLibrary.Models;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IReviewsRepository
    {
        Task<ReviewSummary> GetReviewSummary(int containerId, int userId);
        Task<QueryResult<ReviewedArtifact>> GetReviewedArtifacts(int reviewId, int userId, Pagination pagination, int revisionId);
        Task<ReviewParticipantsContent> GetReviewParticipantsAsync(int reviewId, int? offset, int? limit, int userId, int? versionId = null, bool? addDrafts = true);
        Task<ArtifactReviewContent> GetReviewArtifactStatusesByParticipant(int artifactId, int reviewId, int? offset, int? limit, int userId, int? versionId = null, bool? addDrafts = true);
        Task<QueryResult<ReviewArtifact>> GetReviewArtifactsContentAsync(int reviewId, int userId, Pagination pagination, int? versionId = null, bool? addDrafts = true);
        Task<ReviewTableOfContent> GetReviewTableOfContent(int reviewId, int revisionId, int userId, Pagination pagination);
        Task<AddArtifactsResult> AddArtifactsToReview(int reviewId, int userId, AddArtifactsParameter content);
    }
}
