using AdminStore.Helpers;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Runtime.InteropServices;
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
        public async Task AuthenticateLdapUserAsync_DefaultConnectionSearchDirectoryReturnsTrue_ReturnsSuccess()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            bool useDefaultConnection = true;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Returns(true).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Success, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_DefaultConnectionSearchDirectoryReturnsFalse_ReturnsError()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            bool useDefaultConnection = true;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Returns(false).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_DefaultConnectionSearchDirectoryThrowsInvalidCredentials_ReturnsInvalidCredentials()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string incorrectPassword = "incorrect";
            bool useDefaultConnection = true;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, incorrectPassword)).Throws(new COMException(null, LdapRepository.ActiveDirectoryInvalidCredentialsErrorCode)).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, incorrectPassword, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.InvalidCredentials, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_DefaultConnectionSearchDirectoryThrowsOtherComException_ReturnsError()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            bool useDefaultConnection = true;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Throws(new COMException()).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_DefaultConnectionSearchDirectoryThrowsOtherException_ReturnsError()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            bool useDefaultConnection = true;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Throws(new Exception()).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_NoSettingsBindThrowsInvalidCredentials_CallsSearchDirectory()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            LdapSettings[] settings = { };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings)).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Secure)).Throws(new LdapException(LdapRepository.LdapInvalidCredentialsErrorCode)).Verifiable();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Returns(true).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Success, status);

        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_NoSettingsBindThrowsOtherLdapException_CallsSearchDirectory()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            LdapSettings[] settings = { };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings)).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Secure)).Throws(new LdapException()).Verifiable();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Returns(true).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Success, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_NoMatchingSettings_ReturnsError()
        {
            // Arrange
            var loginInfo = new LoginInfo();
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[]
            {
                new LdapSettings { LdapAuthenticationUrl = "CD=domain", AuthenticationType = AuthenticationTypes.Encryption },
                new LdapSettings { LdapAuthenticationUrl = "DC=wrongdomain", AuthenticationType = AuthenticationTypes.Encryption }
            }; // Covers LdapHelper.MatchesDomain()
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings)).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_SearchLdapReturnsTrueAndBindSucceeds_ReturnsSuccess()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC=domain", AuthenticationType = AuthenticationTypes.Encryption } };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings)).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.SearchLdap(settings[0], "(&(objectCategory=user)(samaccountname=" + loginInfo.UserName + "))")).Returns(true).Verifiable();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Encryption)).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Success, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_SearchLdapReturnsTrueAndBindThrowsException_CallsSearchDirectory()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC=domain", AuthenticationType = AuthenticationTypes.Encryption } };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings)).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.SearchLdap(settings[0], "(&(objectCategory=user)(samaccountname=" + loginInfo.UserName + "))")).Returns(true).Verifiable();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Encryption)).Throws<Exception>().Verifiable();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Returns(false).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_SearchLdapReturnsFalse_ReturnsError()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login\\*()\u0000/"); // Covers LdapHelper.EscapeLdapSearchFilter()
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC= domain ", EnableCustomSettings = true, AccountNameAttribute = "account" } }; // Covers LdapHelper.GetEffectiveAccountNameAttribute()
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings)).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.SearchLdap(settings[0], @"(&(objectCategory=user)(account=login\5c\2a\28\29\00\2f))")).Returns(false).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_SearchLdapThrowsException_ReturnsError()
        {
            // Arrange
            var loginInfo = new LoginInfo(); // Covers LdapHelper.EscapeLdapSearchFilter()
            string password = "password";
            bool useDefaultConnection = false;
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC=", EnableCustomSettings = true } }; // Covers LdapHelper.GetEffectiveAccountNameAttribute()
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).Returns(Task.FromResult((IEnumerable<LdapSettings>)settings)).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(a => a.SearchLdap(settings[0], @"(&(objectCategory=user)(samaccountname=" + loginInfo.UserName + "))")).Throws<Exception>().Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, useDefaultConnection);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        #endregion AuthenticateLdapUserAsync
    }
}
