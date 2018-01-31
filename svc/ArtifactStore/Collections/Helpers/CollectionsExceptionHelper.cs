using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Collections.Helpers
{
    public static class CollectionsExceptionHelper
    {
        public static ResourceNotFoundException NotFoundException(int id)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.Collections.NotFound, id);
            return new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static BadRequestException InvalidTypeException(int id)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.Collections.InvalidType, id);
            return new BadRequestException(errorMessage, ErrorCodes.BadRequest);
        }

        public static AuthorizationException NoAccessException(int id)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.Collections.Unauthorized, id);
            return new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        public static AuthorizationException NoEditPermissionException(int id)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.Collections.NoEditPermission, id);
            return new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        public static ConflictException LockedByAnotherUserException(int id, int userId)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.Collections.LockedByAnotherUser, id, userId);
            return new ConflictException(errorMessage, ErrorCodes.Conflict);
        }

        public static ConflictException CollectionMissingArtifactsCouldBeRemoved(int id)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.Collections.CollectionDoesNotHaveArtifactsCouldBeRemoved, id);
            return new ConflictException(errorMessage, ErrorCodes.Conflict);
        }
    }
}
