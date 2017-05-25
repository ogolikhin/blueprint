using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    public class PaginationValidator
    {
        public static void ValidatePaginationModel(Pagination pagination)
        {
            if (pagination == null)
            {
                throw new BadRequestException(ErrorMessages.InvalidPagination, ErrorCodes.BadRequest);
            }

            if (pagination.Limit < 1)
            {
                throw new BadRequestException(ErrorMessages.IncorrectLimitParameter, ErrorCodes.BadRequest);
            }

            if (pagination.Offset < 0)
            {
                throw new BadRequestException(ErrorMessages.IncorrectOffsetParameter, ErrorCodes.BadRequest);
            }
        }
    }
}