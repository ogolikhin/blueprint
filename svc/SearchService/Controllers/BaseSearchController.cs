using System;
using System.Linq;
using SearchService.Helpers;
using SearchService.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace SearchService.Controllers
{
    public abstract class BaseSearchController : LoggableApiController
    {
        #region Protected

        protected int ValidateAndExtractUserId()
        {
            // get the UserId from the session
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                throw new AuthenticationException("Authorization is required", ErrorCodes.UnauthorizedAccess);
            }
            return userId.Value;
        }

        protected void ValidateCriteria(ISearchCriteria searchCriteria, int minSearchQueryLimit = 1)
        {

            if (!ModelState.IsValid || !ValidateSearchCriteria(searchCriteria, minSearchQueryLimit))
            {
                throw new BadRequestException("Please provide correct search criteria", ErrorCodes.IncorrectSearchCriteria);
            }
        }

        protected int GetPageSize(ISearchConfigurationProvider searchConfigurationProvider, int? requestedPageSize, int maxPageSize = Int32.MaxValue)
        {
            int searchPageSize = requestedPageSize.GetValueOrDefault(searchConfigurationProvider.PageSize);
            if (searchPageSize <= 0)
            {
                searchPageSize = searchConfigurationProvider.PageSize;
            }

            if (searchPageSize > maxPageSize)
            {
                searchPageSize = maxPageSize;
            }
            return searchPageSize;
        }

        protected int GetStartCounter(int? requestedStartCounter, int minStartCounter, int defaultCounterValue)
        {

            int startCounter = requestedStartCounter.GetValueOrDefault(defaultCounterValue);
            if (startCounter < minStartCounter)
            {
                startCounter = defaultCounterValue;
            }
            return startCounter;
        }

       

        #endregion

        #region Private

        private int? GetUserId()
        {
            object sessionValue;
            if (!Request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionValue))
            {
                return null;
            }
            var session = sessionValue as Session;
            return session?.UserId;
        }

        private bool ValidateSearchCriteria(ISearchCriteria searchCriteria, int minSearchQueryLimit = 1)
        {
            if (string.IsNullOrWhiteSpace(searchCriteria?.Query) ||
                searchCriteria.Query.Trim().Length < minSearchQueryLimit ||
                !searchCriteria.ProjectIds.Any())
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}