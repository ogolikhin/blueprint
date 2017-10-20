namespace AdminStore.Saml
{
    public enum FederatedAuthenticationErrorCode
    {
        None = 0,
        Unknown = 1,
        NotTrustedIssuer = 2, // "Certificate was not issued by a trusted issuer"
        NoIdentityProvider = 3, // Identity Provider not defined in Federated Authentication settings
        NoNameClaim = 4,
        WrongFormat = 5,
        CertificateValidation = 6, // cannot verify certificate (don't allow self signed certificates)
    }
}