using AdminStore.Models;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    public interface ILdapRepository
    {
        /// <summary>
        /// Authenticate ldap user
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="instanceSettings"></param>
        /// <returns></returns>
        Task AuthenticateLdapUser(string login, string password, InstanceSettings instanceSettings = null);
    }
}