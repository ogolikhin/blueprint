using System;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using ArtifactStore.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services.Reviews
{
    public class ReviewsService : IReviewsService
    {
        private readonly IReviewsRepository _reviewsRepository;
        private readonly IArtifactRepository _artifactRepository;
        private readonly IArtifactPermissionsRepository _permissionsRepository;

        public ReviewsService() : this(new SqlReviewsRepository(), new SqlArtifactRepository(), new SqlArtifactPermissionsRepository())
        {
        }

        public ReviewsService(IReviewsRepository reviewsRepository, IArtifactRepository artifactRepository, IArtifactPermissionsRepository permissionsRepository)
        {
            _reviewsRepository = reviewsRepository;
            _artifactRepository = artifactRepository;
            _permissionsRepository = permissionsRepository;
        }

        public async Task<ReviewSettings> GetReviewSettingsAsync(int reviewId, int userId, int revisionId = int.MaxValue)
        {
            await ValidateReviewAccess(reviewId, userId, revisionId);

            var reviewPackageRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId);
            return new ReviewSettings(reviewPackageRawData);
        }

        private async Task ValidateReviewAccess(int reviewId, int userId, int revisionId)
        {
            if (reviewId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reviewId));
            }

            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            var artifactInfo = await _artifactRepository.GetArtifactBasicDetails(reviewId, userId);
            if (artifactInfo == null)
            {
                var errorMessage = revisionId != int.MaxValue ?
                    I18NHelper.FormatInvariant("Review (Id:{0}) or its revision (#{1}) is not found.", reviewId, revisionId) :
                    I18NHelper.FormatInvariant("Review (Id:{0}) is not found.", reviewId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            if (artifactInfo.PrimitiveItemTypePredefined != (int) ItemTypePredefined.ArtifactReviewPackage)
            {
                var errorMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) is not a review.", reviewId);
                throw new BadRequestException(errorMessage, ErrorCodes.InvalidParameter);
            }

            if (!await _permissionsRepository.HasReadPermissions(reviewId, userId))
            {
                var errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
                throw new AuthorizationException(errorMessage, ErrorCodes.Forbidden);
            }
        }
    }
}
