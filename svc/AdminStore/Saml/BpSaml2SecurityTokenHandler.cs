using System;
using System.Collections.ObjectModel;
using System.IdentityModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Xml;

namespace AdminStore.Saml
{
    public class BpSaml2SecurityTokenHandler : Saml2SecurityTokenHandler
    {
        private readonly string _samlXml;

        public BpSaml2SecurityTokenHandler(string samlXml, SamlSecurityTokenRequirement samlSecurityTokenRequirement)
            : base(samlSecurityTokenRequirement)
        {
            _samlXml = samlXml;
        }

        /*
        Important: You must override CanValidateToken and return true, or your token handler will not be used.
        */
        public override bool CanValidateToken
        {
            get { return true; }
        }

        protected override void ValidateConfirmationData(Saml2SubjectConfirmationData confirmationData)
        {
            /* Saml2SecurityToken cannot be created from the Saml2Assertion because it contains a SubjectConfirmationData
             * which specifies an InResponseTo value. Enforcement of this value is not supported by default.
             * To customize SubjectConfirmationData processing, extend Saml2SecurityTokenHandler and override ValidateConfirmationData.
             */
            // base.ValidateConfirmationData(confirmationData);
        }

        public override SecurityToken ReadToken(string tokenString)
        {
            var assertion = new Saml2Assertion(new Saml2NameIdentifier("__TemporaryIssuer__"));

            var wrappedSerializer = new WrappedSerializer(this, assertion);

            var sr = new StringReader(_samlXml);
            using (var reader1 = XmlReader.Create(sr))
            {
                var reader2 = new EnvelopedSignatureReader(reader1,
                                                            wrappedSerializer,
                                                            Configuration.IssuerTokenResolver,
                                                            false,
                                                            false, false);

                if (!reader2.ReadToFollowing("Signature", "http://www.w3.org/2000/09/xmldsig#") || !reader2.TryReadSignature())
                {
                    throw new FederatedAuthenticationException("Cannot find token signature",
                                                                FederatedAuthenticationErrorCode.WrongFormat);
                }

                if (!reader2.ReadToFollowing("Assertion", "urn:oasis:names:tc:SAML:2.0:assertion"))
                {
                    throw new FederatedAuthenticationException("Cannot find token assertion",
                                                                FederatedAuthenticationErrorCode.WrongFormat);
                }

                assertion = ReadAssertion(reader2);

                try
                {
                    while (reader2.Read())
                    {
                        //
                    }
                }
                catch (CryptographicException cryptographicExceptione)
                {
                    throw new FederatedAuthenticationException(cryptographicExceptione.Message,
                                                                FederatedAuthenticationErrorCode.NotTrustedIssuer,
                                                                cryptographicExceptione);
                }

                assertion.SigningCredentials = reader2.SigningCredentials;

                var keys = ResolveSecurityKeys(assertion, Configuration.ServiceTokenResolver);
                SecurityToken token;
                TryResolveIssuerToken(assertion, Configuration.IssuerTokenResolver, out token);
                return new Saml2SecurityToken(assertion, keys, token);
            }
        }

        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            ReadOnlyCollection<ClaimsIdentity> claimsCollection;

            try
            {
                claimsCollection = base.ValidateToken(token);
            }
            catch (SecurityTokenNotYetValidException notYetValidException)
            {
                throw new FederatedAuthenticationException(
                    notYetValidException.Message,
                    FederatedAuthenticationErrorCode.NotTrustedIssuer, notYetValidException);
            }
            catch (SecurityTokenExpiredException tokenExpiredException)
            {
                throw new FederatedAuthenticationException(
                    tokenExpiredException.Message,
                    FederatedAuthenticationErrorCode.NotTrustedIssuer, tokenExpiredException);
            }
            catch (AudienceUriValidationFailedException audienceUriValidationFailedException)
            {
                throw new FederatedAuthenticationException(
                    audienceUriValidationFailedException.Message,
                    FederatedAuthenticationErrorCode.NotTrustedIssuer, audienceUriValidationFailedException);
            }
            catch (SecurityTokenValidationException exception)
            {
                // TODO logging
                // Log.ErrorFormat("Need manually validate SAML token {0}", exception);

                // Log.InfoFormat("[SAMLHandler].Token: {0}", _samlXml);

                var saml2Token = token as Saml2SecurityToken;
                if (saml2Token != null && saml2Token.Assertion.SigningCredentials == null)
                {
                    throw new FederatedAuthenticationException("No signature",
                                                               FederatedAuthenticationErrorCode.WrongFormat,
                                                               exception);
                }
                throw new FederatedAuthenticationException("Cannot validate token",
                                                                   FederatedAuthenticationErrorCode.NotTrustedIssuer,
                                                                   exception);
            }

            return claimsCollection;
        }

        internal class WrappedSerializer : SecurityTokenSerializer
        {
            private readonly BpSaml2SecurityTokenHandler _parent;
            private readonly Saml2Assertion _assertion;

            public WrappedSerializer(BpSaml2SecurityTokenHandler parent, Saml2Assertion assertion)
            {
                _assertion = assertion;
                _parent = parent;
            }

            protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader)
            {
                return false;
            }

            protected override bool CanReadKeyIdentifierCore(XmlReader reader)
            {
                return true;
            }

            protected override bool CanReadTokenCore(XmlReader reader)
            {
                return false;
            }

            protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return false;
            }

            protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier)
            {
                return false;
            }

            protected override bool CanWriteTokenCore(SecurityToken token)
            {
                return false;
            }

            protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
            {
                throw new NotSupportedException();
            }

            protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader)
            {
                return _parent.ReadSigningKeyInfo(reader, _assertion);
            }

            protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
            {
                throw new NotSupportedException();
            }

            protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                throw new NotSupportedException();
            }

            protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
            {
                _parent.WriteSigningKeyInfo(writer, keyIdentifier);
            }

            protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
            {
                throw new NotSupportedException();
            }
        }
    }
}
