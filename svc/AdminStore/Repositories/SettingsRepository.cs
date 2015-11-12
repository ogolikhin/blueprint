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
        public async Task<IEnumerable<LdapSettings>> GetLdapSettingsAsync()
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                return await cxn.QueryAsync<LdapSettings>("GetLdapSettings", commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<InstanceSettings> GetInstanceSettingsAsync()
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                return (await cxn.QueryAsync<InstanceSettings>("GetInstanceSettings", commandType: CommandType.StoredProcedure)).First();
            }
        }

        public async Task<IFederatedAuthenticationSettings> GetFederatedAuthenticationSettingsAsync()
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                var result = (await cxn.QueryAsync<dynamic>("GetFederatedAuthentication", commandType: CommandType.StoredProcedure)).FirstOrDefault();
                if (result == null)
                {
                    return null;
                }

                return new FederatedAuthenticationSettings(result.Settings, result.Certificate);
            }
        }
    }
}