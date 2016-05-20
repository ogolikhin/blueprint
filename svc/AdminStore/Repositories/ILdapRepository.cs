using System.Threading.Tasks;
using AdminStore.Helpers;

namespace AdminStore.Repositories
{
    public interface ILdapRepository
    {
        /// <summary>
        /// Authenticate ldap user
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="useDefaultConnection"></param>
        /// <returns></returns>
        Task<AuthenticationStatus> AuthenticateLdapUserAsync(string login, string password, bool useDefaultConnection);
    }
}