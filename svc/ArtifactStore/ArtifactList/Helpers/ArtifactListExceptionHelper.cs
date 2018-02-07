using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ArtifactStore.ArtifactList.Helpers
{
    public static class ArtifactListExceptionHelper
    {
        public static BadRequestException DuplicateColumnException(string columnName)
        {
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactList.AddColumnColumnExists, columnName);
            return new BadRequestException(errorMessage, ErrorCodes.BadRequest);
        }

        public static BadRequestException ColumnCapacityExceededException(string columnName, int maxCapacity)
        {
            var errorMessage = I18NHelper.FormatInvariant(
                ErrorMessages.ArtifactList.ColumnCapacityExceeded, columnName, maxCapacity);
            return new BadRequestException(errorMessage, ErrorCodes.BadRequest);
        }
    }
}
