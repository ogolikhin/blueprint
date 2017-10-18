using AdminStore.Models;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.InstanceSettings;

namespace AdminStore.Repositories
{
    public class SqlSettingsRepository : SqlInstanceSettingsRepository, ISqlSettingsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlSettingsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlSettingsRepository(ISqlConnectionWrapper connectionWrapper)
            : base(connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IEnumerable<LdapSettings>> GetLdapSettingsAsync()
        {
            return await _connectionWrapper.QueryAsync<LdapSettings>("GetLdapSettings", commandType: CommandType.StoredProcedure);
        }

        public async Task<InstanceSettings> GetInstanceSettingsAsync()
        {
            return await base.GetInstanceSettingsAsync(WebApiConfig.MaximumInvalidLogonAttempts);
        }

        public async Task<IFederatedAuthenticationSettings> GetFederatedAuthenticationSettingsAsync()
        {
            var result = (await _connectionWrapper.QueryAsync<dynamic>("GetFederatedAuthentications", commandType: CommandType.StoredProcedure)).FirstOrDefault();

            return result == null ? null : new FederatedAuthenticationSettings(result.Settings, result.Certificate);
        }

        public async Task<UserManagementSettings> GetUserManagementSettingsAsync()
        {
            var instanceSettings = await GetInstanceSettingsAsync();

            return new UserManagementSettings
            {
                // Assumes no difference between disabled Federated Authentication (FA) and no FA
                IsFederatedAuthenticationEnabled = instanceSettings.IsSamlEnabled.HasValue &&
                                                   instanceSettings.IsSamlEnabled.Value,
                IsPasswordExpirationEnabled = instanceSettings.PasswordExpirationInDays > 0
            };
        }
    }
}
