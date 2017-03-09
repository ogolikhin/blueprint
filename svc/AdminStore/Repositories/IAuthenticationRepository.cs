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
        Task<AuthenticationUser> AuthenticateUserAsync(string login, string password);

        /// <summary>
        /// Authenticates SAML user
        /// </summary>
        /// <returns>An <see cref="P:AdminStore.Models.LoginUser"/> object that specifies the authentificated user.</returns>
        /// <exception cref="T:System.Security.Authentication.AuthenticationException">Thrown when the system fails to authentificate an user.</exception>
        Task<AuthenticationUser> AuthenticateSamlUserAsync(string samlResponse);

        /// <summary>
        /// Authenticates user with provided credentials
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns>An <see cref="P:AdminStore.Models.LoginUser"/> object that specifies the authentificated user.</returns>
        /// <exception cref="T:System.Security.Authentication.AuthenticationException">Thrown when the system fails to authentificate an user.</exception>
        Task<AuthenticationUser> AuthenticateUserForResetAsync(string login, string password);

        /// <summary>
        /// Resets the password for the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <exception cref="T:System.Security.Authentication.AuthenticationException">Thrown when the system fails to authentificate an user.</exception>
        Task ResetPassword(AuthenticationUser user, string oldPassword, string newPassword);

        /// <summary>
        /// Verifies whether the user cannot change their password due to change cooldown
        /// </summary>
        /// <param name="user"></param>
        Task<bool> IsChangePasswordCooldownInEffect(AuthenticationUser user);
    }
}
