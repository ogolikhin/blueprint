using ArtifactStore.Models.Review;
using ServiceLibrary.Models;
using System.Collections.Generic;
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
        Task<AddArtifactsResult> AddArtifactsToReviewAsync(int reviewId, int userId, AddArtifactsParameter content);
        Task<AddParticipantsResult> AddParticipantsToReviewAsync(int reviewId, int userId, AddParticipantsParameter content);
        Task AssignApprovalRequiredToArtifacts(int reviewId, int userId, AssignArtifactsApprovalParameter content);
        Task AssignRolesToReviewers(int reviewId, AssignReviewerRolesParameter content, int userId);
        Task<ReviewArtifactIndex> GetReviewArtifactIndexAsync(int reviewId, int revisionId, int artifactId, int userId, bool? addDraft = true);
        Task<ReviewArtifactIndex> GetReviewTableOfContentArtifactIndexAsync(int reviewId, int revisionId, int artifactId, int userId);
        Task<IEnumerable<ReviewArtifactApprovalResult>> UpdateReviewArtifactApprovalAsync(int reviewId, IEnumerable<ReviewArtifactApprovalParameter> reviewArtifactApproval, int userId);
        Task<QueryResult<ParticipantArtifactStats>> GetReviewParticipantArtifactStatsAsync(int reviewId, int participantId, int userId, Pagination pagination);
        Task RemoveArtifactsFromReview(int reviewId, ReviewArtifactsRemovalParams removeParams, int userId);
    }
}
