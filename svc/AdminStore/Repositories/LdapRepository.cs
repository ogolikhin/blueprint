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
    public class LdapRepository : ILdapRepository
    {
        private readonly ISettingsRepository _settingsRepository;

        private const int LdapInvalidCredentialsErrorCode = 49;

        private const int ActiveDirectoryInvalidCredentialsErrorCode = -2147023570;

        public LdapRepository()
            : this(new SettingsRepository())
        {

        }

        internal LdapRepository(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task AuthenticateLdapUser(string login, string password, InstanceSettings instanceSettings)
        {
            var authenticationStatus = AuthenticationStatus.Error;
            if (!instanceSettings.IsLdapIntegrationEnabled)
            {
                throw new AuthenticationException(string.Format("To authenticate user with login: {0}, ldap integration should be enabled", login));
            }
            var loginInfo = LoginInfo.Parse(login);
            if (instanceSettings.UseDefaultConnection)
            {
                authenticationStatus = TryAuthenticateViaDirectorySearcher(loginInfo.Login, password);
            }
            else
            {
                var ldapSettings = await _settingsRepository.GetLdapSettings();
                if (ldapSettings.Any())
                {
                    foreach (var ldapSetting in ldapSettings.OrderByDescending(s => s.MatchsUser(loginInfo.Domain)))
                    {
                        if (!UserExistsInLdapDirectory(ldapSetting, loginInfo))
                        {
                            continue;
                        }
                        authenticationStatus = TryAuthenticate(loginInfo, password, ldapSetting.AuthenticationType);
                        if (authenticationStatus == AuthenticationStatus.Success)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    authenticationStatus = TryAuthenticate(loginInfo, password);
                }
            }
            if (authenticationStatus != AuthenticationStatus.Success)
            {
                if (authenticationStatus == AuthenticationStatus.InvalidCredentials)
                {
                    throw new InvalidCredentialException("Invalid username or password");
                }
                throw new AuthenticationException("The LDAP server is unavailable");
            }
        }

        private AuthenticationStatus TryAuthenticate(LoginInfo loginInfo, string password, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
        {
            var authenticationStatus = TryAuthenticateViaLdap(loginInfo, password, authenticationType);
            if (authenticationStatus != AuthenticationStatus.Success)
            {
                return TryAuthenticateViaDirectorySearcher(loginInfo.Login, password);
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
                if (comException.ErrorCode == ActiveDirectoryInvalidCredentialsErrorCode)
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
                    var networkCredential = new NetworkCredential(loginInfo.UserName, password, loginInfo.Domain);
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
            var userName = loginInfo.UserName != null ? loginInfo.UserName.Trim() : loginInfo.Login;
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