using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using ServiceLibrary.Helpers;

namespace AdminStore.Saml
{
    public class SamlCertificateValidator : X509CertificateValidator
    {
        private readonly X509Certificate2 _allowedCertificate;
        private readonly bool _verifyCertificateChain;

        public SamlCertificateValidator(X509Certificate2 allowedCertificate, bool verifyCertificateChain)
        {
            _allowedCertificate = allowedCertificate;
            _verifyCertificateChain = verifyCertificateChain;
        }

        public override void Validate(X509Certificate2 certificate)
        {
            // Check that there is a certificate.
            ThrowIf.ArgumentNull(certificate, nameof(certificate));

            if (_allowedCertificate.IssuerName.Name != certificate.IssuerName.Name)
            {
                throw new FederatedAuthenticationException(
                    "Certificate was not issued by a trusted issuer",
                    FederatedAuthenticationErrorCode.NotTrustedIssuer);
            }

            if (_allowedCertificate.Thumbprint != certificate.Thumbprint)
            {
                throw new FederatedAuthenticationException(
                    "Invalid thumbprint! Certificate was not issued by a trusted issuer",
                    FederatedAuthenticationErrorCode.NotTrustedIssuer);
            }

            if (!IsValidCertificateTime(certificate.NotBefore, certificate.NotAfter))
            {
                throw new FederatedAuthenticationException(
                    "Invalid usage time! Certificate was not issued by a trusted issuer",
                    FederatedAuthenticationErrorCode.NotTrustedIssuer);
            }

            if (_verifyCertificateChain)
            {
                if (!certificate.Verify())
                {
                    CollectAndLogValidationInfo(certificate);

                    throw new FederatedAuthenticationException("Certificate Validation Failed",
                                                               FederatedAuthenticationErrorCode.CertificateValidation);
                }
            }
        }

        private static bool IsValidCertificateTime(DateTime notBefore, DateTime notAfter)
        {
            var now = DateTime.Now;
            return now <= notAfter && now >= notBefore;
        }

        private void CollectAndLogValidationInfo(X509Certificate2 certificate)
        {
            var chain = new X509Chain();
            var chainIsValid = chain.Build(certificate);

            if (!chainIsValid)
            {
                var chainStatuses = chain.ChainStatus;
                if (chainStatuses.Length > 0)
                {
                    // var statusesString = string.Join(", ",
                    //                                 chainStatuses.Select(
                    //                                     st =>
                    //                                     string.Format("{0}: {1}", st.Status, st.StatusInformation)));
                    // TODO logging
                    // Log.InfoFormat("[SAMLHandler] Certificate Verification Failed:  {0}", statusesString);
                }
            }
        }
    }
}
