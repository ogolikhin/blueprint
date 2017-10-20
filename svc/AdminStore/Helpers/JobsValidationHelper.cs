using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using System.Globalization;

namespace AdminStore.Helpers
{
    public class JobsValidationHelper
    {
        public void Validate(int? page, int? pageSize)
        {
            if (!ValidateGetPage(page))
            {
                throw new BadRequestException("Page value must be provided and be greater than 0", ErrorCodes.PageNullOrNegative);
            }
            if (!ValidatePageSize(pageSize))
            {
                throw new BadRequestException(
                    string.Format(CultureInfo.CurrentCulture, "Page Size value must be provided and value between 1 and {0}", ServiceConstants.JobsMaxPageSize),
                    ErrorCodes.PageSizeNullOrOutOfRange);
            }
        }
        private bool ValidatePageSize(int? pageSize)
        {
            return pageSize.HasValue && pageSize > 0 && pageSize <= ServiceConstants.JobsMaxPageSize;
        }

        private bool ValidateGetPage(int? requestedPage)
        {
            return requestedPage.HasValue && requestedPage > 0;
        }
    }
}
