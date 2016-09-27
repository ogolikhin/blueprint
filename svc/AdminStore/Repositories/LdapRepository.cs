using System;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.ConfigControl;

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
        private readonly IServiceLogRepository _log;
        internal readonly IAuthenticator _authenticator;

        internal const int LdapInvalidCredentialsErrorCode = 49;
        internal const int ActiveDirectoryInvalidCredentialsErrorCode = -2147023570;
        private const string LogSourceLdap = "LdapRepository";

        public LdapRepository()
            : this(new SqlSettingsRepository(), new ServiceLogRepository())
        {
        }

        internal LdapRepository(ISqlSettingsRepository settingsRepository, IServiceLogRepository log, IAuthenticator authenticator = null)
        {
            _settingsRepository = settingsRepository;
            _log = log;
            _authenticator = authenticator ?? this;
        }

        public async Task<AuthenticationStatus> AuthenticateLdapUserAsync(string login, string password, bool useDefaultConnection)
        {
            var authenticationStatus = AuthenticationStatus.Error;
            var loginInfo = LoginInfo.Parse(login);
            if (useDefaultConnection)
            {
                authenticationStatus = Authenticate(loginInfo, password);
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
                        loginInfo.LdapUrl = ldapSetting.LdapAuthenticationUrl;
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
                _log.LogError(LogSourceLdap, comException);
                if (comException.ErrorCode == ActiveDirectoryInvalidCredentialsErrorCode)
                {
                    return AuthenticationStatus.InvalidCredentials;
                }
                return AuthenticationStatus.Error;
            }
            catch (Exception ex)
            {
                _log.LogError(LogSourceLdap, ex);
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
                _log.LogError(LogSourceLdap, ldapException);
                if (ldapException.ErrorCode == LdapInvalidCredentialsErrorCode)
                {
                    return AuthenticationStatus.InvalidCredentials;
                }
                return AuthenticationStatus.Error;
            }
            catch (Exception ex)
            {
                _log.LogError(LogSourceLdap, ex);
                return AuthenticationStatus.Error;
            }
        }

        private bool UserExistsInLdapDirectory(LdapSettings ldapSettings, LoginInfo loginInfo)
        {
            var userName = loginInfo.UserName != null ? loginInfo.UserName.Trim() : loginInfo.Login;
            var filter = I18NHelper.FormatInvariant("(&(objectCategory={0})({1}={2}))", ldapSettings.GetEffectiveUserObjectCategoryAttribute(), ldapSettings.GetEffectiveAccountNameAttribute(), LdapHelper.EscapeLdapSearchFilter(userName));
            try
            {
                var found = _authenticator.SearchLdap(ldapSettings, filter);

                if (found)
                {
                    return true;
                }
                _log.LogInformation(LogSourceLdap, I18NHelper.FormatInvariant("User '{0}' is not found in LDAP directory {1}", userName, ldapSettings.LdapAuthenticationUrl));
                return false;
            }
            catch (Exception ex)
            {
                _log.LogInformation(LogSourceLdap, I18NHelper.FormatInvariant("Error while searching a user in LDAP directory {0}", ldapSettings.LdapAuthenticationUrl));
                _log.LogError(LogSourceLdap, ex);
                return false;
            }
        }

        #region IAuthenticator

        public bool SearchLdap(LdapSettings settings, string filter)
        {
            using (var searchRoot = new DirectoryEntry(settings.LdapAuthenticationUrl, settings.BindUser, settings.GetDecodedBindPassword(), settings.AuthenticationType))
            {
                using (var searcher = new DirectorySearcher(searchRoot, filter) { PropertyNamesOnly = false, ReferralChasing = ReferralChasingOption.None })
                {
                    return searcher.FindOne() != null;
                }
            }
        }

        public bool SearchDirectory(LoginInfo loginInfo, string password)
        {
            using (var searchRoot = new DirectoryEntry(loginInfo.LdapUrl, loginInfo.UserName, password, AuthenticationTypes.Secure))
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
