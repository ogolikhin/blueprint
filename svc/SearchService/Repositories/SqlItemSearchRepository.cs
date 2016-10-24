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
        internal static readonly DataTable Predefineds = SqlConnectionWrapper.ToDataTable(new[]
        {
            4098,
            4099,
            4115,
            16384
        }, "Int32Collection", "Int32Value");
        internal static readonly DataTable PrimitiveItemTypePredefineds = SqlConnectionWrapper.ToDataTable(new[]
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
        private readonly ISearchConfigurationProvider _searchConfigurationProvider;

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
        public async Task<FullTextSearchResultSet> SearchFullText(int userId, ItemSearchCriteria searchCriteria, int page, int pageSize)
        {
            string sql;
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@query", GetQuery(searchCriteria.Query));
            param.Add("@projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value"));
            param.Add("@predefineds", Predefineds);
            param.Add("@primitiveItemTypePredefineds", PrimitiveItemTypePredefineds);
            param.Add("@page", page);
            param.Add("@pageSize", pageSize);
            param.Add("@maxItems", _searchConfigurationProvider.MaxItems);
            param.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);
            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                param.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value"));
                sql = "SearchFullTextByItemTypes";
            }
            else
            {
                sql = "SearchFullText";
            }

            var items = (await ConnectionWrapper.QueryAsync<FullTextSearchResult>(sql, param, commandType: CommandType.StoredProcedure)).ToList();
            return new FullTextSearchResultSet
            {
                Items = items,
                Page = page,
                PageItemCount = items.Count,
                PageSize = pageSize
            };
        }


        /// <summary>
        /// Return metadata for a Full Text Search
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <returns>FullTextSearchMetaDataResult</returns>
        public async Task<MetaDataSearchResultSet> FullTextMetaData(int userId, ItemSearchCriteria searchCriteria)
        {
            string sql;
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@query", GetQuery(searchCriteria.Query));
            param.Add("@projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value"));
            param.Add("@predefineds", Predefineds);
            param.Add("@primitiveItemTypePredefineds", PrimitiveItemTypePredefineds);
            param.Add("@maxItems", _searchConfigurationProvider.MaxItems);
            param.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);
            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                param.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value"));
                sql = "SearchFullTextByItemTypesMetaData";
            }
            else
            {
                sql = "SearchFullTextMetaData";
            }

            var result = await ConnectionWrapper.QueryMultipleAsync<MetaDataSearchResult, int?>(sql, param, commandType: CommandType.StoredProcedure);
            return new MetaDataSearchResultSet
            {
                Items = result.Item1,
                TotalCount = result.Item2.ElementAt(0) ?? 0,
            };
        }

        /// <summary>
        /// Perform an Item search by SearchName
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="startOffset">Search start offset</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public async Task<ItemNameSearchResultSet> SearchName(int userId, ItemSearchCriteria searchCriteria, int startOffset, int pageSize)
        {
            string sql;
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@query", searchCriteria.Query);
            param.Add("@projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value"));
            param.Add("@primitiveItemTypePredefineds", PrimitiveItemTypePredefineds);
            param.Add("@startOffset", startOffset);
            param.Add("@pageSize", pageSize);
            param.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);

            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                param.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value"));
                sql = "SearchItemNameByItemTypes";
            }
            else
            {
                sql = "SearchItemName";
            }

            var items = (await ConnectionWrapper.QueryAsync<ItemSearchResult>(sql, param, commandType: CommandType.StoredProcedure)).ToList();
            return new ItemNameSearchResultSet
            {
                Items = items,
                PageItemCount = items.Count
            };
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
