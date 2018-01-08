using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using System.Net;
using ServiceLibrary.Exceptions;
using Dapper;
using System.Linq;
using System;
using ServiceLibrary.Repositories;
using System.Text;
using SearchEngineLibrary.Model;
using System.Data.SqlClient;
using SearchEngineLibrary.Helpers;

namespace SearchEngineLibrary.Repository
{
    public class SearchEngineRepository : ISearchEngineRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SearchEngineRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SearchEngineRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<SearchArtifactsResult> GetArtifactIds(int scopeId, Pagination pagination, bool includeDrafts, int userId)
        {
            var searchArtifactsResult = new SearchArtifactsResult() { ArtifactIds = new List<int>() };                     
            
            var dbConnection = _connectionWrapper.CreateConnection();

            using (var sqlDataReader = await dbConnection.ExecuteReaderAsync(QueryBuilder.GetArtifactIds(scopeId, pagination, includeDrafts, userId), commandType: CommandType.Text))
            {
                while (sqlDataReader.Read())
                {
                    searchArtifactsResult.Total = (int)sqlDataReader["Total"];
                }

                sqlDataReader.NextResult();

                while (sqlDataReader.Read())
                {
                    searchArtifactsResult.ArtifactIds = searchArtifactsResult.ArtifactIds.Union(Enumerable.Repeat((int)sqlDataReader["VersionArtifactId"], 1));
                }
            }
            return searchArtifactsResult;
        }
    }
}
