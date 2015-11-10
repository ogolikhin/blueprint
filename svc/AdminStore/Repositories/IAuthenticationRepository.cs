using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IAuthenticationRepository
    {
        /// <summary>
        /// Authenticates user with provided credentials
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns>An <see cref="P:AdminStore.Models.LoginUser"/> object that specifies the authentificated user.</returns>
        /// <exception cref="T:System.Security.Authentication.AuthenticationException">Thrown when the system fails to authentificate an user.</exception>
        Task<LoginUser> AuthenticateUser(string login, string password);
    }
}
