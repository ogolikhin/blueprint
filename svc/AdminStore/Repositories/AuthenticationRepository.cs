using System;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Saml;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly ISqlUserRepository _userRepository;

        private readonly ISqlSettingsRepository _settingsRepository;

        private readonly ILdapRepository _ldapRepository;

        private readonly ISamlRepository _samlRepository;

        private readonly IServiceLogRepository _log;

        public AuthenticationRepository()
            : this(new SqlUserRepository(), new SqlSettingsRepository(), new LdapRepository(), new SamlRepository(), new ServiceLogRepository())
        {
        }

        public AuthenticationRepository(ISqlUserRepository userRepository, ISqlSettingsRepository settingsRepository, ILdapRepository ldapRepository, ISamlRepository samlRepository, IServiceLogRepository logRepository)
        {
            _userRepository = userRepository;
            _settingsRepository = settingsRepository;
            _ldapRepository = ldapRepository;
            _samlRepository = samlRepository;
            _log = logRepository;
        }

        public async Task<AuthenticationUser> AuthenticateUserAsync(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                throw new FormatException("Login or password cannot be empty");
            }
            var user = await _userRepository.GetUserByLoginAsync(login);
            if (user == null)
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, I18NHelper.FormatInvariant("Could not get user with login '{0}'", login));
                throw new AuthenticationException("Invalid username or password", ErrorCodes.InvalidCredentials);
            }
            var instanceSettings = await _settingsRepository.GetInstanceSettingsAsync();
            if (instanceSettings.IsSamlEnabled.GetValueOrDefault())
            {
                // Fallback is allowed by default (value is null)
                if (!user.IsFallbackAllowed.GetValueOrDefault(true))
                {
                    throw new AuthenticationException("User must be authenticated via Federated Authentication mechanism", ErrorCodes.FallbackIsDisabled);
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
                        throw new AuthenticationException(string.Format("To authenticate user with login: {0}, ldap integration must be enabled", login), ErrorCodes.LdapIsDisabled);
                    }
                    authenticationStatus = await _ldapRepository.AuthenticateLdapUserAsync(login, password, instanceSettings.UseDefaultConnection);
                    break;
                default:
                    throw new AuthenticationException(string.Format("Authentication provider could not be found for login: {0}", login));
            }
            await ProcessAuthenticationStatus(authenticationStatus, user, instanceSettings);

            user.LicenseType = await _userRepository.GetEffectiveUserLicenseAsync(user.Id);

            return user;
        }

        private async Task ProcessAuthenticationStatus(AuthenticationStatus authenticationStatus, AuthenticationUser user, InstanceSettings instanceSettings)
        {
            switch (authenticationStatus)
            {
                case AuthenticationStatus.Success:
                    if (!user.IsEnabled)
                    {
                        throw new AuthenticationException(string.Format("User account is locked out for the login: {0}", user.Login), ErrorCodes.AccountIsLocked);
                    }
                    await ResetInvalidLogonAttemptsNumber(user);
                    break;
                case AuthenticationStatus.InvalidCredentials:
                    await LockUserIfApplicable(user, instanceSettings);
                    await _log.LogInformation(WebApiConfig.LogSourceSessions, I18NHelper.FormatInvariant("Invalid password for user '{0}'", user.Login));
                    throw new AuthenticationException("Invalid username or password", ErrorCodes.InvalidCredentials);
                case AuthenticationStatus.PasswordExpired:
                    throw new AuthenticationException(string.Format("User password expired for the login: {0}", user.Login), ErrorCodes.PasswordExpired);
                case AuthenticationStatus.Locked:
                    throw new AuthenticationException(string.Format("User account is locked out for the login: {0}", user.Login), ErrorCodes.AccountIsLocked);
                case AuthenticationStatus.Error:
                    throw new AuthenticationException("Authentication Error");
            }
        }

        public async Task<AuthenticationUser> AuthenticateSamlUserAsync(string samlResponse)
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

            if (user == null) // cannot find user in the DB
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, I18NHelper.FormatInvariant("Could not get user with login '{0}'. NameClaimType '{1}'", principal.Identity.Name, fedAuthSettings.NameClaimType));
                throw new AuthenticationException("Invalid user name or password", ErrorCodes.InvalidCredentials);
            }
            if (!user.IsEnabled)
            {
                throw new AuthenticationException(I18NHelper.FormatInvariant("Your account '{0}' has been locked out, please contact your Blueprint Instance Administrator.", user.Login), ErrorCodes.AccountIsLocked);
            }

            user.LicenseType = await _userRepository.GetEffectiveUserLicenseAsync(user.Id);

            return user;
        }

        private AuthenticationStatus AuthenticateDatabaseUser(AuthenticationUser user, string password, int passwordExpirationInDays = 0)
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

        private bool HasExpiredPassword(AuthenticationUser user, int passwordExpirationInDays)
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

        private async Task LockUserIfApplicable(AuthenticationUser user, InstanceSettings instanceSettings)
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

        private async Task ResetInvalidLogonAttemptsNumber(AuthenticationUser user)
        {            
            if (user.InvalidLogonAttemptsNumber > 0)
            {                                
                user.InvalidLogonAttemptsNumber = 0;
                user.LastInvalidLogonTimeStamp = null;
                await _userRepository.UpdateUserOnInvalidLoginAsync(user);
            }                        
        }
    }
}