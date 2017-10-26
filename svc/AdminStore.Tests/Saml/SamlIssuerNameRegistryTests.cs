using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using AdminStore.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Saml
{
    [TestClass]
    public class SamlIssuerNameRegistryTests
    {
        private const string Password = "blueprint";

        private static X509Certificate2 _signingCert;

        private static byte[] _certificate;

        [TestInitialize]
        public void Init()
        {
            _certificate = File.ReadAllBytes("Certificates\\samlTestCertificate.pfx");
            _signingCert = new X509Certificate2(_certificate, Password);
        }

        [TestMethod]
        public void GetIssuerName_Success()
        {
            // Arrange
            var issuerNameRegistry = new SamlIssuerNameRegistry(_signingCert);
            var securityToken = new X509SecurityToken(_signingCert);

            // Act
            var result = issuerNameRegistry.GetIssuerName(securityToken);

            // Assert
            Assert.AreEqual("samlTestCertificate", result);
        }

        [TestMethod]
        public void GetIssuerName_InvalidToken_Failure()
        {
            // Arrange
            var issuerNameRegistry = new SamlIssuerNameRegistry(_signingCert);
            var securityToken = SamlUtilities.CreateSaml2SecurityToken(_certificate, Password);

            // Act
            try
            {
                issuerNameRegistry.GetIssuerName(securityToken);
            }
                // Assert
            catch (SecurityTokenValidationException e)
            {
                Assert.AreEqual("Invalid token.", e.Message);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GetIssuerName_WrongThumbprint_Failure()
        {
            // Arrange
            var issuerNameRegistry = new SamlIssuerNameRegistry(_signingCert);

            var dummyCertificate = File.ReadAllBytes("Certificates\\dummyCertificate.pfx");
            var certificate = new X509Certificate2(dummyCertificate, Password);
            var securityToken = new X509SecurityToken(certificate);

            // Act
            try
            {
                issuerNameRegistry.GetIssuerName(securityToken);
            }
            // Assert
            catch (SecurityTokenValidationException e)
            {
                Assert.AreEqual("Untrusted issuer token.", e.Message);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GetWindowsIssuerName_Success()
        {
            // Arrange
            var issuerNameRegistry = new SamlIssuerNameRegistry(_signingCert);

            // Act
            var windowsIssuerName = issuerNameRegistry.GetWindowsIssuerName();

            // Assert
            Assert.AreEqual("WINDOWS AUTHORITY", windowsIssuerName);
        }
    }
}
