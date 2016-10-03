using System.Collections.Generic;
using System.Net;
using Model.SearchServiceModel.Impl;

namespace Model.SearchServiceModel
{
    public interface ISearchService
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
        FullTextSearchResult FullTextSearch(IUser user, FullTextSearchCriteria searchCriteria, int? page = null, int? pageSize = null, List<HttpStatusCode> expectedStatusCodes = null);

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
        FullTextSearchMetaDataResult FullTextSearchMetaData(IUser user, FullTextSearchCriteria searchCriteria, int? pageSize = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets the current status of the SearchService service.
        /// (Runs: GET /status)
        /// </summary>
        /// <param name="preAuthorizedKey">(optional) The pre-authorized key to use for authentication.  Defaults to a valid key.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of this service and its dependent services.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Ignore this warning.
        string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets the current status of the SearchService service.
        /// (Runs: GET /status/upcheck)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Status of SearchService service.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Ignore this warning.
        HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null);


        /// <summary>
        /// Returns the list of projects name which contains searchText
        /// </summary>
        /// <param name="user">The user performing the search.</param>
        /// <param name="searchText">Text to search</param>
        /// <param name="resultCount">(optional)The number of search results to return.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of projects.</returns>
        List<ProjectSearchResult> SearchProjects(IUser user, string searchText, int? resultCount = null, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
