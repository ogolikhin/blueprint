using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ApplicationSettings
{
    public class SqlFeaturesRepository : IFeaturesRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlFeaturesRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlFeaturesRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IEnumerable<Feature>> GetFeaturesAsync(bool includeExpired = false)
        {
            var prm = new DynamicParameters();
            prm.Add("@includeExpired", includeExpired);

            var features = (await _connectionWrapper.QueryAsync<Feature>("[dbo].[GetFeatures]", prm, commandType: CommandType.StoredProcedure));

            return features;
        }
    }
}
