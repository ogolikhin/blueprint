using System;
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
    public class SqlItemSearchRepository : IItemSearchRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        private ISearchConfigurationProvider _searchConfigurationProvider;

        public SqlItemSearchRepository() : this(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString), new SearchConfiguration())
        {
        }

        internal SqlItemSearchRepository(ISqlConnectionWrapper connectionWrapper, ISearchConfiguration configuration)
        {
            ConnectionWrapper = connectionWrapper;
            _searchConfigurationProvider = new SearchConfigurationProvider(configuration);
        }

        /// <summary>
        /// Perform a full text search
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="page">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public async Task<ItemSearchResult> FindItemByName(int userId, ItemSearchCriteria searchCriteria, int startOffset, int pageSize)
        {
            IEnumerable<ItemSearchResultItem> queryResult;
            ItemSearchResult result = new ItemSearchResult();

            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@query", GetQuery(searchCriteria.Query));
            prm.Add("@projectIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ProjectIds));            
            prm.Add("@primitiveItemTypePredefineds", SqlMapperHelper.ToInt32Collection(new[] { 4097, 4098, 4353, 4354, 4355, 4609, 4610, 32769 }));
            prm.Add("@startOffset", startOffset);
            prm.Add("@pageSize", pageSize);            
            prm.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);

            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                prm.Add("@itemTypeIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ItemTypeIds));
                queryResult = await ConnectionWrapper.QueryAsync<ItemSearchResultItem>("SearchItemNameByItemTypes", prm, commandType: CommandType.StoredProcedure);
            }
            else
            {
                queryResult = await ConnectionWrapper.QueryAsync<ItemSearchResultItem>("SearchItemName", prm, commandType: CommandType.StoredProcedure);
            }
            result.SearchItems = queryResult;

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