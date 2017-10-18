using ServiceLibrary.Helpers;

namespace SearchService.Helpers
{
    public class SearchConfigurationProvider : ISearchConfigurationProvider
    {
        private ISearchConfiguration _configuration;
        public SearchConfigurationProvider(ISearchConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int PageSize
        {
            get
            {
                var pageSize = _configuration.PageSize.ToInt32(ServiceConstants.SearchPageSize);

                return pageSize > 0 ? pageSize : ServiceConstants.SearchPageSize;
            }
        }
        public int MaxItems
        {
            get
            {
                var maxItems = _configuration.MaxItems.ToInt32(ServiceConstants.MaxSearchItems);

                return maxItems > 0 ? maxItems : ServiceConstants.MaxSearchItems;
            }
        }

        public int MaxSearchableValueStringSize
        {
            get
            {
                var maxSearchableValueStringSize = _configuration.MaxSearchableValueStringSize.ToInt32(ServiceConstants.MaxSearchableValueStringSize);

                return maxSearchableValueStringSize > 0 ? maxSearchableValueStringSize : ServiceConstants.MaxSearchableValueStringSize;
            }
        }

        /// <summary>
        /// Search Sql Timeout in seconds. System defined default is 120 seconds.
        /// Sql server default value is 30 secs.
        /// Setting it to 0 means no timeout
        /// </summary>
        public int SearchTimeout
        {
            get
            {
                var searchTimeout = _configuration.SearchTimeout.ToInt32(ServiceConstants.DefaultSearchTimeout); ;

                return searchTimeout >= 0 ? searchTimeout : ServiceConstants.DefaultSearchTimeout;
            }
        }
    }
}