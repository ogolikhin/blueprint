﻿using Common;
using Model.Impl;
using System.Collections.Generic;
using System.Net;
using System.Web;
using Utilities;
using Utilities.Facades;

namespace Model.SearchServiceModel.Impl
{
    public class SearchService : NovaServiceBase, ISearchService
    {

        #region Constructor

        public SearchService(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #endregion Constructor

        #region Members inherited from ISearchService

        /// <seealso cref="ISearchService.FullTextSearch(IUser, FullTextSearchCriteria, int?, int?, List{HttpStatusCode})"/>
        public FullTextSearchResult FullTextSearch(IUser user,
            FullTextSearchCriteria searchCriteria,
            int? page = null,
            int? pageSize = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SearchService), nameof(FullTextSearch));

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

            var tokenValue = user?.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Projects: {1} Item Types: {2} Search criteria: {3}",
                nameof(SearchService), searchCriteria.ProjectIds, searchCriteria.ItemTypeIds, searchCriteria.Query);

            var restResponse = restApi.SendRequestAndDeserializeObject<FullTextSearchResult, FullTextSearchCriteria>(
                RestPaths.Svc.SearchService.FULLTEXTSEARCH,
                RestRequestMethod.POST,
                searchCriteria,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);

            return restResponse;
        }

        /// <seealso cref="ISearchService.FullTextSearchMetaData(IUser, FullTextSearchCriteria, int?, List{HttpStatusCode})"/>
        public FullTextSearchMetaDataResult FullTextSearchMetaData(IUser user,
            FullTextSearchCriteria searchCriteria,
            int? pageSize,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SearchService), nameof(FullTextSearchMetaData));

            ThrowIf.ArgumentNull(user, nameof(user));

            var queryParams = new Dictionary<string, string>();

            if (pageSize != null)
            {
                queryParams.Add("pageSize", pageSize.ToString());
            }

            var tokenValue = user.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Projects: {1} Item Types: {2} Search criteria: {3} Page Size: {4}",
                nameof(SearchService), searchCriteria?.ProjectIds, searchCriteria?.ItemTypeIds, searchCriteria?.Query, pageSize);

            var restResponse = restApi.SendRequestAndDeserializeObject<FullTextSearchMetaDataResult, FullTextSearchCriteria>(
                RestPaths.Svc.SearchService.FullTextSearch.METADATA,
                RestRequestMethod.POST,
                searchCriteria,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes, 
                shouldControlJsonChanges: true);

            return restResponse;
        }

        /// <seealso cref="ISearchService.SearchProjects(IUser, string, int?, string, List{HttpStatusCode})"/>
        public List<ProjectSearchResult> SearchProjects(IUser user,
            string searchText,
            int? resultCount = null,
            string separatorString = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string url = RestPaths.Svc.SearchService.PROJECTSEARCH;

            if (separatorString != null)
            {
                // we use concat instead of query param to make call identical to call from fron-end		
                url = string.Concat(url, "?separatorString=", HttpUtility.UrlEncode(separatorString));
            }

            var jsonObject = new Dictionary<string, string> {{ "query", searchText }};
            
            var queryParams = new Dictionary<string, string>();

            if (resultCount != null)
            {
                queryParams.Add("resultCount", resultCount.ToString());
            }
            else
            {
                queryParams = null;
            }

            var tokenValue = user.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            var projectSearchResult = restApi.SendRequestAndDeserializeObject<ProjectSearchResultSet, Dictionary<string, string>>(
                url,
                RestRequestMethod.POST,
                jsonObject: jsonObject,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return projectSearchResult.Items;
        }

        /// <seealso cref="ISearchService.SearchItems(IUser, ItemNameSearchCriteria, int?, int?, List{HttpStatusCode})"/>
        public ItemNameSearchResultSet SearchItems(IUser user,
            ItemNameSearchCriteria searchCriteria,
            int? startOffset = null,
            int? pageSize = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(searchCriteria, nameof(searchCriteria));

            var queryParams = new Dictionary<string, string>();

            if (pageSize != null)
            {
                queryParams.Add("pageSize", pageSize.ToString());
            }
            if (startOffset != null)
            {
                queryParams.Add("startOffset", startOffset.ToString());
            }

            var tokenValue = user.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            string url = RestPaths.Svc.SearchService.ITEMNAMESEARCH;

            var itemSearchResult = restApi.SendRequestAndDeserializeObject<ItemNameSearchResultSet, ItemNameSearchCriteria>(
                url,
                RestRequestMethod.POST,
                searchCriteria,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return itemSearchResult;
        }

        /// <seealso cref="ISearchService.GetStatus(string, List{HttpStatusCode})"/>
        public string GetStatus(string preAuthorizedKey = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatus(RestPaths.Svc.SearchService.STATUS, preAuthorizedKey, expectedStatusCodes);
        }

        /// <seealso cref="ISearchService.GetStatusUpcheck(List{HttpStatusCode})"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(RestPaths.Svc.SearchService.Status.UPCHECK, expectedStatusCodes);
        }

        #endregion Members inherited from IFullTextSearch
    }
}
