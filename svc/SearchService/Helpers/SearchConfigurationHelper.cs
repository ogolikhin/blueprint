using ServiceLibrary.Helpers;

namespace SearchService.Helpers
{
    public class SearchConfigurationHelper
    {
        private IConfiguration _configuration;
        public SearchConfigurationHelper(IConfiguration configuration)
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
    }
}