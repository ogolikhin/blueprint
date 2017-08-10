using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories.InstanceSettings
{
    public class SqlInstanceSettingsRepository : SqlBaseArtifactRepository, IInstanceSettingsRepository
    {
        public SqlInstanceSettingsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
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
    }
}
