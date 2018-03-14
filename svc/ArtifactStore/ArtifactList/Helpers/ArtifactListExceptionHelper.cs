using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.ArtifactList.Models;
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

        public static BadRequestException InvalidColumnsException(IReadOnlyList<ProfileColumn> invalidColumns)
        {
            if (invalidColumns.IsEmpty())
            {
                throw new ArgumentException(nameof(invalidColumns));
            }

            const int maxPropertiesToShow = 3;

            var message = I18NHelper.FormatInvariant(
                invalidColumns.Count > maxPropertiesToShow
                    ? ErrorMessages.ArtifactList.ColumnsSettings.MultipleInvalidColumns
                    : ErrorMessages.ArtifactList.ColumnsSettings.SingleOrSomeInvalidColumns,
                string.Join(", ", invalidColumns.Take(maxPropertiesToShow).Select(column => column.PropertyName)));

            return new BadRequestException(message, ErrorCodes.InvalidColumns);
        }
    }
}
