using ArtifactStore.Models.Review;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IReviewsRepository
    {
        Task<ReviewSummary> GetReviewSummaryAsync(int containerId, int userId);
        Task<ReviewParticipantsContent> GetReviewParticipantsAsync(int reviewId, int? offset, int? limit, int userId, int? versionId = null, bool? addDrafts = true);
        Task<ArtifactReviewContent> GetReviewArtifactStatusesByParticipant(int artifactId, int reviewId, int? offset, int? limit, int userId, int? versionId = null, bool? addDrafts = true);
        Task<ReviewArtifactsContent> GetReviewArtifactsContentAsync(int reviewId, int userId, int? offset, int? limit, int? versionId = null, bool? addDrafts = true);
        Task<ReviewTableOfContent> GetReviewTableOfContent(int reviewId, int? revisionId, int userId, int? offset, int? limit);
        Task<AddArtifactsResult> AddArtifactsToReview(int reviewId, int userId, AddArtifactsParameter content);
    }
}
