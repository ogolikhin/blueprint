using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchService.Models;
using ServiceLibrary.Repositories;
using System.Data;
using ArtifactStore.Models;
using Dapper;

namespace SearchService.Repositories
{
    public class SqlProjectSearchRepository : IProjectSearchRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        //private ISearchConfigurationProvider _searchConfigurationProvider;
        private readonly IFullTextSearchRepository _fullTextSearchRepository;

        public SqlProjectSearchRepository() : this(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString))
        {
        }

        internal SqlProjectSearchRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
            _fullTextSearchRepository = new SqlFullTextSearchRepository();
            //_searchConfigurationProvider = new SearchConfigurationProvider(configuration);
        }

        /// <summary>
        /// Performs searching projects by name
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <param name="searchText">SearchText</param>
        /// <param name="resultCount">ResultCount</param>
        /// <returns></returns>
        public async Task<IEnumerable<ProjectSearchResult>> GetProjectsByName(int userId, string searchText, int resultCount)
        {
            var searchPrms = new DynamicParameters();
            searchPrms.Add("@userId", userId);
            searchPrms.Add("@projectName", searchText);
            searchPrms.Add("@resultCount", resultCount);

            return (await ConnectionWrapper.QueryAsync<ProjectSearchResult>("GetProjectsByName", searchPrms, commandType: CommandType.StoredProcedure)).ToList();
        }

        public Task<IEnumerable<ItemSearchResult>> FindItemByName(int userId, string searchText, int[] projectIds, int[] itemTypes, int resultCount)
        {
            var searchCriteria = new SearchCriteria()
            {
                Query = searchText,
                ItemTypeIds = itemTypes,
                ProjectIds = projectIds                
            };

            _fullTextSearchRepository.Search(userId, searchCriteria);
        }
    }
}