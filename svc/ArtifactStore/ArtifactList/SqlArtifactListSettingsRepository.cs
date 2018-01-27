using System.Data;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models.Xml;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace ArtifactStore.ArtifactList
{
    public class SqlArtifactListSettingsRepository : IArtifactListSettingsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlArtifactListSettingsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlArtifactListSettingsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<XmlProfileSettings> GetSettingsAsync(int itemId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemId", itemId);

            var settingsXml = await _connectionWrapper.ExecuteScalarAsync<string>(
                "GetArtifactListSettings", parameters, commandType: CommandType.StoredProcedure);

            return SerializationHelper.FromXml<XmlProfileSettings>(settingsXml);
        }

        public async Task<int> CreateSettingsAsync(int itemId, int userId, XmlProfileSettings settings)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemId", itemId);
            parameters.Add("@settings", SerializationHelper.ToXml(settings));

            return await _connectionWrapper.ExecuteScalarAsync<int>(
                "CreateArtifactListSettings", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateSettingsAsync(int itemId, int userId, XmlProfileSettings settings)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemId", itemId);
            parameters.Add("@settings", SerializationHelper.ToXml(settings));

            return await _connectionWrapper.ExecuteScalarAsync<int>(
                "UpdateArtifactListSettings", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
