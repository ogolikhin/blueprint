using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using Dapper;

namespace AdminStore.Repositories
{
    public class LdapRepository : ILdapRepository
    {
        private const int AdServerNotOperationalErrorCode = -2147016646;

        private const int LdapInvalidCredentialsErrorCode = -2147023570;

        public AuthenticationStatus AuthenticateLdapUser(string domainUserName, string password, bool useLdapSettings = false)
        {
            if (string.IsNullOrEmpty(domainUserName) || string.IsNullOrEmpty(password))
            {
                return AuthenticationStatus.InvalidCredentials;
            }

            if (!useLdapSettings)
            {
                return TryAuthenticate(domainUserName, password);
            }

            return AuthenticationStatus.Error;
        }

        private AuthenticationStatus TryAuthenticate(string domainUserName, string password, string path = null, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
        {
            var authenticationStatus = AuthenticationStatus.Error; 
            try
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (AuthenticateUsingLdap(domainUserName, password, authenticationType) == AuthenticationStatus.Success)
                    {
                        return AuthenticationStatus.Success;
                    }
                }
                return AuthenticateUsingDirectorySearcher(domainUserName, password);
            }
            catch (COMException comException)
            {
                if (comException.ErrorCode == LdapInvalidCredentialsErrorCode)
                {
                    authenticationStatus = AuthenticationStatus.InvalidCredentials;
                }
            }
            catch (LdapException ldapException)
            {
                if (ldapException.ErrorCode == LdapInvalidCredentialsErrorCode)
                {
                    authenticationStatus = AuthenticationStatus.InvalidCredentials;
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
            return authenticationStatus;
        }

        private AuthenticationStatus AuthenticateUsingDirectorySearcher(string userName, string password, string path = null, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
        {
            using (var searchRoot = new DirectoryEntry(path, userName, password, authenticationType))
            {
                using (var searcher = new DirectorySearcher(searchRoot))
                {
                    searcher.CacheResults = false;
                    var result = searcher.FindOne();
                    using (result.GetDirectoryEntry())
                    {
                    }
                }
            }
            return AuthenticationStatus.Success;
        }

        private AuthenticationStatus AuthenticateUsingLdap(string domainUserName, string password, AuthenticationTypes authenticationType = AuthenticationTypes.Secure)
        {
            var domainAndUserName = SplitDomainUserName(domainUserName);
            var userName = domainAndUserName[1];
            var domain = domainAndUserName[0];
            using (var ldapConnection = new LdapConnection(domain))
            {
                var networkCredential = new NetworkCredential(userName, password, domain);
                ldapConnection.SessionOptions.SecureSocketLayer = authenticationType.HasFlag(AuthenticationTypes.SecureSocketsLayer);
                ldapConnection.AuthType = AuthType.Negotiate;
                ldapConnection.Bind(networkCredential);
            }
            return AuthenticationStatus.Success;
        }

        private static string[] SplitDomainUserName(string domainUserName)
        {
            var result = new string[] { null, null };
            if (string.IsNullOrWhiteSpace(domainUserName))
                return result;

            var array = domainUserName.Split(new[] { '\\' });
            if (array.Length == 2)
                return array;

            result[0] = array[0];
            return result;
        }

        public virtual async Task<IEnumerable<LdapSettings>> GetLdapSettings()
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                return await cxn.QueryAsync<LdapSettings>("GetLdapSettings", commandType: CommandType.StoredProcedure);
            }
        }
    }
}