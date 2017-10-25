using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using AdminStore.Models;
using AdminStore.Saml;
using AdminStore.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AdminStore.Repositories
{
    [TestClass]
    [DeploymentItem("Certificates", "Certificates")]
    public class SamlRepositoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessResponse_SamlResponse_Null_ArgumentNullException()
        {
            // Arrange
            var samlRepository = new SamlRepository();
            var fedAuthSettingsMock = new Mock<IFederatedAuthenticationSettings>();
            // Act&assert
            samlRepository.ProcessResponse(null, fedAuthSettingsMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessResponse_FederatedAuthenticationSettings_Null_ArgumentNullException()
        {
            // Arrange
            var samlRepository = new SamlRepository();
            // Act&assert
            samlRepository.ProcessResponse("<saml:Assertion/>", null);
        }

        [TestMethod]
        public void ProcessResponse_WrongFormat_FederatedAuthenticationException()
        {
            // Arrange
            const string password = "blueprint";
            const string claimType = "Username";
            const string userName = "admin";

            var certificate = File.ReadAllBytes("Certificates\\samlTestCertificate.pfx");
            var signingCert = new X509Certificate2(certificate, password);
            var securityToken = SamlUtilities.CreateSaml2SecurityToken(certificate, "blueprint", new Claim(claimType, userName));
            var samltoken = SamlUtilities.Serialize(securityToken);
            samltoken = samltoken.Replace("Assertion", "FakeAssertion");

            var samlRepository = new SamlRepository();
            var fedAuthSettingsMock = new Mock<IFederatedAuthenticationSettings>();
            fedAuthSettingsMock.SetupGet(p => p.NameClaimType).Returns(claimType);
            fedAuthSettingsMock.SetupGet(p => p.Certificate).Returns(signingCert);
            // Act
            try
            {
                samlRepository.ProcessResponse(samltoken, fedAuthSettingsMock.Object);
            }
            // Assert
            catch (FederatedAuthenticationException e)
            {
                Assert.AreEqual(FederatedAuthenticationErrorCode.WrongFormat, e.ErrorCode);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ProcessResponse_WrongFormat_FixDev1727_FederatedAuthenticationException()
        {
            // Arrange
            const string password = "blueprint";
            const string claimType = "Username";

            var certificate = File.ReadAllBytes("Certificates\\samlTestCertificate.pfx");
            var signingCert = new X509Certificate2(certificate, password);
            var samltoken = @"<samlp:AuthnRequest />";

            var samlRepository = new SamlRepository();
            var fedAuthSettingsMock = new Mock<IFederatedAuthenticationSettings>();
            fedAuthSettingsMock.SetupGet(p => p.NameClaimType).Returns(claimType);
            fedAuthSettingsMock.SetupGet(p => p.Certificate).Returns(signingCert);

            // Act
            try
            {
                samlRepository.ProcessResponse(samltoken, fedAuthSettingsMock.Object);
            }
            // Assert
            catch (FederatedAuthenticationException e)
            {
                Assert.AreEqual(FederatedAuthenticationErrorCode.WrongFormat, e.ErrorCode);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ProcessEncodedResponse_Success()
        {
            // Arrange
            const string password = "blueprint";
            const string claimType = "Username";
            const string userName = "admin";

            var certificate = File.ReadAllBytes("Certificates\\samlTestCertificate.pfx");
            var signingCert = new X509Certificate2(certificate, password);
            var securityToken = SamlUtilities.CreateSaml2SecurityToken(certificate, password, new Claim(claimType, userName));
            var samltoken = SamlUtilities.Serialize(securityToken);
            var encodedSamltoken = HttpUtility.HtmlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(samltoken)));

            var samlRepository = new SamlRepository();
            var fedAuthSettingsMock = new Mock<IFederatedAuthenticationSettings>();
            fedAuthSettingsMock.SetupGet(p => p.NameClaimType).Returns(claimType);
            fedAuthSettingsMock.SetupGet(p => p.Certificate).Returns(signingCert);
            // Act
            var result = samlRepository.ProcessEncodedResponse(encodedSamltoken, fedAuthSettingsMock.Object);

            // Assert
            Assert.IsTrue(result.Identity.IsAuthenticated);
            Assert.AreEqual(userName, result.Identity.Name);
        }

        [TestMethod]
        public void ProcessResponse_Success()
        {
            // Arrange
            const string password = "blueprint";
            const string claimType = "Username";
            const string userName = "admin";

            var certificate = File.ReadAllBytes("Certificates\\samlTestCertificate.pfx");
            var signingCert = new X509Certificate2(certificate, password);
            var securityToken = SamlUtilities.CreateSaml2SecurityToken(certificate, password, new Claim(claimType, userName));
            var samltoken = SamlUtilities.Serialize(securityToken);

            var samlRepository = new SamlRepository();
            var fedAuthSettingsMock = new Mock<IFederatedAuthenticationSettings>();
            fedAuthSettingsMock.SetupGet(p => p.NameClaimType).Returns(claimType);
            fedAuthSettingsMock.SetupGet(p => p.Certificate).Returns(signingCert);
            // Act
            var result = samlRepository.ProcessResponse(samltoken, fedAuthSettingsMock.Object);

            // Assert
            Assert.IsTrue(result.Identity.IsAuthenticated);
            Assert.AreEqual(userName, result.Identity.Name);
        }

        [TestMethod]
        public void ProcessResponse_Failure_SecurityTokenValidationException()
        {
            // Arrange
            const string password = "blueprint";
            const string claimType = "Username";
            const string userName = "admin";

            var certificate = File.ReadAllBytes("Certificates\\samlTestCertificate.pfx");
            var signingCert = new X509Certificate2(certificate, password);

            var securityToken = SamlUtilities.CreateSaml2SecurityTokenSigningByRsa(certificate, password, new Claim(claimType, userName));
            var samltoken = SamlUtilities.Serialize(securityToken);
            var samlRepository = new SamlRepository();
            var fedAuthSettingsMock = new Mock<IFederatedAuthenticationSettings>();
            fedAuthSettingsMock.SetupGet(p => p.NameClaimType).Returns("Username");
            fedAuthSettingsMock.SetupGet(p => p.Certificate).Returns(signingCert);
            // Act
            try
            {
                samlRepository.ProcessResponse(samltoken, fedAuthSettingsMock.Object);
            }
            // Assert
            catch (FederatedAuthenticationException e)
            {
                Assert.AreEqual(FederatedAuthenticationErrorCode.NotTrustedIssuer, e.ErrorCode);
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}
