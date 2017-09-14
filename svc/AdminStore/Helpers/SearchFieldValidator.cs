using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class SearchFieldValidator
    {
        public const int MaxSearchLength = 250;

        public static void Validate(string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                if (search.Length > MaxSearchLength)
                {
                    throw new BadRequestException(ErrorMessages.SearchFieldLimitation, ErrorCodes.BadRequest);
                }
            }
        }
    }
} 