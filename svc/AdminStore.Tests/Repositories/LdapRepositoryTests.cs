using AdminStore.Helpers;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories.ConfigControl;
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
        #region AuthenticateLdapUserAsync

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_DefaultConnectionSearchDirectoryReturnsTrue_ReturnsSuccess()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Secure)).Throws(new Exception());
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Returns(true).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, true);

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
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Secure)).Throws(new Exception());
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Returns(false).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, true);

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
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.Bind(loginInfo, incorrectPassword, AuthenticationTypes.Secure)).Throws(new Exception());
            authenticator.Setup(a => a.SearchDirectory(loginInfo, incorrectPassword)).Throws(new COMException(null, LdapRepository.ActiveDirectoryInvalidCredentialsErrorCode)).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, incorrectPassword, true);

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
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Secure)).Throws(new Exception());
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Throws(new COMException()).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, true);

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
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Secure)).Throws(new Exception());
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Throws(new Exception()).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, true);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_NoSettingsBindThrowsInvalidCredentials_ReturnsInvalidCredentials()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            LdapSettings[] settings = { };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).ReturnsAsync((IEnumerable<LdapSettings>)settings).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Secure)).Throws(new LdapException(LdapRepository.LdapInvalidCredentialsErrorCode)).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, false);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.InvalidCredentials, status);

        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_NoSettingsBindThrowsOtherLdapException_CallsSearchDirectory()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            string password = "password";
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            LdapSettings[] settings = { };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).ReturnsAsync((IEnumerable<LdapSettings>)settings).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Secure)).Throws(new LdapException()).Verifiable();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Returns(true).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, false);

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
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[]
            {
                new LdapSettings { LdapAuthenticationUrl = "CD=domain", AuthenticationType = AuthenticationTypes.Encryption },
                new LdapSettings { LdapAuthenticationUrl = "DC=wrongdomain", AuthenticationType = AuthenticationTypes.Encryption }
            }; // Covers LdapHelper.MatchesDomain()
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).ReturnsAsync((IEnumerable<LdapSettings>)settings).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, false);

            // Assert
            settingsRepository.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        [TestMethod]
        public async Task AuthenticateLdapUserAsync_SearchLdapReturnsTrueAndBindSucceeds_ReturnsSuccess()
        {
            // Arrange
            var loginInfo = LoginInfo.Parse("domain\\login");
            loginInfo.LdapUrl = "DC=domain";
            string password = "password";
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = loginInfo.LdapUrl, AuthenticationType = AuthenticationTypes.Encryption } };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).ReturnsAsync((IEnumerable<LdapSettings>)settings).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.SearchLdap(settings[0], "(&(objectCategory=user)(samaccountname=" + loginInfo.UserName + "))")).Returns(true).Verifiable();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Encryption)).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, false);

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
            loginInfo.LdapUrl = "DC=domain";
            string password = "password";
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = loginInfo.LdapUrl, AuthenticationType = AuthenticationTypes.Encryption } };
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).ReturnsAsync((IEnumerable<LdapSettings>)settings).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.SearchLdap(settings[0], "(&(objectCategory=user)(samaccountname=" + loginInfo.UserName + "))")).Returns(true).Verifiable();
            authenticator.Setup(a => a.Bind(loginInfo, password, AuthenticationTypes.Encryption)).Throws<Exception>().Verifiable();
            authenticator.Setup(a => a.SearchDirectory(loginInfo, password)).Returns(false).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, false);

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
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC= domain ", EnableCustomSettings = true, AccountNameAttribute = "account" } }; // Covers LdapHelper.GetEffectiveAccountNameAttribute()
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).ReturnsAsync((IEnumerable<LdapSettings>)settings).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.SearchLdap(settings[0], @"(&(objectCategory=user)(account=login\5c\2a\28\29\00\2f))")).Returns(false).Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, false);

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
            var settingsRepository = new Mock<ISqlSettingsRepository>();
            var settings = new[] { new LdapSettings { LdapAuthenticationUrl = "DC=", EnableCustomSettings = true } }; // Covers LdapHelper.GetEffectiveAccountNameAttribute()
            settingsRepository.Setup(r => r.GetLdapSettingsAsync()).ReturnsAsync((IEnumerable<LdapSettings>)settings).Verifiable();
            var authenticator = new Mock<IAuthenticator>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticator.Setup(a => a.SearchLdap(settings[0], @"(&(objectCategory=user)(samaccountname=" + loginInfo.UserName + "))")).Throws<Exception>().Verifiable();
            var repository = new LdapRepository(settingsRepository.Object, logMock.Object, authenticator.Object);

            // Act
            AuthenticationStatus status = await repository.AuthenticateLdapUserAsync(loginInfo.Login, password, false);

            // Assert
            settingsRepository.Verify();
            authenticator.Verify();
            Assert.AreEqual(AuthenticationStatus.Error, status);
        }

        #endregion AuthenticateLdapUserAsync
    }
}
