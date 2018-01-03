using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

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

        public Task<IEnumerable<int>> GetArtifactIds()
        {
            return _connectionWrapper.QueryAsync<int>(@"SELECT DISTINCT([ArtifactId]) FROM [dbo].[SearchItems]", commandType: CommandType.Text);

        }
    }
}
