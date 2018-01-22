using ServiceLibrary.Exceptions;

namespace ServiceLibrary.Helpers
{
    public static class ExceptionHelper
    {
        public static ResourceNotFoundException NotFoundException(int projectId, int? artifactId)
        {
            var errorMessage = artifactId == null
                ? I18NHelper.FormatInvariant("The project (Id:{0}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", projectId)
                : I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifactId, projectId);
            return new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static AuthorizationException ForbiddenException(int projectId, int? artifactId)
        {
            if (artifactId.HasValue)
            {
                return ArtifactForbiddenException(artifactId.Value);
            }

            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions for Project (Id:{0}).", projectId);
            return new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        public static ResourceNotFoundException ArtifactNotFoundException(int artifactId)
        {
            var errorMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) is not found.", artifactId);
            return new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static AuthorizationException ArtifactForbiddenException(int artifactId)
        {
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifactId);
            return new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        public static ConflictException ArtifactNotLockedException(int artifactId, int userId)
        {
            var errorMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) is not locked by user (Id:{1}).", artifactId, userId);
            return new ConflictException(errorMessage, ErrorCodes.LockedByOtherUser);
        }

        public static BadRequestException ArtifactDoesNotSupportOperation(int artifactId)
        {
            var errorMessage = I18NHelper.FormatInvariant("Operation cannot be invoked on Artifact with (Id:{0}).", artifactId);
            return new BadRequestException(errorMessage, ErrorCodes.BadRequest);
        }
    }
}
