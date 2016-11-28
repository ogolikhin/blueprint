using System.Threading.Tasks;
using SearchService.Models;
using ServiceLibrary.Repositories;
using System.Data;
using Dapper;
using SearchService.Helpers;
using System.Data.SqlClient;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace SearchService.Repositories
{
    public class SqlProjectSearchRepository : IProjectSearchRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        private readonly ISearchConfigurationProvider _searchConfigurationProvider;

        public SqlProjectSearchRepository() : this(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString), new SearchConfiguration())
        {
        }

        internal SqlProjectSearchRepository(ISqlConnectionWrapper connectionWrapper, ISearchConfiguration configuration)
        {
            ConnectionWrapper = connectionWrapper;
            _searchConfigurationProvider = new SearchConfigurationProvider(configuration);
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

            try
            {
                var items = await ConnectionWrapper.QueryAsync<SearchResult>("GetProjectsByName",
                param,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _searchConfigurationProvider.SearchTimeout);
                return new ProjectSearchResultSet
                {
                    Items = items
                };
            }
            catch (SqlException sqlException)
            {
                //Sql timeout error
                if (sqlException.Number == -2)
                {
                    throw new SqlTimeoutException("Server did not respond with a response in the allocated time. Please try again later.", ErrorCodes.Timeout);
                }
                throw;
            }
        }
    }
}
