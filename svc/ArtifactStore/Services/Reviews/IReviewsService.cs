using System.Threading.Tasks;
using ArtifactStore.Models.Review;

namespace ArtifactStore.Services.Reviews
{
    public interface IReviewsService
    {
        Task<ReviewSettings> GetReviewSettingsAsync(int reviewId, int userId, int revisionId = int.MaxValue);

        Task UpdateReviewSettingsAsync(int reviewId, ReviewSettings updatedReviewSettings, int userId);
    }
}
