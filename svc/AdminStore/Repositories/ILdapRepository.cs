using AdminStore.Helpers;

namespace AdminStore.Repositories
{
    public interface ILdapRepository
    {
        AuthenticationStatus AuthenticateLdapUser(string domainUserName, string password, bool useLdapSettings = false);
    }
}