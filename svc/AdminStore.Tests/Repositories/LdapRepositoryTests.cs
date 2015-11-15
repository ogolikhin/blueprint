using AdminStore.Helpers;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    [TestClass]
    public class LdapRepositoryTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var repository = new LdapRepository();

            // Assert
            Assert.IsInstanceOfType(repository._settingsRepository, typeof(SqlSettingsRepository));
        }

        #endregion Constructor

        #region AuthenticateLdapUserAsync

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_UseDefaultConnection_ReturnsAuthenticateResul()
        {
            // Arrange
            string login = "domain\\login";
            string password = "password";
            bool useDefaultConnection = true;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.AuthenticateViaDirectory(It.Is<LoginInfo>(l => l.Login == login), password))
                .Returns(AuthenticationStatus.Success);
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(login, password, useDefaultConnection);

            // Assert
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Success, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_NoLdapSettings_ReturnsAuthenticateResult()
        {
            // Arrange
            string login = "domain\\login";
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            LdapSettings[] settings = { };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings));
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.Authenticate(It.Is<LoginInfo>(l => l.Login == login), password, AuthenticationTypes.Secure))
                .Returns(AuthenticationStatus.Success);
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Success, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_UserDoesNotExist_ReturnsError()
        {
            // Arrange
            string login = "domain\\login";
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC=domain", AuthenticationType = AuthenticationTypes.Encryption } };
            var authenticator = new Mock<IAuthenticator>();
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings));
            authenticator.Setup(a => a.UserExistsInLdapDirectory(settings[0], It.Is<LoginInfo>(l => l.Login == login))).Returns(false);
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_UserExistsAndError_ReturnsError()
        {
            // Arrange
            string login = "domain\\login";
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC=domain", AuthenticationType = AuthenticationTypes.Encryption } };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings));
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.UserExistsInLdapDirectory(settings[0], It.Is<LoginInfo>(l => l.Login == login))).Returns(true);
            authenticator.Setup(a => a.Authenticate(It.Is<LoginInfo>(l => l.Login == login), password, AuthenticationTypes.Encryption))
                .Returns(AuthenticationStatus.Error);
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_UserExistsAndInvalidCredentials_ReturnsInvalidCredentials()
        {
            // Arrange
            string login = "domain\\login";
            string incorrectPassword = "incorrect";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC=domain", AuthenticationType = AuthenticationTypes.Encryption } };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings));
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.UserExistsInLdapDirectory(settings[0], It.Is<LoginInfo>(l => l.Login == login))).Returns(true);
            authenticator.Setup(a => a.Authenticate(It.Is<LoginInfo>(l => l.Login == login), incorrectPassword, AuthenticationTypes.Encryption))
                .Returns(AuthenticationStatus.InvalidCredentials);
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(login, incorrectPassword, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.InvalidCredentials, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_UserExistsAndAuthenticates_ReturnsSuccess()
        {
            // Arrange
            string login = "domain\\login";
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC=domain", AuthenticationType = AuthenticationTypes.Encryption } };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings));
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.UserExistsInLdapDirectory(settings[0], It.Is<LoginInfo>(l => l.Login == login))).Returns(true);
            authenticator.Setup(a => a.Authenticate(It.Is<LoginInfo>(l => l.Login == login), password, AuthenticationTypes.Encryption))
                .Returns(AuthenticationStatus.Success);
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Success, status);
        }

        #endregion AuthenticateLdapUserAsync
    }
}
