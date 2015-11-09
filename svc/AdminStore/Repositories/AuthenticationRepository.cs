using System;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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

        private readonly int? _maximumInvalidLogonAttempts;

        private const int AdServerNotOperationalErrorCode = -2147016646;

        private const int LdapInvalidCredentialsErrorCode = -2147023570;

        public AuthenticationRepository(): this(new UserRepository(), new SettingsRepository())
        {
        }

        public AuthenticationRepository(IUserRepository userRepository, ISettingsRepository settingsRepository)
        {
            _userRepository = userRepository;
            _settingsRepository = settingsRepository;
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
                throw new InvalidCredentialException("User cannot be found");
            }
            var instanceSettings = await _settingsRepository.GetInstanceSettings();
            switch (user.Source)
            {
                case UserGroupSource.Database:
                    await AuthenticateDatabaseUser(user, password, instanceSettings.PasswordExpirationInDays);
                    break;
                case UserGroupSource.Windows:
                    if (!instanceSettings.EnableLDAPIntegration)
                    {
                        //TODO message
                        throw new AuthenticationException();

                    }
                    await AuthenticateLdapUser(login, password, instanceSettings);
                    break;
                default:
                    throw new AuthenticationException("", new ArgumentOutOfRangeException());
            }
            return user;
        }

        private async Task AuthenticateDatabaseUser(LoginUser user, string password, int passwordExpirationInDays = 0)
        {
            var hashedPassword = HashingUtilities.GenerateSaltedHash(password, user.UserSalt);
            if (!string.Equals(user.Password, hashedPassword))
            {
                await LockUserIfApplicable(user);
                //TODO exception message
                throw new InvalidCredentialException("");
            }
            if (passwordExpirationInDays > 0)
            {
                if (HasExpiredPassword(user, passwordExpirationInDays))
                {
                    throw new AuthenticationException("Password expired");
                }
            }
        }

        private bool HasExpiredPassword(LoginUser user, int passwordExpirationInDays)
        {
            if (!user.ExpirePassword.GetValueOrDefault())
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

        private async Task AuthenticateLdapUser(string domainUserName, string password, InstanceSettings instanceSettings)
        {
            var loginInfo = LoginInfo.Parse(domainUserName);
            if (!instanceSettings.UseDefaultConnection)
            {
                TryAuthenticate(loginInfo, password);
            }

            var ldapSettings = await _settingsRepository.GetLdapSettings();
            foreach (var ldapSetting in ldapSettings.OrderByDescending(s => s.MatchsUser(loginInfo.Domain)))
            {
                if (!UserExistsInLdapDirectory(ldapSetting, loginInfo))
                    continue;
                var authStatus = TryAuthenticate(loginInfo, password, ldapSetting.AuthenticationType);
                if (authStatus == AuthenticationStatus.Error)
                {
                    continue;
                }

                return;
            }
        }

        private AuthenticationStatus TryAuthenticate(LoginInfo loginInfo, string password, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
        {
            var authenticationStatus = TryAuthenticateViaLdap(loginInfo, password, authenticationType);
            if (authenticationStatus == AuthenticationStatus.Error)
            {
                return TryAuthenticateViaDirectorySearcher(loginInfo.DomainUserName, password);
            }
            return AuthenticationStatus.Success;
        }

        private AuthenticationStatus TryAuthenticateViaDirectorySearcher(string userName, string password, string path = null, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
        {
            try
            {
                using (var searchRoot = new DirectoryEntry(path, userName, password, authenticationType))
                {
                    using (var searcher = new DirectorySearcher(searchRoot))
                    {
                        searcher.CacheResults = false;
                        var result = searcher.FindOne();
                        if (result == null)
                        {
                            return AuthenticationStatus.Error;
                        }
                    }
                }
            }
            catch (COMException comException)
            {
                if (comException.ErrorCode == LdapInvalidCredentialsErrorCode)
                {
                    return AuthenticationStatus.InvalidCredentials;
                }
                return AuthenticationStatus.Error;
            }
            catch
            {
                return AuthenticationStatus.Error;
            }

            return AuthenticationStatus.Success;
        }

        private AuthenticationStatus TryAuthenticateViaLdap(LoginInfo loginInfo, string password, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
        {
            try
            {
                using (var ldapConnection = new LdapConnection(loginInfo.Domain))
                {
                    var networkCredential = new NetworkCredential(loginInfo.UserName, password, loginInfo.UserName);
                    ldapConnection.SessionOptions.SecureSocketLayer = authenticationType.HasFlag(AuthenticationTypes.SecureSocketsLayer);
                    ldapConnection.AuthType = AuthType.Negotiate;
                    ldapConnection.Bind(networkCredential);
                }
            }
            catch (LdapException ldapException)
            {
                if (ldapException.ErrorCode == LdapInvalidCredentialsErrorCode)
                {
                    return AuthenticationStatus.InvalidCredentials;
                }
                return AuthenticationStatus.Error;
            }
            catch
            {
                return AuthenticationStatus.Error;
            }
            return AuthenticationStatus.Success;
        }

        private bool UserExistsInLdapDirectory(LdapSettings ldapSettings, LoginInfo loginInfo)
        {
            var userName = loginInfo.UserName != null ? loginInfo.UserName.Trim() : loginInfo.DomainUserName;
            var filter = string.Format("(&(objectCategory=user)({0}={1}))", ldapSettings.GetEffectiveAccountNameAttribute(), LdapHelper.EscapeLdapSearchFilter(userName));
            try
            {
                using (var directoryEntry = ldapSettings.CreateDirectoryEntry())
                {
                    using (var dirSearch = new DirectorySearcher(directoryEntry))
                    {
                        dirSearch.Filter = filter;
                        dirSearch.PropertyNamesOnly = false;
                        dirSearch.ReferralChasing = ReferralChasingOption.None;

                        if (dirSearch.FindOne() == null)
                        {
                            //TODO logging
                            //Log.Debug(string.Format("User '{0}' is not found in LDAP directory {1}", domainUserName, ldapSettings.LdapAuthenticationUrl));
                            return false;
                        }

                        return true;
                    }
                }
            }
            catch
            {
                //TODO logging
                //Log.Debug(string.Format("Error while searching a user in LDAP directory {0} - {1}", ldapSettings.LdapAuthenticationUrl, ex.Message));
                //Log.Debug(ex);
                return false;
            }
        }
    }
}