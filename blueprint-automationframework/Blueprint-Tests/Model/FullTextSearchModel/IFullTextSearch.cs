using System;
using Model.FullTextSearchModel.Impl;
using Model.Impl;

namespace Model.FullTextSearchModel
{
    public interface IFullTextSearch
    {
        /// <summary>
        /// Returns a subset of search results based upon the page index that was requested.
        /// </summary>
        /// <param name="user">The user performing the search.</param>
        /// <param name="searchCriteria">The criteria for the search request. (i.e. "search", 
        /// "search phrase", "search ph*)</param>
        /// <param name="page">An index to a subset of search results the length of which is determined 
        /// by the pageSize argument</param>
        /// <param name="pageSize">The number of search results to return in a single request.</param>
        /// <returns>The subset of search results.</returns>
        FullTextSearchResult Search(User user, FullTextSearchCriteria searchCriteria, int page, int pageSize);

        /// <summary>
        /// Returns the search result metadata indicating what would be returned if the full search 
        /// were performed.
        /// </summary>
        /// <param name="user">The user performing the search.</param>
        /// <param name="searchCriteria">The criteria for the search request. (i.e. "search", 
        /// "search phrase", "search ph*)</param>
        /// <returns>The search result metadata.</returns>
        FullTextSearchMetaDataResult SearchMetaData(User user, FullTextSearchCriteria searchCriteria);
    }
}
