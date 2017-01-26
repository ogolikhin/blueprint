using CustomAttributes;
using Model.SearchServiceModel;
using Model.SearchServiceModel.Impl;

namespace Model.Factories
{
    public static class SearchServiceFactory
    {
        /// <summary>
        /// Creates a new ISearchService.
        /// </summary>
        /// <param name="address">The URI address of the searchservice.</param>
        /// <returns>An ISearchService object.</returns>
        public static ISearchService CreateSearchService(string address)
        {
            var searchService = new SearchService(address);
            return searchService;
        }

        /// <summary>
        /// Creates a SearchService object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The SearchService object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static ISearchService GetSearchServiceFromTestConfig()
        {
            var address = FactoryCommon.GetServiceAddressFromTestConfig(Categories.SearchService);
            return CreateSearchService(address);
        }
    }
}
