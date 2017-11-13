using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Helpers
{
    public static class ReviewsExceptionHelper
    {
        public static ResourceNotFoundException ReviewNotFoundException(int reviewId, int revisionId = int.MaxValue)
        {
            var errorMessage = revisionId != int.MaxValue ?
                I18NHelper.FormatInvariant("Review (Id:{0}) or its revision (#{1}) is not found.", reviewId, revisionId) :
                I18NHelper.FormatInvariant("Review (Id:{0}) is not found.", reviewId);
            return new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static AuthorizationException UserCannotAccessReviewException(int reviewId)
        {
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
            return new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        public static AuthorizationException UserCannotModifyReviewException(int reviewId)
        {
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions to modify the review (Id:{0}).", reviewId);
            return new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        public static ConflictException ReviewClosedException()
        {
            const string errorMessage = "This Review is now closed. No modifications can be made to its artifacts or participants.";
            return new ConflictException(errorMessage, ErrorCodes.ReviewClosed);
        }

        public static ConflictException ReviewExpiredException()
        {
            const string errorMessage = "This Review has expired. No modifications can be made to its artifacts or participants.";
            return new ConflictException(errorMessage, ErrorCodes.ReviewExpired);
        }

        public static ConflictException ReviewActiveFormalException()
        {
            const string errorMessage = "The content of the review cannot be changed because review is active and formal.";
            return new ConflictException(errorMessage, ErrorCodes.ReviewActive);
        }

        public static ConflictException ReviewIsNotFormalException(int reviewId)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotFormal, reviewId);
            return new ConflictException(errorMessage, ErrorCodes.Conflict);
        }

        public static ConflictException ReviewIsNotDraftException(int reviewId)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotDraft, reviewId);
            return new ConflictException(errorMessage, ErrorCodes.Conflict);
        }

        public static ConflictException RequireESignatureDisabledException(int reviewId)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.RequireESignatureDisabled, reviewId);
            return new ConflictException(errorMessage, ErrorCodes.Conflict);
        }

        public static ConflictException MeaningOfSignatureIsDisabledInProjectException()
        {
            return new ConflictException(ErrorMessages.MeaningOfSignatureDisabledInProject, ErrorCodes.Conflict);
        }

        public static BadRequestException BaselineNotSealedException()
        {
            var errorMessage = I18NHelper.FormatInvariant("The baseline could not be added to the review because it is not sealed.");
            return new BadRequestException(errorMessage, ErrorCodes.BaselineIsNotSealed);
        }
    }
}
