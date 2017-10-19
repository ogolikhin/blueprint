using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using ServiceLibrary.Models;

namespace ArtifactStore.Repositories
{
    public interface IReviewsRepository
    {
        Task<ReviewSummary> GetReviewSummary(int containerId, int userId);
        Task<ReviewSummaryMetrics> GetReviewSummaryMetrics(int containerId, int userId);
        Task<QueryResult<ReviewedArtifact>> GetReviewedArtifacts(int reviewId, int userId, Pagination pagination, int revisionId);
        Task<ReviewParticipantsContent> GetReviewParticipantsAsync(int reviewId, Pagination pagination, int userId, int? versionId = null, bool? addDrafts = true);
        Task<QueryResult<ReviewArtifactDetails>> GetReviewArtifactStatusesByParticipant(int artifactId, int reviewId, Pagination pagination, int userId, int? versionId = null, bool? addDrafts = true);
        Task<QueryResult<ReviewArtifact>> GetReviewArtifactsContentAsync(int reviewId, int userId, Pagination pagination, int? versionId = null, bool? addDrafts = true);
        Task<QueryResult<ReviewTableOfContentItem>> GetReviewTableOfContent(int reviewId, int revisionId, int userId, Pagination pagination);
        Task<AddArtifactsResult> AddArtifactsToReviewAsync(int reviewId, int userId, AddArtifactsParameter content);
        Task<AddParticipantsResult> AddParticipantsToReviewAsync(int reviewId, int userId, AddParticipantsParameter content);
        Task AssignApprovalRequiredToArtifacts(int reviewId, int userId, AssignArtifactsApprovalParameter content);
        Task AssignRolesToReviewers(int reviewId, AssignReviewerRolesParameter content, int userId);
        Task<ReviewArtifactIndex> GetReviewArtifactIndexAsync(int reviewId, int revisionId, int artifactId, int userId, bool? addDraft = true);
        Task<ReviewArtifactIndex> GetReviewTableOfContentArtifactIndexAsync(int reviewId, int revisionId, int artifactId, int userId);
        Task<ReviewArtifactApprovalResult> UpdateReviewArtifactApprovalAsync(int reviewId, ReviewArtifactApprovalParameter reviewArtifactApproval, int userId);
        Task UpdateReviewArtifactsViewedAsync(int reviewId, ReviewArtifactViewedInput viewedInput, int userId);
        Task UpdateReviewerStatusAsync(int reviewId, int revisionId, ReviewStatus status, int userId);
        Task<QueryResult<ParticipantArtifactStats>> GetReviewParticipantArtifactStatsAsync(int reviewId, int participantId, int userId, Pagination pagination);
        Task RemoveArtifactsFromReviewAsync(int reviewId, ReviewItemsRemovalParams removeParams, int userId);
        Task RemoveParticipantsFromReviewAsync(int reviewId, ReviewItemsRemovalParams removeParams, int userId);
        Task<ReviewSettings> GetReviewSettingsAsync(int reviewId, int userId, int revisionId = int.MaxValue);
    }
}
