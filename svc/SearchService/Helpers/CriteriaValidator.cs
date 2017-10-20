using System;
using System.Linq;
using SearchService.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace SearchService.Helpers
{
    enum SearchOption
    {
        FullTextSearch,
        ItemName,
        ProjectName
    }

    class CriteriaValidator
    {
        public void Validate(SearchOption searchOption, bool modelStateIsValid, SearchCriteria criteria, int minSearchQueryLimit)
        {
            switch (searchOption)
            {
                case SearchOption.FullTextSearch:
                    ValidateFullTextCriteria(modelStateIsValid, criteria as FullTextSearchCriteria, minSearchQueryLimit);
                    break;
                case SearchOption.ItemName:
                    ValidateItemNameCriteria(modelStateIsValid, criteria as ItemNameSearchCriteria, minSearchQueryLimit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(searchOption), searchOption, null);
            }
        }

        private void ValidateFullTextCriteria(bool modelStateIsValid, FullTextSearchCriteria searchCriteria, int minSearchQueryLimit = 1)
        {
            if (IsCriteriaQueryInvalid(modelStateIsValid, searchCriteria, minSearchQueryLimit, ServiceConstants.MaxSearchQueryCharLimit) ||
                !searchCriteria.ProjectIds.Any())
            {
                throw new BadRequestException("Please provide correct search criteria", ErrorCodes.IncorrectSearchCriteria);
            }
        }

        private void ValidateItemNameCriteria(bool modelStateIsValid, ItemNameSearchCriteria searchCriteria, int minSearchQueryLimit = 1)
        {
            if (IsCriteriaQueryInvalid(modelStateIsValid, searchCriteria, minSearchQueryLimit) ||
                !searchCriteria.ProjectIds.Any())
            {
                throw new BadRequestException("Please provide correct search criteria", ErrorCodes.IncorrectSearchCriteria);
            }
        }

        private bool IsCriteriaQueryInvalid(bool modelStateIsValid, SearchCriteria searchCriteria, int minSearchQueryLimit = 1, int maxLengthQueryLimit = int.MaxValue)
        {
            return !modelStateIsValid ||
                   string.IsNullOrWhiteSpace(searchCriteria?.Query) ||
                   searchCriteria.Query.Trim().Length < minSearchQueryLimit ||
                   searchCriteria.Query.Trim().Length > maxLengthQueryLimit;
        }
    }
}