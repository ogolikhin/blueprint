using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories.InstanceSettings
{
    public class SqlInstanceSettingsRepository : SqlBaseArtifactRepository, IInstanceSettingsRepository
    {
        public SqlInstanceSettingsRepository() : this (new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
            
        }

        public SqlInstanceSettingsRepository(string connectionString) : this(new SqlConnectionWrapper(connectionString))
        {
        }

        public SqlInstanceSettingsRepository(ISqlConnectionWrapper connectionWrapper) : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlInstanceSettingsRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository) : 
            base(connectionWrapper, artifactPermissionsRepository)
        {
        }

        public async Task<EmailSettings> GetEmailSettings()
        {
            var result = (await ConnectionWrapper.QueryAsync<dynamic>("GetInstanceEmailSettings", commandType: CommandType.StoredProcedure)).FirstOrDefault();
            return result == null ? null : EmailSettings.CreateFromString(result.EmailSettings);
        }

        public Task UpdateEmailSettingsAsync(EmailSettings emailSettings)
        {
            string emailSettingsXmlString = SerializationHelper.Serialize(emailSettings);

            var parameters = new DynamicParameters();

            parameters.Add("@emailSettingsXml", emailSettingsXmlString);

            return ConnectionWrapper.ExecuteAsync("UpdateInstanceEmailSettings", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Models.InstanceSettings> GetInstanceSettingsAsync(int maxInvalidLogonAttempts)
        {
            var settings = (await ConnectionWrapper.QueryAsync<Models.InstanceSettings>("GetInstanceSettings", commandType: CommandType.StoredProcedure)).First();
            if (!string.IsNullOrEmpty(settings.EmailSettings))
            {
                settings.EmailSettingsDeserialized = new EmailConfigInstanceSettings(settings.EmailSettings);
            }

            //TODO temporary solution, MaximumInvalidLogonAttempts property should be moved to database
            settings.MaximumInvalidLogonAttempts = maxInvalidLogonAttempts;

            return settings;
        }

        public async Task<int> CheckMaxArtifactsPerProjectBoundary(int projectId)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@projectId ", projectId);

            var result = await ConnectionWrapper.QueryAsync<int>("CheckMaxArtifactsPerProjectBoundary", parameters, commandType: CommandType.StoredProcedure);
            return result.First();
        }
    }

    public enum BoundaryLimit
    {
        Normal = 0,
        ReachingBoundary = 1,
        BoundaryReached = 2
    }
}
