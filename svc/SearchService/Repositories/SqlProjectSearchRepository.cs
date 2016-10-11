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
        /// <param name="searchText">Search Text</param>
        /// <param name="resultCount">Result Count</param>
        /// <param name="separatorChar">Separator Character</param>
        /// <returns></returns>
        public async Task<IEnumerable<ProjectSearchResult>> GetProjectsByName(
            int userId, 
            string searchText, 
            int resultCount,
            string separatorChar)
        {
            var searchPrms = new DynamicParameters();
            searchPrms.Add("@userId", userId);
            searchPrms.Add("@projectName", searchText);
            searchPrms.Add("@resultCount", resultCount);
            searchPrms.Add("@separatorChar", separatorChar);

            return (await ConnectionWrapper.QueryAsync<ProjectSearchResult>(
                "GetProjectsByName", 
                searchPrms, 
                commandType: CommandType.StoredProcedure)).ToList();
        }
    }
}