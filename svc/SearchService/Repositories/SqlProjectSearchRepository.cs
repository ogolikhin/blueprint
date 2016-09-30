﻿using System.Collections.Generic;
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
        /// <param name="userId">UserId</param>
        /// <param name="searchText">SearchText</param>
        /// <param name="resultCount">ResultCount</param>
        /// <returns></returns>
        public async Task<IEnumerable<ProjectSearchResult>> GetProjectsByName(int userId, string searchText, int resultCount)
        {
            var repliesPrm = new DynamicParameters();
            repliesPrm.Add("@userId", userId);
            repliesPrm.Add("@projectName", searchText);
            repliesPrm.Add("@resultCount", resultCount);

            return (await ConnectionWrapper.QueryAsync<ProjectSearchResult>("GetProjectsByName", repliesPrm, commandType: CommandType.StoredProcedure)).ToList();
        }
    }
}