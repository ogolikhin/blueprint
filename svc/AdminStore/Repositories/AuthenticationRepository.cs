using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IUserRepository _userRepository;

        private readonly ISettingsRepository _settingsRepository;

        private readonly ILdapRepository _ldapRepository;

        private readonly int? _maximumInvalidLogonAttempts;

        public AuthenticationRepository(): this(new UserRepository(), new SettingsRepository(), new LdapRepository())
        {
        }

        public AuthenticationRepository(IUserRepository userRepository, ISettingsRepository settingsRepository, ILdapRepository ldapRepository)
        {
            _userRepository = userRepository;
            _settingsRepository = settingsRepository;
            _ldapRepository = ldapRepository;
            int value;
            if (int.TryParse(WebApiConfig.MaximumInvalidLogonAttempts, out value))
            {
                _maximumInvalidLogonAttempts = value;
            }
        }

        public async Task<LoginUser> AuthenticateUser(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                throw new InvalidCredentialException("Login or password cannot be empty");
            }
            var user = await _userRepository.GetUserByLogin(login);
            if (user == null)
            {
                throw new InvalidCredentialException(string.Format("User does not exists with login: {0}", login));
            }
            var instanceSettings = await _settingsRepository.GetInstanceSettings();
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
                    await _ldapRepository.AuthenticateLdapUser(login, password, instanceSettings);
                    break;
                default:
                    throw new AuthenticationException(string.Format("Authentication provider could not be found for login: {0}", login),
                                                    new ArgumentOutOfRangeException(user.Source.ToString()));
            }
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
            if (user == null || !user.IsEnabled || _maximumInvalidLogonAttempts == null)
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

                if (user.InvalidLogonAttemptsNumber >= _maximumInvalidLogonAttempts.Value)
                {
                    user.IsEnabled = false;
                }
            }

            user.LastInvalidLogonTimeStamp = DateTime.UtcNow;
            await _userRepository.UpdateUserOnInvalidLogin(user);
        }
    }
}