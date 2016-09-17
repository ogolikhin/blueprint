using System.Data;
using System.Linq;
using ServiceLibrary.Repositories;
using System.Threading.Tasks;
using Dapper;
using SearchService.Models;
using ServiceLibrary.Helpers;

namespace SearchService.Repositories
{
    public class SqlFullTextSearchRepository : IFullTextSearchRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public SqlFullTextSearchRepository() : this(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString))
        {
        }

        internal SqlFullTextSearchRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        /// <summary>
        /// Perform a full text search
        /// </summary>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="page">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public async Task<FullTextSearchResult> Search(SearchCriteria searchCriteria, int page, int pageSize)
        {
            FullTextSearchResult result = new FullTextSearchResult();

            var prm = new DynamicParameters();
            prm.Add("@userId", searchCriteria.UserId);
            prm.Add("@query", searchCriteria.Query);
            prm.Add("@projectIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ProjectIds));
            prm.Add("@itemTypeIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ItemTypeIds));
            prm.Add("@predefineds", SqlMapperHelper.ToInt32Collection(new int[] { 4098, 4099, 4115, 16384 }));
            prm.Add("@primitiveItemTypePredefineds", SqlMapperHelper.ToInt32Collection(new int[] { 4097, 4098, 4353, 4354, 4355, 4609, 4610, 32769 }));
            prm.Add("@page", page);
            prm.Add("@pageSize", pageSize);
            prm.Add("@maxItems", WebApiConfig.MaxItems);
            prm.Add("@maxSearchableValueStringSize", WebApiConfig.MaxSearchableValueStringSize);
            prm.Add("@includeMetadata", searchCriteria.IncludeMetaData);

            if (searchCriteria.IncludeMetaData)
            {
                var queryResult = await ConnectionWrapper.QueryMultipleAsync<FullTextSearchItem, FullTextSearchTypeItem, int?>("SearchFullText", prm, commandType: CommandType.StoredProcedure);
                result.FullTextSearchItems = queryResult.Item1;
                result.FullTextSearchTypeItems = queryResult.Item2;
                var totalCount = queryResult.Item3.ElementAt(0);
                result.TotalCount = totalCount ?? 0;
            }
            else
            {
                var queryResult = await ConnectionWrapper.QueryAsync<FullTextSearchItem>("SearchFullText", prm, commandType: CommandType.StoredProcedure);
                result.FullTextSearchItems = queryResult;
                result.FullTextSearchTypeItems = new FullTextSearchTypeItem[0];
                result.TotalCount = -1;
            }

            return result;
        }
    }
}