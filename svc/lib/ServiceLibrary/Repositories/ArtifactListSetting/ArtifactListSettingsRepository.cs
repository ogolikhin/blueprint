using System.Data;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories.ArtifactListSetting
{
    public class ArtifactListSettingsRepository : IArtifactListSettingsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public ArtifactListSettingsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public ArtifactListSettingsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<int> CreateArtifactListSettingsAsync(int itemId, int userId, string settings)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemId", itemId);
            parameters.Add("@settings", settings);

            return await _connectionWrapper.ExecuteScalarAsync<int>("CreateArtifactListSettings", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateArtifactListSettingsAsync(int itemId, int userId, string settings)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemId", itemId);
            parameters.Add("@settings", settings);

            return await _connectionWrapper.ExecuteScalarAsync<int>("UpdateArtifactListSettings", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
