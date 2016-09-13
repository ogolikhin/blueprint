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

        public async Task<FullTextSearchResult> Search(SearchCriteria searchCriteria, int page, int pageSize)
        {
            FullTextSearchResult result = new FullTextSearchResult();

            var prm = new DynamicParameters();
            prm.Add("@userId", searchCriteria.UserId);
            prm.Add("@query", searchCriteria.Query);
            prm.Add("@projectIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ProjectIds));
            prm.Add("@itemTypeIds", SqlMapperHelper.ToInt32Collection(searchCriteria.ItemTypeIds));
            prm.Add("@page", page);
            prm.Add("@pageSize", pageSize);
            prm.Add("@maxItems", WebApiConfig.MaxItems);
            prm.Add("@maxSearchableValueStringSize", WebApiConfig.MaxSearchableValueStringSize);

            var queryResult = await ConnectionWrapper.QueryMultipleAsync<FullTextSearchItem, FullTextSearchTypeItem, int>("SearchFullText", prm, commandType: CommandType.StoredProcedure);
            result.FullTextSearchItems = queryResult.Item1;
            result.FullTextSearchTypeItems = queryResult.Item2;
            result.TotalCount = queryResult.Item3.ElementAt(0);

            return result;
        }
    }
}