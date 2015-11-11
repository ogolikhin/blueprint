using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface ISettingsRepository
    {
        Task<IEnumerable<LdapSettings>> GetLdapSettingsAsync();

        Task<InstanceSettings> GetInstanceSettingsAsync();

        Task<IFederatedAuthenticationSettings> GetFederatedAuthenticationSettingsAsync();
    }
}