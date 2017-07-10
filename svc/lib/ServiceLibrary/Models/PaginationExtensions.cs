using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models
{
    public static class PaginationExtensions
    {
        public static void Validate(this Pagination pagination)
        {
            if (pagination == null)
            {
                throw new BadRequestException(ErrorMessages.InvalidPagination, ErrorCodes.BadRequest);
            }

            if (!pagination.Offset.HasValue || pagination.Offset < 0)
            {
                throw new BadRequestException(ErrorMessages.IncorrectOffsetParameter, ErrorCodes.BadRequest);
            }

            if (!pagination.Limit.HasValue || pagination.Limit <= 0)
            {
                throw new BadRequestException(ErrorMessages.IncorrectLimitParameter, ErrorCodes.BadRequest);
            }
        }
    }
}
