using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    public interface ISqlSettingsRepository
    {
        Task<IEnumerable<LdapSettings>> GetLdapSettingsAsync();

        Task<InstanceSettings> GetInstanceSettingsAsync();

        Task<IFederatedAuthenticationSettings> GetFederatedAuthenticationSettingsAsync();

        Task<UserManagementSettings> GetUserManagementSettingsAsync();
    }
}