using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SearchService.Helpers;
using SearchService.Models;
using ServiceLibrary.Repositories;

namespace SearchService.Repositories
{
    public class SqlItemSearchRepository : IItemSearchRepository
    {
        internal static DataTable Predefineds = SqlConnectionWrapper.ToDataTable(new[] { 4098, 4099, 4115, 16384 }, "Int32Collection", "Int32Value");
        internal static DataTable PrimitiveItemTypePredefineds = SqlConnectionWrapper.ToDataTable(new[]
        {
            4097, // Project
            4098, // Baseline
            4353, // Baseline Folder
            4354, // Artifact Baseline
            4355, // ArtifactReviewPackage
            4609, // Collection Folder
            4610, // Artifact Collection
            32769 // Data Object
        }, "Int32Collection", "Int32Value");

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
        /// <param name="startOffset">Search start offset</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public async Task<ItemSearchResult> SearchName(int userId, ItemSearchCriteria searchCriteria, int startOffset, int pageSize)
        {
            IEnumerable<ItemSearchResultItem> queryResult;
            ItemSearchResult result = new ItemSearchResult();

            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@query", searchCriteria.Query);
            prm.Add("@projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value"));
            prm.Add("@primitiveItemTypePredefineds", PrimitiveItemTypePredefineds);
            prm.Add("@startOffset", startOffset);
            prm.Add("@pageSize", pageSize);
            prm.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);

            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                prm.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value"));
                queryResult = await ConnectionWrapper.QueryAsync<ItemSearchResultItem>("SearchItemNameByItemTypes", prm, commandType: CommandType.StoredProcedure);
            }
            else
            {
                queryResult = await ConnectionWrapper.QueryAsync<ItemSearchResultItem>("SearchItemName", prm, commandType: CommandType.StoredProcedure);
            }
            result.SearchItems = queryResult;

            return result;
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
            prm.Add("@projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value"));
            prm.Add("@predefineds", Predefineds);
            prm.Add("@primitiveItemTypePredefineds", PrimitiveItemTypePredefineds);
            prm.Add("@page", page);
            prm.Add("@pageSize", pageSize);
            prm.Add("@maxItems", _searchConfigurationProvider.MaxItems);
            prm.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);

            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                prm.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value"));
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
            prm.Add("@projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value"));
            prm.Add("@predefineds", Predefineds);
            prm.Add("@primitiveItemTypePredefineds", PrimitiveItemTypePredefineds);
            prm.Add("@maxItems", _searchConfigurationProvider.MaxItems);
            prm.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);

            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                prm.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value"));
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

        internal static string GetQuery(string input)
        {
            //Unfortunately, double-quotes have special meaning inside FTI, so even if you parameterize it, the FTI engine treats it as a phrase delimiter.
            //doubling the quote to "" fixes it.
            //Likewise, ' needs to be doubled to '' before passing to FTI (completely separate to TSQL escaping)
            return string.IsNullOrWhiteSpace(input) ? string.Empty :
                string.Format(CultureInfo.InvariantCulture, "\"{0}\"", input.Replace("'", "''").Replace("\"", "\"\""));
        }
    }
}
