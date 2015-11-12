using System.Collections.Generic;
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
            return (await _connectionWrapper.QueryAsync<InstanceSettings>("GetInstanceSettings", commandType: CommandType.StoredProcedure)).First();
        }

        public async Task<FederatedAuthenticationSettings> GetFederatedAuthentication()
        {
            return (await _connectionWrapper.QueryAsync<FederatedAuthenticationSettings>("GetFederatedAuthentication", commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public Task<IFederatedAuthenticationSettings> GetFederatedAuthenticationSettingsAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
