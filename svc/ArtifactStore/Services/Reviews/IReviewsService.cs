using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using ServiceLibrary.Models;

namespace ArtifactStore.Services.Reviews
{
    public interface IReviewsService
    {
        Task<ReviewSettings> GetReviewSettingsAsync(int reviewId, int userId, int? versionId = null);

        Task UpdateReviewSettingsAsync(int reviewId, ReviewSettings updatedReviewSettings, int userId);

        Task UpdateMeaningOfSignaturesAsync(int reviewId, IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters, int userId);

        Task<IEnumerable<DropdownItem>> AssignRoleToParticipantAsync(int reviewId, AssignParticipantRoleParameter content, int userId);

        Task<ReviewChangeItemsStatusResult> AssignApprovalRequiredToArtifactsAsync(int reviewId, AssignArtifactsApprovalParameter content, int userId);
    }
}
