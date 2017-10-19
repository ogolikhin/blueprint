using System;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
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
                throw new ResourceNotFoundException(GetReviewNotFoundErrorMessage(reviewId, revisionId), ErrorCodes.ResourceNotFound);
            }

            if (artifactInfo.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactReviewPackage)
            {
                throw new BadRequestException(GetInvalidReviewIdErrorMessage(reviewId), ErrorCodes.InvalidParameter);
            }

            if (!await _permissionsRepository.HasReadPermissions(reviewId, userId))
            {
                throw new AuthorizationException(GetReviewInaccessibleErrorMessage(reviewId), ErrorCodes.Forbidden);
            }

            var reviewXml = await _reviewsRepository.GetReviewXmlAsync(reviewId, userId);
            if (!reviewXml.ReviewExists)
            {
                throw new ResourceNotFoundException(GetReviewNotFoundErrorMessage(reviewId, revisionId), ErrorCodes.ResourceNotFound);
            }

            return new ReviewSettings
            {
                EndDate = ReviewRawDataHelper.ExtractReviewEndDate(reviewXml.XmlString),
                ShowOnlyDescription = ReviewRawDataHelper.ExtractBooleanProperty("ShowOnlyDescription", reviewXml.XmlString) ?? false,
                CanMarkAsComplete = ReviewRawDataHelper.ExtractBooleanProperty("IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed", reviewXml.XmlString) ?? false,
                RequireESignature = ReviewRawDataHelper.ExtractBooleanProperty("IsESignatureEnabled", reviewXml.XmlString) ?? false,
                RequireMeaningOfSignature = ReviewRawDataHelper.ExtractBooleanProperty("IsMoSEnabled", reviewXml.XmlString) ?? false
            };
        }

        private static string GetInvalidReviewIdErrorMessage(int reviewId)
        {
            return I18NHelper.FormatInvariant("Artifact (Id:{0}) is not a review.", reviewId);
        }

        private static string GetReviewInaccessibleErrorMessage(int reviewId)
        {
            return I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
        }

        private static string GetReviewNotFoundErrorMessage(int reviewId, int revisionId)
        {
            return revisionId != int.MaxValue ?
                I18NHelper.FormatInvariant("Review (Id:{0}) or its revision (#{1}) is not found.", reviewId, revisionId) :
                I18NHelper.FormatInvariant("Review (Id:{0}) is not found.", reviewId);
        }
    }
}
