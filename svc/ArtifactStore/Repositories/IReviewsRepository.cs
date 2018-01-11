using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using ServiceLibrary.Models;

namespace ArtifactStore.Repositories
{
    public interface IReviewsRepository
    {
        Task<ReviewSummary> GetReviewSummary(int containerId, int userId);
        Task<ReviewSummaryMetrics> GetReviewSummaryMetrics(int containerId, int userId);
        Task<QueryResult<ReviewedArtifact>> GetReviewedArtifacts(int reviewId, int userId, Pagination pagination, int revisionId, ReviewFilterParameters filterParameters = null);
        Task<ReviewParticipantsContent> GetReviewParticipantsAsync(int reviewId, Pagination pagination, int userId, int? versionId = null, bool? addDrafts = true, ReviewFilterParameters filterParameters = null);
        Task<QueryResult<ReviewArtifactDetails>> GetReviewArtifactStatusesByParticipant(int artifactId, int reviewId, Pagination pagination, int userId, int? versionId = null, bool? addDrafts = true);
        Task<QueryResult<ReviewArtifact>> GetReviewArtifactsContentAsync(int reviewId, int userId, Pagination pagination, int? versionId = null, bool? addDrafts = true);
        Task<QueryResult<ReviewTableOfContentItem>> GetReviewTableOfContent(int reviewId, int revisionId, int userId, Pagination pagination);
        Task<AddArtifactsResult> AddArtifactsToReviewAsync(int reviewId, int userId, AddArtifactsParameter content);
        Task<AddParticipantsResult> AddParticipantsToReviewAsync(int reviewId, int userId, AddParticipantsParameter content);
        Task<ReviewArtifactIndex> GetReviewArtifactIndexAsync(int reviewId, int revisionId, int artifactId, int userId, bool? addDraft = true);
        Task<ReviewArtifactIndex> GetReviewTableOfContentArtifactIndexAsync(int reviewId, int revisionId, int artifactId, int userId);
        Task<ReviewArtifactApprovalResult> UpdateReviewArtifactApprovalAsync(int reviewId, ReviewArtifactApprovalParameter reviewArtifactApproval, int userId);
        Task UpdateReviewArtifactsViewedAsync(int reviewId, ReviewArtifactViewedInput viewedInput, int userId);
        Task UpdateReviewerStatusAsync(int reviewId, int revisionId, ReviewerStatusParameter statusParameter, int userId);
        Task<QueryResult<ParticipantArtifactStats>> GetReviewParticipantArtifactStatsAsync(int reviewId, int participantId, int userId, Pagination pagination);
        Task RemoveArtifactsFromReviewAsync(int reviewId, ReviewItemsRemovalParams removeParams, int userId);
        Task RemoveParticipantsFromReviewAsync(int reviewId, ReviewItemsRemovalParams removeParams, int userId);
        Task UpdateReviewPackageRawDataAsync(int reviewId, ReviewPackageRawData reviewPackageRawData, int userId);
        Task<IEnumerable<ReviewInfo>> GetReviewInfo(ISet<int> artifactIds, int userId, bool addDrafts = true, int revisionId = int.MaxValue);
        Task<bool> IsMeaningOfSignatureEnabledAsync(int reviewId, int userId, bool addDrafts);
        Task<Dictionary<int, List<ParticipantMeaningOfSignatureResult>>> GetPossibleMeaningOfSignaturesForParticipantsAsync(int reviewId, int userId, IEnumerable<int> participantIds, bool includeDrafts = true);
        Task<Review> GetReviewAsync(int reviewId, int userId, int revisionId = int.MaxValue, bool? addDraft = true);
        Task<IEnumerable<Review>> GetReviewsAsync(IEnumerable<int> reviewIds, int userId, int revisionId = int.MaxValue, bool? addDraft = true);
        Task<int> UpdateReviewArtifactsAsync(int reviewId, int userId, string xmlArtifacts, IDbTransaction transaction = null, bool addReviewSubArtifactIfNeeded = true);
        Task<ReviewType> GetReviewTypeAsync(int reviewId, int userId, int revisionId = int.MaxValue, bool includeDrafts = true);
    }
}
