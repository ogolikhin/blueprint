using System;
using System.Collections.Generic;
using System.Net;
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
        /// <param name="page">(optional)An index to a subset of search results the length of which is determined 
        /// by the pageSize argument</param>
        /// <param name="pageSize">(optional)The number of search results to return in a single request.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The subset of search results.</returns>
        FullTextSearchResult Search(IUser user, FullTextSearchCriteria searchCriteria, int? page = null, int? pageSize = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Returns the search result metadata indicating what would be returned if the full search 
        /// were performed.
        /// </summary>
        /// <param name="user">The user performing the search.</param>
        /// <param name="searchCriteria">The criteria for the search request. (i.e. "search", 
        /// "search phrase", "search ph*)</param>
        /// <param name="pageSize">(optional)The number of search results to return in a single request.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The search result metadata.</returns>
        FullTextSearchMetaDataResult SearchMetaData(IUser user, FullTextSearchCriteria searchCriteria, int? pageSize = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Returns the list of projects name which contains searchText
        /// </summary>
        /// <param name="user">The user performing the search.</param>
        /// <param name="searchText">Text to search</param>
        /// <param name="resultCount">(optional)The number of search results to return.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of projects.</returns>
        List<ProjectSearchResult> SearchProjects(IUser user, string searchText, int resultCount, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
