using ServiceLibrary.Exceptions;

namespace ServiceLibrary.Helpers
{
    public class ExceptionHelper
    {
        public static void ThrowNotFoundException(int projectId, int? artifactId)
        {
            var errorMessage = artifactId == null
                ? I18NHelper.FormatInvariant("The project (Id:{0}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", projectId)
                : I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifactId, projectId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static void ThrowForbiddenException(int projectId, int? artifactId)
        {
            if (artifactId.HasValue)
            {
                ThrowArtifactForbiddenException(artifactId.Value);
            }
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions for Project (Id:{0}).",
                projectId);
            throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        public static void ThrowArtifactNotFoundException(int artifactId)
        {
            var errorMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) is not found.", artifactId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static void ThrowStateNotFoundException(int artifactId)
        {
            var errorMessage = I18NHelper.FormatInvariant("No state association could be found for Artifact (Id:{0}).", artifactId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static void ThrowArtifactForbiddenException(int artifactId)
        {
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).",
                                   artifactId);
            throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        public static void ThrowArtifactNotLockedException(int artifactId, int userId)
        {
            var errorMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) is not locked by user (Id:{1}).",
                                   artifactId, userId);
            throw new BadRequestException(errorMessage, ErrorCodes.LockedByOtherUser);
        }

        public static void ThrowArtifactDoesNotSupportOperation(int artifactId)
        {
            var errorMessage = I18NHelper.FormatInvariant("Operation cannot be invoked on Artifact with (Id:{0}).",
                                   artifactId);
            throw new BadRequestException(errorMessage, ErrorCodes.BadRequest);
        }
    }
}
