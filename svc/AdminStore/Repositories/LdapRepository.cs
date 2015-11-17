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
        bool SearchLdap(LdapSettings settings, string filter);
        bool SearchDirectory(LoginInfo loginInfo, string password);
        void Bind(LoginInfo loginInfo, string password, AuthenticationTypes authenticationType);
    }

    public class LdapRepository : ILdapRepository, IAuthenticator
    {
        internal readonly ISqlSettingsRepository _settingsRepository;
        internal readonly IAuthenticator _authenticator;

        internal const int LdapInvalidCredentialsErrorCode = 49;
        internal const int ActiveDirectoryInvalidCredentialsErrorCode = -2147023570;

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
                authenticationStatus = AuthenticateViaDirectory(loginInfo, password);
            }
            else
            {
                var ldapSettings = await _settingsRepository.GetLdapSettingsAsync();
                if (ldapSettings.Any())
                {
                    foreach (var ldapSetting in ldapSettings.OrderByDescending(s => s.MatchesDomain(loginInfo.Domain)))
                    {
                        if (!UserExistsInLdapDirectory(ldapSetting, loginInfo))
                        {
                            continue;
                        }
                        authenticationStatus = Authenticate(loginInfo, password, ldapSetting.AuthenticationType);
                        if (authenticationStatus == AuthenticationStatus.Success)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    authenticationStatus = Authenticate(loginInfo, password);
                }
            }
            return authenticationStatus;
        }

        private AuthenticationStatus Authenticate(LoginInfo loginInfo, string password, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
        {
            var authenticationStatus = AuthenticateViaLdap(loginInfo, password, authenticationType);
            if (authenticationStatus != AuthenticationStatus.Success)
            {
                return AuthenticateViaDirectory(loginInfo, password);
            }
            return AuthenticationStatus.Success;
        }

        private AuthenticationStatus AuthenticateViaDirectory(LoginInfo loginInfo, string password)
        {
            try
            {
                if (!_authenticator.SearchDirectory(loginInfo, password))
                {
                    return AuthenticationStatus.Error;
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
                _authenticator.Bind(loginInfo, password, authenticationType);
                return AuthenticationStatus.Success;
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
        }

        private bool UserExistsInLdapDirectory(LdapSettings ldapSettings, LoginInfo loginInfo)
        {
            var userName = loginInfo.UserName != null ? loginInfo.UserName.Trim() : loginInfo.Login;
            var filter = string.Format("(&(objectCategory=user)({0}={1}))", ldapSettings.GetEffectiveAccountNameAttribute(), LdapHelper.EscapeLdapSearchFilter(userName));
            try
            {
                bool found = _authenticator.SearchLdap(ldapSettings, filter);

                if (!found)
                {
                    //TODO logging
                    //Log.Debug(string.Format("User '{0}' is not found in LDAP directory {1}", domainUserName, ldapSettings.LdapAuthenticationUrl));
                    return false;
                }

                return true;
            }
            catch
            {
                //TODO logging
                //Log.Debug(string.Format("Error while searching a user in LDAP directory {0} - {1}", ldapSettings.LdapAuthenticationUrl, ex.Message));
                //Log.Debug(ex);
                return false;
            }
        }

        #region IAuthenticator

        public bool SearchLdap(LdapSettings settings, string filter)
        {
            using (var searchRoot = new DirectoryEntry(settings.LdapAuthenticationUrl, settings.BindUser, settings.BindPassword, settings.AuthenticationType))
            {
                using (var searcher = new DirectorySearcher(searchRoot, filter) { PropertyNamesOnly = false, ReferralChasing = ReferralChasingOption.None })
                {
                    return searcher.FindOne() != null;
                }
            }
        }

        public bool SearchDirectory(LoginInfo loginInfo, string password)
        {
            using (var searchRoot = new DirectoryEntry(null, loginInfo.UserName, password, AuthenticationTypes.Secure))
            {
                using (var searcher = new DirectorySearcher(searchRoot) { CacheResults = false })
                {
                    return searcher.FindOne() != null;
                }
            }
        }

        public void Bind(LoginInfo loginInfo, string password, AuthenticationTypes authenticationType)
        {
            using (var ldapConnection = new LdapConnection(loginInfo.Domain) { AuthType = AuthType.Negotiate })
            {
                ldapConnection.SessionOptions.SecureSocketLayer = authenticationType.HasFlag(AuthenticationTypes.SecureSocketsLayer);
                ldapConnection.Bind(new NetworkCredential(loginInfo.UserName, password, loginInfo.Domain));
            }
        }

        #endregion IAuthenticator
    }
}
