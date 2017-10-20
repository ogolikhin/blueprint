using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Saml
{
    [TestClass]
    public class SamlCertificateValidatorTests
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void Validate_Failure_ArgumentNullException()
        {
            var samlCertificateValidator = new SamlCertificateValidator(_signingCert, false);
            X509Certificate2 certificate = null;
            // Act&Assert
            samlCertificateValidator.Validate(certificate);
        }

        [TestMethod]
        [ExpectedException(typeof(FederatedAuthenticationException))]
        public void Validate_InvalidThumbprint_FederatedAuthenticationException()
        {
            var samlCertificateValidator = new SamlCertificateValidator(_signingCert, false);
            var dummyCertificate = File.ReadAllBytes("Certificates\\dummyCertificate.pfx");
            var certificate = new X509Certificate2(dummyCertificate, Password);
            // Act&Assert
            samlCertificateValidator.Validate(certificate);
        }

        // [TestMethod]
        // [ExpectedException(typeof(FederatedAuthenticationException))]
        // public void Validate_InvalidCertificateChainValidation_FederatedAuthenticationException()
        // {
        //    var samlCertificateValidator = new SamlCertificateValidator(_signingCert, true);
        //    //Act&Assert
        //    samlCertificateValidator.Validate(_signingCert);
        // }
    }
}
