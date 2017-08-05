using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories.InstanceSettings
{
    public class SqlInstanceSettingsRepository : IInstanceSettingsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlInstanceSettingsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        protected SqlInstanceSettingsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<EmailSettings> GetEmailSettings()
        {
            var result = (await _connectionWrapper.QueryAsync<dynamic>("GetInstanceEmailSettings", commandType: CommandType.StoredProcedure)).FirstOrDefault();
            return result == null ? null : EmailSettings.CreateFromString(result.EmailSettings);
        }

        public async Task<Models.InstanceSettings> GetInstanceSettingsAsync(int maxInvalidLogonAttempts)
        {
            var settings = (await _connectionWrapper.QueryAsync<Models.InstanceSettings>("GetInstanceSettings", commandType: CommandType.StoredProcedure)).First();
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
