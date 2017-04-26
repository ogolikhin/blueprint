﻿using ArtifactStore.Models.Review;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IReviewsRepository
    {
        Task<ReviewContainer> GetReviewContainerAsync(int containerId, int userId);

        Task<ReviewContent> GetContentAsync(int reviewId, int userId, int? offset, int? limit, int? versionId = null, bool? addDrafts = true);

        Task<Reviewer> GetReviewer(int reviewId, int userId);
    }
}