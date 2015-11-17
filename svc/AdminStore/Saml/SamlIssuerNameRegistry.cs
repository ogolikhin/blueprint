using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace AdminStore.Saml
{
    public class SamlIssuerNameRegistry : IssuerNameRegistry
    {
        private readonly X509Certificate2 _certificate;

        public SamlIssuerNameRegistry(X509Certificate2 certificate)
        {
            _certificate = certificate;
        }

        // called by X509SecurityTokenHandler.Validate
        public override string GetIssuerName(SecurityToken securityToken)
        {
            if (!(securityToken is X509SecurityToken))
            {
                throw new SecurityTokenValidationException("Invalid token.");
            }

            var x509Token = securityToken as X509SecurityToken;

            // in the X509 case, the X509 token has no notion of issuer name
            var issuerTokenValid = IsIssuerTokenValid(x509Token);

            if (!issuerTokenValid)
            {
                throw new SecurityTokenValidationException("Untrusted issuer token.");
            }

            return x509Token.Certificate.FriendlyName;
        }

        // called by Saml11SecurityTokenHandler.Validate and Saml2SecurityTokenHandler.Validate
        public override string GetIssuerName(SecurityToken securityToken, string requestedIssuerName)
        {
            var x509Token = securityToken as X509SecurityToken;
            var issuerTokenValid = IsIssuerTokenValid(x509Token);

            if (!issuerTokenValid)
            {
                throw new SecurityTokenValidationException("Untrusted issuer token.");
            }

            return requestedIssuerName;
        }

        private bool IsIssuerTokenValid(X509SecurityToken x509Token)
        {
            if (_certificate != null && x509Token != null && x509Token.Certificate != null)
            {
                return _certificate.Thumbprint == x509Token.Certificate.Thumbprint;
            }

            return false;
        }

        public override string GetWindowsIssuerName()
        {
            return "WINDOWS AUTHORITY";
        }
    }
}