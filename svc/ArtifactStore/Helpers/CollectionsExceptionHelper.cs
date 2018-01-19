using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Helpers
{
    public static class CollectionsExceptionHelper
    {
        public static ResourceNotFoundException NotFoundException(int id)
        {
            var errorMessage = I18NHelper.FormatInvariant("Collection (Id:{0}) is not found.", id);
            return new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static BadRequestException InvalidTypeException(int id)
        {
            var errorMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not a collection.", id);
            return new BadRequestException(errorMessage, ErrorCodes.BadRequest);
        }

        public static AuthorizationException NoAccessException(int id)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.NoAcessForCollection, id);
            throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }
    }
}
