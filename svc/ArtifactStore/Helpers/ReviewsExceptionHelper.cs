using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Helpers
{
    public static class ReviewsExceptionHelper
    {
        public static ResourceNotFoundException ReviewNotFoundException(int reviewId, int? revisionId = null)
        {
            var errorMessage = revisionId.HasValue ?
                I18NHelper.FormatInvariant("Review (Id:{0}) or its revision (#{1}) is not found.", reviewId, revisionId) :
                I18NHelper.FormatInvariant("Review (Id:{0}) is not found.", reviewId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static AuthorizationException UserCannotAccessReviewException(int reviewId)
        {
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
            return new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        public static ConflictException ReviewClosedException()
        {
            const string errorMessage = "This Review is now closed. No modifications can be made to its artifacts or participants.";
            return new ConflictException(errorMessage, ErrorCodes.ReviewClosed);
        }

        public static ConflictException ReviewExpiredException()
        {
            var errorMessage = "This Review has expired. No modifications can be made to its artifacts or participants.";
            return new ConflictException(errorMessage, ErrorCodes.ReviewExpired);
        }
    }
}
