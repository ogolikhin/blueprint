using ServiceLibrary.Helpers;
using System;
using System.Globalization;

namespace AdminStore.Helpers
{
    public class JobsValidationHelper
    {
        public string Validate(int? page, int? pageSize)
        {
            if (!ValidateGetPage(page))
            {
                return "Page value must be provided and be greater than 0";
            }
            if (!ValidatePageSize(pageSize))
            {
                return String.Format(CultureInfo.CurrentCulture, "Page Size value must be provided and value between 1 and {0}", ServiceConstants.JobsMaxPageSize);
            }
            return null;
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