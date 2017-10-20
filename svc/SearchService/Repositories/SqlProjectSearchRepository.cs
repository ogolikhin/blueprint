using Dapper;
using SearchService.Helpers;
using SearchService.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SearchService.Repositories
{
    public class SqlProjectSearchRepository : IProjectSearchRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly ISearchConfigurationProvider _searchConfigurationProvider;

        public SqlProjectSearchRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SearchConfiguration())
        {
        }

        internal SqlProjectSearchRepository(ISqlConnectionWrapper connectionWrapper, ISearchConfiguration configuration)
        {
            _connectionWrapper = connectionWrapper;
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
                var items = await _connectionWrapper.QueryAsync<ProjectSearchResult>("GetProjectsByName",
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
                // Sql timeout error
                if (sqlException.Number == ErrorCodes.SqlTimeoutNumber)
                {
                    throw new SqlTimeoutException("Server did not respond with a response in the allocated time. Please try again later.", ErrorCodes.Timeout);
                }
                throw;
            }
        }
    }
}
