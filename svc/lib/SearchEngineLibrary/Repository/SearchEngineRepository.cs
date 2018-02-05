using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Linq;
using Dapper;
using ServiceLibrary.Repositories;
using SearchEngineLibrary.Model;
using SearchEngineLibrary.Helpers;

namespace SearchEngineLibrary.Repository
{
    public class SearchEngineRepository : ISearchEngineRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SearchEngineRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SearchEngineRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<SearchArtifactsResult> GetCollectionContentSearchArtifactResults(
            int scopeId, Pagination pagination, bool includeDrafts, int userId, IDbTransaction transaction)
        {
            var searchArtifactsResult = new SearchArtifactsResult { ArtifactIds = new List<int>() };
            
            Tuple<IEnumerable<int>, IEnumerable<int>> result; 

            if (transaction == null)
            {
                result = await _connectionWrapper.QueryMultipleAsync<int, int>(
                        QueryBuilder.GetCollectionContentSearchArtifactResults(scopeId, pagination, includeDrafts, userId),
                        commandType: CommandType.Text);
            }
            else
            {
                using (var command = await transaction.Connection.QueryMultipleAsync(QueryBuilder.GetCollectionContentSearchArtifactResults(scopeId, pagination, includeDrafts, userId),
                                                                                     param: null,
                                                                                     transaction: transaction,
                                                                                     commandType: CommandType.Text))
                {
                    var item1 = command.Read<int>().ToList();
                    var item2 = command.Read<int>().ToList();

                    result = new Tuple<IEnumerable<int>, IEnumerable<int>>(item1, item2);
                }
            }


            if (result.Item1 != null)
            {
                searchArtifactsResult.Total = result.Item1.FirstOrDefault();
            }

            searchArtifactsResult.ArtifactIds = result.Item2;

            return searchArtifactsResult;
        }
    }
}
