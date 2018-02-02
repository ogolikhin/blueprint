using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models
{
    public static class PaginationExtensions
    {
        public static void Validate(this Pagination pagination)
        {
            Validate(pagination, nullAllowed: false);
        }

        public static void Validate(this Pagination pagination, bool nullAllowed)
        {
            var paginationNotNullValid = nullAllowed
                || pagination != null && (pagination.Offset.HasValue || pagination.Limit.HasValue);
            if (!paginationNotNullValid)
            {
                throw new BadRequestException(ErrorMessages.InvalidPagination, ErrorCodes.BadRequest);
            }

            var offsetValid =
                nullAllowed && !pagination.Offset.HasValue
                || pagination.Offset.HasValue && pagination.Offset >= 0;
            if (!offsetValid)
            {
                throw new BadRequestException(ErrorMessages.IncorrectOffsetParameter, ErrorCodes.BadRequest);
            }

            var limitValid =
                nullAllowed && !pagination.Limit.HasValue
                || pagination.Limit.HasValue && pagination.Limit > 0;
            if (!limitValid)
            {
                throw new BadRequestException(ErrorMessages.IncorrectLimitParameter, ErrorCodes.BadRequest);
            }
        }
    }
}
