using System;
using System.Security.Authentication;
using System.Threading.Tasks;
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

        private readonly ISamlRepository _samlRepository;

        public AuthenticationRepository()
            : this(new SqlUserRepository(), new SqlSettingsRepository(), new LdapRepository(), new SamlRepository())
        {
        }

        public AuthenticationRepository(ISqlUserRepository userRepository, ISqlSettingsRepository settingsRepository, ILdapRepository ldapRepository, ISamlRepository samlRepository)
        {
            _userRepository = userRepository;
            _settingsRepository = settingsRepository;
            _ldapRepository = ldapRepository;
            _samlRepository = samlRepository;
        }

        public async Task<LoginUser> AuthenticateUserAsync(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                throw new FormatException("Login or password cannot be empty");
            }
            var user = await _userRepository.GetUserByLoginAsync(login);
            if (user == null)
            {
                throw new InvalidCredentialException(string.Format("User does not exist with login: {0}", login));
            }
            var instanceSettings = await _settingsRepository.GetInstanceSettingsAsync();
            if (instanceSettings.IsSamlEnabled.GetValueOrDefault())
            {
				// Fallback is allowed by default (value is null)
	            var isFallbackAllowed = !user.IsFallbackAllowed.HasValue || user.IsFallbackAllowed.Value;
				if(!isFallbackAllowed)
				{
					throw new AuthenticationException("User must be authenticated via Federated Authentication mechanism");
				}
            }
            AuthenticationStatus authenticationStatus;
            switch (user.Source)
            {
                case UserGroupSource.Database:
                    authenticationStatus = AuthenticateDatabaseUser(user, password, instanceSettings.PasswordExpirationInDays);
                    break;
                case UserGroupSource.Windows:
                    if (!instanceSettings.IsLdapIntegrationEnabled)
                    {
                        throw new AuthenticationException(string.Format("To authenticate user with login: {0}, ldap integration must be enabled", login));
                    }
                    authenticationStatus = await _ldapRepository.AuthenticateLdapUserAsync(login, password, instanceSettings.UseDefaultConnection);
                    break;
                default:
                    throw new AuthenticationException(string.Format("Authentication provider could not be found for login: {0}", login),
                                                    new ArgumentOutOfRangeException(user.Source.ToString()));
            }
            await ProcessAuthenticationStatus(authenticationStatus, user, instanceSettings);
            return user;
        }

        private async Task ProcessAuthenticationStatus(AuthenticationStatus authenticationStatus, LoginUser user, InstanceSettings instanceSettings)
        {
            switch (authenticationStatus)
            {
                case AuthenticationStatus.Success:
                    break;
                case AuthenticationStatus.InvalidCredentials:
                    await LockUserIfApplicable(user, instanceSettings);
                    throw new InvalidCredentialException("Invalid username or password");
                case AuthenticationStatus.PasswordExpired:
                    throw new AuthenticationException(string.Format("User password expired for the login: {0}", user.Login));
                case AuthenticationStatus.Locked:
                    throw new AuthenticationException(string.Format("User account is locked out for the login: {0}", user.Login));
                case AuthenticationStatus.Error:
                    throw new AuthenticationException();
            }
        }

        public async Task<LoginUser> AuthenticateSamlUserAsync(string samlResponse)
        {
            if (string.IsNullOrEmpty(samlResponse))
            {
                throw new FormatException("Saml response cannot be empty");
            }
            var instanceSettings = await _settingsRepository.GetInstanceSettingsAsync();
            if (!instanceSettings.IsSamlEnabled.GetValueOrDefault())
            {
                throw new AuthenticationException("Federated Authentication mechanism must be enabled");
            }
            var fedAuthSettings = await _settingsRepository.GetFederatedAuthenticationSettingsAsync();
            if (fedAuthSettings == null)
            {
                throw new AuthenticationException("Federated Authentication settings must be provided");
            }

            var principal = _samlRepository.ProcessEncodedResponse(samlResponse, fedAuthSettings);
            var user = await _userRepository.GetUserByLoginAsync(principal.Identity.Name);
            return user;
        }

        private AuthenticationStatus AuthenticateDatabaseUser(LoginUser user, string password, int passwordExpirationInDays = 0)
        {
            var hashedPassword = HashingUtilities.GenerateSaltedHash(password, user.UserSalt);
            if (!string.Equals(user.Password, hashedPassword))
            {
                return AuthenticationStatus.InvalidCredentials;
            }
            if (!user.IsEnabled)
            {
                return AuthenticationStatus.Locked;
            }
            if (HasExpiredPassword(user, passwordExpirationInDays))
            {
                return AuthenticationStatus.PasswordExpired;
            }
            return AuthenticationStatus.Success;
        }

        private bool HasExpiredPassword(LoginUser user, int passwordExpirationInDays)
        {
            // If the value is 0 then password never expires
            if (passwordExpirationInDays <= 0)
            {
                return false;
            }

            if (!user.ExpirePassword.GetValueOrDefault())
            {
                return false;
            }

            if (!user.LastPasswordChangeTimestamp.HasValue)
            {
                return false;
            }

            var currentUtcTime = DateTime.UtcNow;
            var hasExpiredPassword = user.LastPasswordChangeTimestamp.Value.AddDays(passwordExpirationInDays) <= currentUtcTime;

            return hasExpiredPassword;
        }

        private async Task LockUserIfApplicable(LoginUser user, InstanceSettings instanceSettings)
        {
            if (instanceSettings.MaximumInvalidLogonAttempts <= 0)
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

                if (user.InvalidLogonAttemptsNumber >= instanceSettings.MaximumInvalidLogonAttempts)
                {
                    user.IsEnabled = false;
                }
            }

            user.LastInvalidLogonTimeStamp = DateTime.UtcNow;
            await _userRepository.UpdateUserOnInvalidLoginAsync(user);
        }
    }
}