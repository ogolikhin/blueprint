﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    public class SqlSettingsRepository : ISqlSettingsRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlSettingsRepository()
            : this(new SqlConnectionWrapper(WebApiConfig.RaptorMain))
        {
        }

        internal SqlSettingsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IEnumerable<LdapSettings>> GetLdapSettingsAsync()
        {
            return await _connectionWrapper.QueryAsync<LdapSettings>("GetLdapSettings", commandType: CommandType.StoredProcedure);
        }

        public async Task<InstanceSettings> GetInstanceSettingsAsync()
        {
            var settings = (await _connectionWrapper.QueryAsync<InstanceSettings>("GetInstanceSettings", commandType: CommandType.StoredProcedure)).First();
            //TODO temporary solution, MaximumInvalidLogonAttempts property should be moved to database
            settings.MaximumInvalidLogonAttempts = WebApiConfig.MaximumInvalidLogonAttempts;
            return settings;
        }

        public async Task<IFederatedAuthenticationSettings> GetFederatedAuthenticationSettingsAsync()
        {
            var result = (await _connectionWrapper.QueryAsync<dynamic>("GetFederatedAuthentications", commandType: CommandType.StoredProcedure)).FirstOrDefault();
            return result == null ? null : new FederatedAuthenticationSettings(result.Settings, result.Certificate);
        }
    }
}
