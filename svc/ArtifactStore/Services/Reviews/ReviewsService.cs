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
        private const string ReviewSettingsMissingMessage = "The updated review settings are missing.";
        private const string ReviewNotFoundMessage = "Review (Id:{0}) is not found.";
        private const string ReviewOrRevisionNotFoundMessage = "Review (Id:{0}) or its revision (#{1}) is not found.";
        private const string ArtifactIsNotReviewMessage = "Artifact (Id:{0}) is not a review.";
        private const string NoPermissionsMessage = "User does not have permissions to access the review (Id:{0}).";
        private const string ReviewIsClosedMessage = "This Review is now closed. No modifications can be made to its artifacts or participants.";
        private const string ReviewIsNotDraftMessage = "This action cannot be completed. The Review is not a draft.";
        private const string RequireESignatureDisabledMessage = "Cannot enable meaning of signature if electornic signatures are not enabled for this Review.";

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

        public async Task UpdateReviewSettingsAsync(int reviewId, ReviewSettings updatedReviewSettings, int userId)
        {
            await ValidateReviewAccess(reviewId, userId, int.MaxValue);

            if (updatedReviewSettings == null)
            {
                throw new BadRequestException(ReviewSettingsMissingMessage, ErrorCodes.InvalidParameter);
            }

            var reviewPackageRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId) ?? new ReviewPackageRawData();

            if (reviewPackageRawData.Status == ReviewPackageStatus.Closed)
            {
                throw new ConflictException(ReviewIsClosedMessage, ErrorCodes.ReviewClosed);
            }

            UpdateShowOnlyDescription(updatedReviewSettings, reviewPackageRawData);
            UpdateCanMarkAsComplete(updatedReviewSettings, reviewPackageRawData);
            UpdateRequireESignature(updatedReviewSettings, reviewPackageRawData);
            UpdateRequireMeaningOfSignature(updatedReviewSettings, reviewPackageRawData);

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewPackageRawData, userId);
        }

        private static void UpdateShowOnlyDescription(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            reviewPackageRawData.ShowOnlyDescription = updatedReviewSettings.ShowOnlyDescription;
        }

        private static void UpdateCanMarkAsComplete(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            var canMarkAsCompleteChanged =
                            reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed =
                                updatedReviewSettings.CanMarkAsComplete;

            if (canMarkAsCompleteChanged && reviewPackageRawData.Status != ReviewPackageStatus.Draft)
            {
                throw new ConflictException(ReviewIsNotDraftMessage, ErrorCodes.Conflict);
            }

            reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = updatedReviewSettings.CanMarkAsComplete;
        }

        private static void UpdateRequireESignature(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            reviewPackageRawData.IsESignatureEnabled = updatedReviewSettings.RequireESignature;
        }

        private static void UpdateRequireMeaningOfSignature(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            if (!reviewPackageRawData.IsESignatureEnabled)
            {
                throw new ConflictException(RequireESignatureDisabledMessage, ErrorCodes.Conflict);
            }

            reviewPackageRawData.IsMoSEnabled = updatedReviewSettings.RequireMeaningOfSignature;
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
                    I18NHelper.FormatInvariant(ReviewOrRevisionNotFoundMessage, reviewId, revisionId) :
                    I18NHelper.FormatInvariant(ReviewNotFoundMessage, reviewId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            if (artifactInfo.PrimitiveItemTypePredefined != (int) ItemTypePredefined.ArtifactReviewPackage)
            {
                throw new BadRequestException(I18NHelper.FormatInvariant(ArtifactIsNotReviewMessage, reviewId), ErrorCodes.InvalidParameter);
            }

            if (!await _permissionsRepository.HasReadPermissions(reviewId, userId))
            {
                throw new AuthorizationException(I18NHelper.FormatInvariant(NoPermissionsMessage, reviewId), ErrorCodes.Forbidden);
            }
        }
    }
}
