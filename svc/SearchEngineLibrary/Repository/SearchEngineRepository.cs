using System.Collections.Generic;
using ServiceLibrary.Repositories;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SearchEngineLibrary.Model;
using ServiceLibrary.Helpers;

namespace SearchEngineLibrary.Repository
{
    public class SearchEngineRepository: ISearchEngineRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly ISqlHelper _sqlHelper;

        public SearchEngineRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlHelper())
        {
        }

        internal SearchEngineRepository(ISqlConnectionWrapper connectionWrapper, ISqlHelper sqlHelper)
        {
            _connectionWrapper = connectionWrapper;
            _sqlHelper = sqlHelper;
        }

        public Task<IEnumerable<int>> GetArtifactIdsFromSearchItems()
        {
            return _connectionWrapper.QueryAsync<int>(
                @"SELECT DISTINCT([ArtifactId]) FROM [dbo].[SearchItems]", commandType:CommandType.Text);
        }
    }
}