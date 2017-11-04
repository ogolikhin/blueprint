using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using ServiceLibrary.Models;

namespace ArtifactStore.Services.Reviews
{
    public interface IReviewsService
    {
        Task<ReviewSettings> GetReviewSettingsAsync(int reviewId, int userId, int revisionId = int.MaxValue);

        Task UpdateReviewSettingsAsync(int reviewId, ReviewSettings updatedReviewSettings, int userId);

        Task UpdateMeaningOfSignaturesAsync(int reviewId, int userId, IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters);

        Task<IEnumerable<DropdownItem>> AssignRoleToParticipantAsync(int reviewId, AssignParticipantRoleParameter content, int userId);
    }
}
