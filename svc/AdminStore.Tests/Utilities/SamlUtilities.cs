using System;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using ServiceLibrary.Helpers;

namespace AdminStore.Utilities
{
    public class SamlUtilities
    {
        public const string BearerString = "urn:oasis:names:tc:SAML:2.0:cm:bearer";

        /// <summary>
        /// The subject of the assertion is the bearer of the assertion. [Saml2Prof, 3.3]
        /// </summary>
        public static readonly Uri Bearer = new Uri(BearerString);

        public static readonly string DefaultIssuer = "http://idp.bptest.ca/adfs/services/trust";

        /// <summary>
        /// Creates a SAML assertion signed with the given certificate.
        /// </summary>
        public static Saml2SecurityToken CreateSaml2SecurityToken(byte[] certificate, string password, params Claim[] claims)
        {
            const string acsUrl = "http://blueprintsys.com";

            var assertion = new Saml2Assertion(new Saml2NameIdentifier(DefaultIssuer));

            var conditions = new Saml2Conditions
            {
                NotBefore = DateTime.UtcNow,
                NotOnOrAfter = DateTime.MaxValue
            };
            conditions.AudienceRestrictions.Add(new Saml2AudienceRestriction(new Uri(acsUrl, UriKind.RelativeOrAbsolute)));
            assertion.Conditions = conditions;

            var subject = new Saml2Subject();
            subject.SubjectConfirmations.Add(new Saml2SubjectConfirmation(Bearer));
            assertion.Subject = subject;

            var statement = new Saml2AttributeStatement();
            foreach (var claim in claims)
            {
                statement.Attributes.Add(new Saml2Attribute(claim.Type, claim.Value));
                assertion.Statements.Add(statement);
            }

            var clientSigningCredentials = new X509SigningCredentials(
                    new X509Certificate2(certificate, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable));

            assertion.SigningCredentials = clientSigningCredentials;

            return new Saml2SecurityToken(assertion);
        }

        // public static SamlSecurityToken CreateSamlSecurityToken(byte[] certificate, string password, params Claim[] claims)
        // {
        //    const string acsUrl = "http://blueprintsys.com";

        // var assertion = new SamlAssertion(new SamlNameIdentifier(DefaultIssuer));

        // var conditions = new Saml2Conditions
        //    {
        //        NotBefore = DateTime.UtcNow,
        //        NotOnOrAfter = DateTime.MaxValue
        //    };
        //    conditions.AudienceRestrictions.Add(new Saml2AudienceRestriction(new Uri(acsUrl, UriKind.RelativeOrAbsolute)));
        //    assertion.Conditions = conditions;

        // var subject = new Saml2Subject();
        //    subject.SubjectConfirmations.Add(new Saml2SubjectConfirmation(Bearer));
        //    assertion.Subject = subject;

        // var statement = new Saml2AttributeStatement();
        //    foreach (var claim in claims)
        //    {
        //        statement.Attributes.Add(new Saml2Attribute(claim.Type, claim.Value));
        //        assertion.Statements.Add(statement);
        //    }

        // var clientSigningCredentials = new X509SigningCredentials(
        //            new X509Certificate2(certificate, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable));

        // assertion.SigningCredentials = clientSigningCredentials;

        // return new Saml2SecurityToken(assertion);
        // }

        public static string Serialize(Saml2SecurityToken token)
        {
            var handler = new Saml2SecurityTokenHandler();
            var sw = I18NHelper.CreateStringWriterInvariant();
            using (var textWriter = new XmlTextWriter(sw))
            {
                handler.WriteToken(textWriter, token);
                return sw.ToString();
            }
        }

        public static Saml2SecurityToken CreateSaml2SecurityTokenSigningByRsa(byte[] certificate, string password, params Claim[] claims)
        {
            var descriptor = new SecurityTokenDescriptor();

            var digestAlgorithm = "http://www.w3.org/2000/09/xmldsig#sha1";
            var signatureAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

            var signingCert = new X509Certificate2(certificate, password);

            var rsa = signingCert.PrivateKey as RSACryptoServiceProvider;
            var rsaKey = new RsaSecurityKey(rsa);
            var rsaClause = new RsaKeyIdentifierClause(rsa);
            var signingSki = new SecurityKeyIdentifier(rsaClause);
            var signingCredentials = new SigningCredentials(rsaKey, signatureAlgorithm, digestAlgorithm, signingSki);

            descriptor.TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
            descriptor.TokenIssuerName = "CN=app.nhin-hv.com, OU=Domain Control Validated, O=app.nhin-hv.com";
            descriptor.SigningCredentials = signingCredentials;
            descriptor.Subject = new ClaimsIdentity(claims);
            descriptor.AppliesToAddress = "http://localhost/RelyingPartyApplication";

            var issueInstant = DateTime.UtcNow;
            descriptor.Lifetime = new Lifetime(issueInstant, issueInstant + TimeSpan.FromHours(8));

            var tokenHandler = new Saml2SecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor) as Saml2SecurityToken;
            return token;
        }
    }
}
