using System.Security.Principal;
using AdminStore.Models;

namespace AdminStore.Saml
{
    public interface ISamlRepository
    {
        IPrincipal ProcessEncodedResponse(string samlResponse, IFederatedAuthenticationSettings fedAuthSettings);

        IPrincipal ProcessResponse(string samlResponse, IFederatedAuthenticationSettings settings);
    }
}
