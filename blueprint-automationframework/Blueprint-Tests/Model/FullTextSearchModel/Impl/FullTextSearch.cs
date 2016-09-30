using System.Collections.Generic;
using System.Net;
using Common;
using Model.Impl;
using Utilities;
using Utilities.Facades;

namespace Model.FullTextSearchModel.Impl
{
    public class FullTextSearch : IFullTextSearch
    {
        public string Address { get; }

        #region Constructor

        public FullTextSearch(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #endregion Constructor

        #region Members inherited from IFullTextSearch

        public FullTextSearchResult Search(IUser user, FullTextSearchCriteria searchCriteria, int? page = null, int? pageSize = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(FullTextSearch), nameof(Search));

            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(searchCriteria, nameof(searchCriteria));

            var queryParams = new Dictionary<string, string>();

            if (page != null)
            {
                queryParams.Add("page", page.ToString());
            }

            if (pageSize != null)
            {
                queryParams.Add("pageSize", pageSize.ToString());
            }

            var tokenValue = user.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Projects: {1} Item Types: {2} Search criteria: {3}", nameof(FullTextSearch), searchCriteria.ProjectIds, searchCriteria.ItemTypeIds, searchCriteria.Query);

            var restResponse = restApi.SendRequestAndDeserializeObject<FullTextSearchResult, FullTextSearchCriteria>(
                RestPaths.Svc.SearchService.FULLTEXTSEARCH,
                RestRequestMethod.POST,
                searchCriteria,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);

            return restResponse;
        }

        public FullTextSearchMetaDataResult SearchMetaData(IUser user, FullTextSearchCriteria searchCriteria, int? pageSize, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(FullTextSearch), nameof(SearchMetaData));

            ThrowIf.ArgumentNull(user, nameof(user));

            var queryParams = new Dictionary<string, string>();

            if (pageSize != null)
            {
                queryParams.Add("pageSize", pageSize.ToString());
            }

            var tokenValue = user.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Projects: {1} Item Types: {2} Search criteria: {3} Page Size: {4}", nameof(FullTextSearch), searchCriteria?.ProjectIds, searchCriteria?.ItemTypeIds, searchCriteria?.Query, pageSize);

            var restResponse = restApi.SendRequestAndDeserializeObject<FullTextSearchMetaDataResult, FullTextSearchCriteria>(
                RestPaths.Svc.SearchService.FullTextSearch.METADATA,
                RestRequestMethod.POST,
                searchCriteria,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);

            return restResponse;
        }

        /// <seealso cref="FullTextSearch.SearchProjects(IUser, string, int, List{HttpStatusCode})"/>
        public List<ProjectSearchResult> SearchProjects(IUser user, string searchText, int resultCount, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var queryParams = new Dictionary<string, string>();

            queryParams.Add("searchText", searchText);

            var tokenValue = user.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            var projects = restApi.SendRequestAndDeserializeObject<List<ProjectSearchResult>>(
                RestPaths.Svc.SearchService.PROJECTSEARCH,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);

            return projects;
        }

        #endregion Members inherited from IFullTextSearch
    }
}