using CustomAttributes;
using Model.FullTextSearchModel;
using Model.FullTextSearchModel.Impl;

namespace Model.Factories
{
    public static class SearchServiceFactory
    {
        /// <summary>
        /// Creates a new IFullTextSearch.
        /// </summary>
        /// <param name="address">The URI address of the searchservice.</param>
        /// <returns>An IFullTextSearch object.</returns>
        public static IFullTextSearch CreateFullTextSearch(string address)
        {
            IFullTextSearch fullTextSearch = new FullTextSearch(address);
            return fullTextSearch;
        }

        /// <summary>
        /// Creates a SearchService object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The FullTextSearch object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IFullTextSearch GetSearchServiceFromTestConfig()
        {
            string address = FactoryCommon.GetServiceAddressFromTestConfig(Categories.SearchService);
            return CreateFullTextSearch(address);
        }
    }
}
