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
            await AccessReviewAsync(reviewId, userId, revisionId);

            var reviewPackageRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId);
            return new ReviewSettings(reviewPackageRawData);
        }

        public async Task UpdateReviewSettingsAsync(int reviewId, ReviewSettings updatedReviewSettings, int userId)
        {
            if (updatedReviewSettings == null)
            {
                throw new BadRequestException(ErrorMessages.ReviewSettingsAreRequired, ErrorCodes.InvalidParameter);
            }

            var reviewInfo = await AccessReviewAsync(reviewId, userId, int.MaxValue);

            var reviewPackageRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId) ?? new ReviewPackageRawData();

            if (reviewPackageRawData.Status == ReviewPackageStatus.Closed)
            {
                throw new ConflictException(I18NHelper.FormatInvariant(ErrorMessages.ReviewIsClosed, reviewId), ErrorCodes.ReviewClosed);
            }

            UpdateShowOnlyDescription(updatedReviewSettings, reviewPackageRawData);
            UpdateCanMarkAsComplete(reviewId, updatedReviewSettings, reviewPackageRawData);
            UpdateRequireESignature(updatedReviewSettings, reviewPackageRawData);
            await UpdateRequireMeaningOfSignatureAsync(reviewInfo, updatedReviewSettings, reviewPackageRawData);

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewPackageRawData, userId);
        }

        private static void UpdateShowOnlyDescription(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            reviewPackageRawData.ShowOnlyDescription = updatedReviewSettings.ShowOnlyDescription;
        }

        private static void UpdateCanMarkAsComplete(int reviewId, ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            var settingChanged =
                reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed != updatedReviewSettings.CanMarkAsComplete;

            if (settingChanged && reviewPackageRawData.Status != ReviewPackageStatus.Draft)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotDraft, reviewId);
                throw new ConflictException(errorMessage, ErrorCodes.Conflict);
            }

            reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = updatedReviewSettings.CanMarkAsComplete;
        }

        private static void UpdateRequireESignature(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            reviewPackageRawData.IsESignatureEnabled = updatedReviewSettings.RequireESignature;
        }

        private async Task UpdateRequireMeaningOfSignatureAsync(ArtifactBasicDetails reviewInfo, ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            if (!reviewPackageRawData.IsESignatureEnabled)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.RequireESignatureDisabled, reviewInfo.ItemId);
                throw new ConflictException(errorMessage, ErrorCodes.Conflict);
            }

            var settingChanged = reviewPackageRawData.IsMoSEnabled != updatedReviewSettings.RequireMeaningOfSignature;
            if (settingChanged && reviewPackageRawData.Status != ReviewPackageStatus.Draft)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotDraft, reviewInfo.ItemId);
                throw new ConflictException(errorMessage, ErrorCodes.Conflict);
            }

            var projectPermissions = await _permissionsRepository.GetProjectPermissions(reviewInfo.ProjectId);
            if (!projectPermissions.HasFlag(ProjectPermissions.IsMeaningOfSignatureEnabled))
            {
                throw new ConflictException(ErrorMessages.MeaningOfSignatureDisabledInProject, ErrorCodes.Conflict);
            }

            reviewPackageRawData.IsMoSEnabled = updatedReviewSettings.RequireMeaningOfSignature;
        }

        private async Task<ArtifactBasicDetails> AccessReviewAsync(int reviewId, int userId, int revisionId)
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
                    I18NHelper.FormatInvariant(ErrorMessages.ReviewOrRevisionNotFound, reviewId, revisionId) :
                    I18NHelper.FormatInvariant(ErrorMessages.ReviewNotFound, reviewId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            if (artifactInfo.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactReviewPackage)
            {
                throw new BadRequestException(I18NHelper.FormatInvariant(ErrorMessages.ArtifactIsNotReview, reviewId), ErrorCodes.BadRequest);
            }

            if (!await _permissionsRepository.HasReadPermissions(reviewId, userId))
            {
                throw new AuthorizationException(I18NHelper.FormatInvariant(ErrorMessages.CannotAccessReview, reviewId), ErrorCodes.Forbidden);
            }

            return artifactInfo;
        }
    }
}
