using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchService.Models;
using ServiceLibrary.Repositories;
using System.Data;
using Dapper;

namespace SearchService.Repositories
{
    public class SqlProjectSearchRepository : IProjectSearchRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        //private ISearchConfigurationProvider _searchConfigurationProvider;

        public SqlProjectSearchRepository() : this(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString))
        {
        }

        internal SqlProjectSearchRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
            //_searchConfigurationProvider = new SearchConfigurationProvider(configuration);
        }

        /// <summary>
        /// Performs searching projects by name
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="resultCount">Result Count</param>
        /// <param name="separatorString">Separator String</param>
        /// <returns></returns>
        public async Task<ProjectSearchResultSet> SearchName(
            int userId,
            SearchCriteria searchCriteria, 
            int resultCount,
            string separatorString)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@projectName", searchCriteria.Query);
            param.Add("@resultCount", resultCount);
            param.Add("@separatorString", separatorString);

            var items = await ConnectionWrapper.QueryAsync<SearchResult>("GetProjectsByName", param, commandType: CommandType.StoredProcedure);
            return new ProjectSearchResultSet
            {
                Items = items,
            };
        }
    }
}
