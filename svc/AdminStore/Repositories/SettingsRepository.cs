using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using Dapper;

namespace AdminStore.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        public virtual async Task<IEnumerable<LdapSettings>> GetLdapSettingsAsync()
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                return await cxn.QueryAsync<LdapSettings>("GetLdapSettings", commandType: CommandType.StoredProcedure);
            }
        }

        public virtual async Task<InstanceSettings> GetInstanceSettingsAsync()
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                return (await cxn.QueryAsync<InstanceSettings>("GetInstanceSettings", commandType: CommandType.StoredProcedure)).First();
            }
        }

        public virtual async Task<FederatedAuthenticationSettings> GetFederatedAuthentication()
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                return (await cxn.QueryAsync<FederatedAuthenticationSettings>("GetFederatedAuthentication", commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
        }

        public Task<IFederatedAuthenticationSettings> GetFederatedAuthenticationSettingsAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}