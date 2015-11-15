using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IAuthenticator
    {
        AuthenticationStatus Authenticate(LoginInfo loginInfo, string password, AuthenticationTypes authenticationType = AuthenticationTypes.Secure);
        AuthenticationStatus AuthenticateViaDirectory(LoginInfo loginInfo, string password);
        bool UserExistsInLdapDirectory(LdapSettings ldapSettings, LoginInfo loginInfo);
    }

    public class LdapRepository : ILdapRepository, IAuthenticator
    {
        internal readonly ISqlSettingsRepository _settingsRepository;
        internal readonly IAuthenticator _authenticator;

        private const int LdapInvalidCredentialsErrorCode = 49;

        private const int ActiveDirectoryInvalidCredentialsErrorCode = -2147023570;

        public LdapRepository()
            : this(new SqlSettingsRepository())
        {
        }

        internal LdapRepository(ISqlSettingsRepository settingsRepository, IAuthenticator authenticator = null)
        {
            _settingsRepository = settingsRepository;
            _authenticator = authenticator ?? this;
        }

        public async Task<AuthenticationStatus> AuthenticateLdapUserAsync(string login, string password, bool useDefaultConnection)
        {
            var authenticationStatus = AuthenticationStatus.Error;
            var loginInfo = LoginInfo.Parse(login);
            if (useDefaultConnection)
            {
                authenticationStatus = _authenticator.AuthenticateViaDirectory(loginInfo, password);
            }
            else
            {
                var ldapSettings = await _settingsRepository.GetLdapSettingsAsync();
                if (ldapSettings.Any())
                {
                    foreach (var ldapSetting in ldapSettings.OrderByDescending(s => s.MatchsUser(loginInfo.Domain)))
                    {
                        if (!_authenticator.UserExistsInLdapDirectory(ldapSetting, loginInfo))
                        {
                            continue;
                        }
                        authenticationStatus = _authenticator.Authenticate(loginInfo, password, ldapSetting.AuthenticationType);
                        if (authenticationStatus == AuthenticationStatus.Success)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    authenticationStatus = _authenticator.Authenticate(loginInfo, password);
                }
            }
            return authenticationStatus;
        }

        public AuthenticationStatus Authenticate(LoginInfo loginInfo, string password, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
        {
            var authenticationStatus = AuthenticateViaLdap(loginInfo, password, authenticationType);
            if (authenticationStatus != AuthenticationStatus.Success)
            {
                return AuthenticateViaDirectory(loginInfo, password);
            }
            return AuthenticationStatus.Success;
        }

        public AuthenticationStatus AuthenticateViaDirectory(LoginInfo loginInfo, string password)
        {
            try
            {
                using (var searchRoot = new DirectoryEntry(null, loginInfo.UserName, password, AuthenticationTypes.Secure))
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

        private AuthenticationStatus AuthenticateViaLdap(LoginInfo loginInfo, string password, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
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

        public bool UserExistsInLdapDirectory(LdapSettings ldapSettings, LoginInfo loginInfo)
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
