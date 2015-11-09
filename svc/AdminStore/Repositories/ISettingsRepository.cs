using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface ISettingsRepository
    {
        Task<IEnumerable<LdapSettings>> GetLdapSettings();

        Task<InstanceSettings> GetInstanceSettings();
    }
}