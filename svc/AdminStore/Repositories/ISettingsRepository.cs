using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface ISettingsRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<LdapSettings>> GetLdapSettingsAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<InstanceSettings> GetInstanceSettingsAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IFederatedAuthenticationSettings> GetFederatedAuthenticationSettingsAsync();
    }
}