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

            string message;

            if (invalidColumns.Count == 1)
            {
                message = I18NHelper.FormatInvariant(
                    ErrorMessages.ArtifactList.ColumnsSettings.SingleInvalidColumn,
                    invalidColumns.First().PropertyName);
            }
            else
            {
                const int maxPropertiesToShow = 3;

                message = I18NHelper.FormatInvariant(
                    invalidColumns.Count > maxPropertiesToShow ?
                        ErrorMessages.ArtifactList.ColumnsSettings.MultipleInvalidColumns :
                        ErrorMessages.ArtifactList.ColumnsSettings.SomeInvalidColumns,
                    string.Join(", ", invalidColumns.Take(maxPropertiesToShow).Select(column => column.PropertyName)));
            }

            return new BadRequestException(message, ErrorCodes.BadRequest);
        }
    }
}
