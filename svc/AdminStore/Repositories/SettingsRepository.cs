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
        public virtual async Task<IEnumerable<LdapSettings>> GetLdapSettings()
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                return await cxn.QueryAsync<LdapSettings>("GetLdapSettings", commandType: CommandType.StoredProcedure);
            }
        }

        public virtual async Task<InstanceSettings> GetInstanceSettings()
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                return (await cxn.QueryAsync<InstanceSettings>("GetInstanceSettings", commandType: CommandType.StoredProcedure)).First();
            }
        }
    }
}