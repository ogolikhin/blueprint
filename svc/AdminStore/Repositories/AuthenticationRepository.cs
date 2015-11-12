using System;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Saml;

namespace AdminStore.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly ISqlUserRepository _userRepository;

        private readonly ISqlSettingsRepository _settingsRepository;

        private readonly ILdapRepository _ldapRepository;

        public AuthenticationRepository(): this(new SqlUserRepository(), new SqlSettingsRepository(), new LdapRepository())
        {
        }

        public AuthenticationRepository(ISqlUserRepository userRepository, ISqlSettingsRepository settingsRepository, ILdapRepository ldapRepository)
        {
            _userRepository = userRepository;
            _settingsRepository = settingsRepository;
            _ldapRepository = ldapRepository;
        }

        public async Task<LoginUser> AuthenticateUserAsync(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                throw new InvalidCredentialException("Login or password cannot be empty");
            }
            var user = await _userRepository.GetUserByLoginAsync(login);
            if (user == null)
            {
                throw new InvalidCredentialException(string.Format("User does not exists with login: {0}", login));
            }
            var instanceSettings = await _settingsRepository.GetInstanceSettingsAsync();
            if (instanceSettings.IsSamlEnabled.GetValueOrDefault() && !user.IsFallbackAllowed.GetValueOrDefault())
            {
                throw new AuthenticationException("User must be authenticated via Federated Authentication mechanism");
            }
            switch (user.Source)
            {
                case UserGroupSource.Database:
                    await AuthenticateDatabaseUser(user, password, instanceSettings.PasswordExpirationInDays);
                    break;
                case UserGroupSource.Windows:
                    await _ldapRepository.AuthenticateLdapUserAsync(login, password, instanceSettings);
                    break;
                default:
                    throw new AuthenticationException(string.Format("Authentication provider could not be found for login: {0}", login),
                                                    new ArgumentOutOfRangeException(user.Source.ToString()));
            }
            return user;
        }

        public async Task<LoginUser> AuthenticateSamlUserAsync(string samlResponse)
        {
            var instanceSettings = await _settingsRepository.GetInstanceSettingsAsync();
            if (!instanceSettings.IsSamlEnabled.GetValueOrDefault())
            {
                throw new AuthenticationException("Federated Authentication mechanism must be enabled");
            }
            var fedAuthSettings = await _settingsRepository.GetFederatedAuthenticationSettingsAsync();

            var responseDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(HttpUtility.HtmlDecode(samlResponse)));

            var principal = SamlUtil.ProcessResponse(responseDecoded, fedAuthSettings);
            var user = await _userRepository.GetUserByLoginAsync(principal.Identity.Name);
            return user;
        }

        private async Task AuthenticateDatabaseUser(LoginUser user, string password, int passwordExpirationInDays = 0)
        {
            var hashedPassword = HashingUtilities.GenerateSaltedHash(password, user.UserSalt);
            if (!string.Equals(user.Password, hashedPassword))
            {
                await LockUserIfApplicable(user);
                throw new InvalidCredentialException("Invalid username or password");
            }
            if (!user.IsEnabled)
            {
                throw new AuthenticationException(string.Format("User account is locked out for the login: {0}", user.Login));
            }
            if (passwordExpirationInDays > 0)
            {
                if (HasExpiredPassword(user, passwordExpirationInDays))
                {
                    throw new AuthenticationException(string.Format("User password expired for the login: {0}", user.Login));
                }
            }
        }

        private bool HasExpiredPassword(LoginUser user, int passwordExpirationInDays)
        {
            if (!user.ExpirePassword.GetValueOrDefault())
            {
                return false;
            }

            if (!user.LastPasswordChangeTimestamp.HasValue)
            {
                return false;
            }

            // If the value is 0 then password never expires
            if (passwordExpirationInDays == 0)
            {
                return false;
            }

            var currentUtcTime = DateTime.UtcNow;
            var hasExpiredPassword = user.LastPasswordChangeTimestamp.Value.AddDays(passwordExpirationInDays) <= currentUtcTime;

            return hasExpiredPassword;
        }

        private async Task LockUserIfApplicable(LoginUser user)
        {
            if (user == null || !user.IsEnabled || WebApiConfig.MaximumInvalidLogonAttempts == 0)
            {
                return;
            }

            if (user.InvalidLogonAttemptsNumber > 0 && user.LastInvalidLogonTimeStamp != null && DateTime.UtcNow > ((DateTime)user.LastInvalidLogonTimeStamp).AddHours(24))
            {
                //We don't lock the user if she/he tries to login after 24 hours of the last failure attempt
                //after 24 hours we reset the counter and let the user to try invalid logon till reaches the limit

                user.InvalidLogonAttemptsNumber = 1;
            }
            else
            {
                user.InvalidLogonAttemptsNumber++;

                if (user.InvalidLogonAttemptsNumber >= WebApiConfig.MaximumInvalidLogonAttempts)
                {
                    user.IsEnabled = false;
                }
            }

            user.LastInvalidLogonTimeStamp = DateTime.UtcNow;
            await _userRepository.UpdateUserOnInvalidLoginAsync(user);
        }
    }
}