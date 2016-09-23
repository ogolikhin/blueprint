﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using ServiceLibrary.Repositories;
using System.Threading.Tasks;
using Dapper;
using SearchService.Models;
using ServiceLibrary.Helpers;
using SearchService.Helpers;

namespace SearchService.Repositories
{
    public class SqlFullTextSearchRepository : IFullTextSearchRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        private SearchConfigurationHelper _searchConfigurationHelper;

        public SqlFullTextSearchRepository() : this(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString), new Configuration())
        {
        }

        internal SqlFullTextSearchRepository(ISqlConnectionWrapper connectionWrapper, IConfiguration configuration)
        {
            ConnectionWrapper = connectionWrapper;
            _searchConfigurationHelper = new SearchConfigurationHelper(configuration);
        }

        /// <summary>
        /// Perform a full text search
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="page">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public async Task<FullTextSearchResult> Search(int userId, SearchCriteria searchCriteria, int page, int pageSize)
        {
            IEnumerable<FullTextSearchItem> queryResult;
            FullTextSearchResult result = new FullTextSearchResult();

            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@query", GetQuery(searchCriteria.Query));
            prm.Add("@projectIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ProjectIds));
            prm.Add("@predefineds", SqlMapperHelper.ToInt32Collection(new [] { 4098, 4099, 4115, 16384 }));
            prm.Add("@primitiveItemTypePredefineds", SqlMapperHelper.ToInt32Collection(new [] { 4097, 4098, 4353, 4354, 4355, 4609, 4610, 32769 }));
            prm.Add("@page", page);
            prm.Add("@pageSize", pageSize);
            prm.Add("@maxItems", _searchConfigurationHelper.MaxItems);
            prm.Add("@maxSearchableValueStringSize", _searchConfigurationHelper.MaxSearchableValueStringSize);

            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                prm.Add("@itemTypeIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ItemTypeIds));
                queryResult = await ConnectionWrapper.QueryAsync<FullTextSearchItem>("SearchFullTextByItemTypes", prm, commandType: CommandType.StoredProcedure);
            }
            else
            {
                queryResult = await ConnectionWrapper.QueryAsync<FullTextSearchItem>("SearchFullText", prm, commandType: CommandType.StoredProcedure);
            }
            result.FullTextSearchItems = queryResult;

            return result;
        }

        /// <summary>
        /// Perform a full text search
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <returns>FullTextSearchMetaDataResult</returns>
        public async Task<FullTextSearchMetaDataResult> SearchMetaData(int userId, SearchCriteria searchCriteria)
        {
            Tuple<IEnumerable<FullTextSearchTypeItem>, IEnumerable<int?>> queryResult;
            FullTextSearchMetaDataResult result = new FullTextSearchMetaDataResult();

            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@query", GetQuery(searchCriteria.Query));
            prm.Add("@projectIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ProjectIds));
            prm.Add("@predefineds", SqlMapperHelper.ToInt32Collection(new [] { 4098, 4099, 4115, 16384 }));
            prm.Add("@primitiveItemTypePredefineds", SqlMapperHelper.ToInt32Collection(new [] { 4097, 4098, 4353, 4354, 4355, 4609, 4610, 32769 }));
            prm.Add("@maxItems", _searchConfigurationHelper.MaxItems);
            prm.Add("@maxSearchableValueStringSize", _searchConfigurationHelper.MaxSearchableValueStringSize);

            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                prm.Add("@itemTypeIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ItemTypeIds));
                queryResult = await ConnectionWrapper.QueryMultipleAsync<FullTextSearchTypeItem, int?>("SearchFullTextByItemTypesMetaData", prm, commandType: CommandType.StoredProcedure);
            }
            else
            {
                queryResult = await ConnectionWrapper.QueryMultipleAsync<FullTextSearchTypeItem, int?>("SearchFullTextMetaData", prm, commandType: CommandType.StoredProcedure);
            }
            result.FullTextSearchTypeItems = queryResult.Item1;
            var totalCount = queryResult.Item2.ElementAt(0);
            result.TotalCount = totalCount ?? 0;

            return result;
        }

        private string GetQuery(string input)
        {
            //Unfortunately, double-quotes have special meaning inside FTI, so even if you parameterize it, the FTI engine treats it as a phrase delimiter. 
            //doubling the quote to "" fixes it. 
            //Likewise, ' needs to be doubled to '' before passing to FTI (completely separate to TSQL escaping)
            return string.IsNullOrWhiteSpace(input) ? string.Empty :
                string.Format(CultureInfo.InvariantCulture, "\"{0}\"", input.Replace("'", "''").Replace("\"", "\"\""));
        }
    }
}