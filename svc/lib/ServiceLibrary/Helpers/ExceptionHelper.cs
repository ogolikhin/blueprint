using ServiceLibrary.Exceptions;

namespace ServiceLibrary.Helpers
{
    public class ExceptionHelper
    {
        internal static void ThrowNotFoundException(int projectId, int? artifactId)
        {
            var errorMessage = artifactId == null
                ? I18NHelper.FormatInvariant("The project (Id:{0}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", projectId)
                : I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifactId, projectId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        internal static void ThrowForbiddenException(int projectId, int? artifactId)
        {
            if (artifactId.HasValue)
            {
                ThrowArtifactForbiddenException(artifactId.Value);
            }
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions for Project (Id:{0}).",
                projectId);
            throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        internal static void ThrowArtifactNotFoundException(int artifactId)
        {
            var errorMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) is not found.", artifactId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        internal static void ThrowArtifactForbiddenException(int artifactId)
        {
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).",
                                   artifactId);
            throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }
    }
}
