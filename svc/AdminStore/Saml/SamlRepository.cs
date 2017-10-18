using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Xml;
using AdminStore.Models;
using ServiceLibrary.Helpers;

namespace AdminStore.Saml
{
    public class SamlRepository : ISamlRepository
    {
        public IPrincipal ProcessEncodedResponse(string encodedSamlXml, IFederatedAuthenticationSettings settings)
        {
            var samlXml = Encoding.UTF8.GetString(Convert.FromBase64String(HttpUtility.HtmlDecode(encodedSamlXml)));
            return ProcessResponse(samlXml, settings);
        }

        public IPrincipal ProcessResponse(string samlResponse, IFederatedAuthenticationSettings settings)
        {
            ThrowIf.ArgumentNull(samlResponse, nameof(samlResponse));
            ThrowIf.ArgumentNull(settings, nameof(settings));

            var token = ReadSecurityToken(samlResponse, settings);

            if (token == null)
            {
                // TODO add logging
                // Log.DebugFormat("[SAMLHandler] Cannot read non SAML2 token.\n {0}", tokenString);
                throw new FederatedAuthenticationException("Cannot read token",
                                                           FederatedAuthenticationErrorCode.WrongFormat);
            }

            var samlSecurityTokenRequirement = new SamlSecurityTokenRequirement
            {
                NameClaimType = settings.NameClaimType, // "Username",
                MapToWindows = false
            };
            var handler = new BpSaml2SecurityTokenHandler(samlResponse, samlSecurityTokenRequirement)
            {
                Configuration = new SecurityTokenHandlerConfiguration()
            };

            ConfigureHandler(handler.Configuration, settings);

            ReadOnlyCollection<ClaimsIdentity> validateToken;
            try
            {
                validateToken = handler.ValidateToken(token);
            }
            catch (FederatedAuthenticationException faEx)
            {
                if (faEx.ErrorCode != FederatedAuthenticationErrorCode.WrongFormat)
                {
                    throw;
                }
                token = (Saml2SecurityToken)handler.ReadToken(samlResponse);
                validateToken = handler.ValidateToken(token);
            }

            return new ClaimsPrincipal(validateToken);
        }

        private static Saml2SecurityToken ReadSecurityToken(string samlXml, IFederatedAuthenticationSettings settings)
        {
            var sr = new StringReader(samlXml);
            using (var reader = XmlReader.Create(sr))
            {
                try
                {
                    if (!reader.ReadToFollowing("Assertion", "urn:oasis:names:tc:SAML:2.0:assertion"))
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    throw new FederatedAuthenticationException("Cannot read token", FederatedAuthenticationErrorCode.WrongFormat, ex);
                }
                // Deserialize the token so that data can be taken from it and plugged into the RSTR
                var collection = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
                ConfigureHandler(collection.Configuration, settings);
                var tokenString = reader.ReadSubtree();
                return collection.ReadToken(tokenString) as Saml2SecurityToken;
            }
        }

        private static void ConfigureHandler(SecurityTokenHandlerConfiguration hc, IFederatedAuthenticationSettings settings)
        {
            hc.AudienceRestriction = new AudienceRestriction(AudienceUriMode.Never);
            hc.CertificateValidator = new SamlCertificateValidator(settings.Certificate, WebApiConfig.VerifyCertificateChain);
            hc.IssuerNameRegistry = new SamlIssuerNameRegistry(settings.Certificate);
            hc.IssuerTokenResolver = new IssuerTokenResolver();

            // need this if certificate not included into sign info
            var tokens = new List<SecurityToken> { new X509SecurityToken(settings.Certificate) };
            hc.ServiceTokenResolver = SecurityTokenResolver.CreateDefaultSecurityTokenResolver(
                new ReadOnlyCollection<SecurityToken>(tokens), false);
        }
    }
}