using System;
using System.Security.Principal;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Saml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AdminStore.Repositories
{
    [TestClass]
    public class AuthenticationRepositoryTests
    {
        private static Mock<ISqlUserRepository> _sqlUserRepositoryMock;
        private static Mock<ISqlSettingsRepository> _sqlSettingsRepositoryMock;
        private static Mock<ILdapRepository> _ldapRepositoryMock;
        private static Mock<ISamlRepository> _samlRepositoryMock;
        private const string Login = "admin";
        private const string Password = "changeme";
        private const string HashedPassword = "ALqCo8odf0FtFQBndSz1dH8P2bSIDmSqjGjTj4fj+Ao=";
        private static readonly Guid UserSalt = Guid.Parse("39CAB5B0-076E-403F-B682-A761CF34E466");

        private static InstanceSettings _instanceSettings;

        private static LoginUser _loginUser;

        [TestInitialize]
        public void Init()
        {
            _instanceSettings = new InstanceSettings { MaximumInvalidLogonAttempts = 5 };
            _loginUser = new LoginUser { Id = 1, Login = Login, UserSalt = UserSalt, Password = HashedPassword, IsEnabled = true };

            _sqlUserRepositoryMock = new Mock<ISqlUserRepository>();
            _sqlUserRepositoryMock.Setup(m => m.GetUserByLoginAsync(Login)).ReturnsAsync(_loginUser);
            _sqlSettingsRepositoryMock = new Mock<ISqlSettingsRepository>();

            _sqlSettingsRepositoryMock.Setup(m => m.GetInstanceSettingsAsync()).ReturnsAsync(_instanceSettings);

            _ldapRepositoryMock = new Mock<ILdapRepository>();
            _samlRepositoryMock = new Mock<ISamlRepository>();
        }

        #region AuthenticateUserAsync

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public async Task AuthenticateUserAsync_DatabaseUser_EmptyLogin_InvalidCredentialException()
        {
            //Arrange
            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync("", Password);

            //Assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public async Task AuthenticateUserAsync_EmptyPassword_InvalidCredentialException()
        {
            //Arrange
            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync(Login, "");

            //Assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_UserDoesNotExist_InvalidCredentialException()
        {
            //Arrange
            const string fakeLogin = "fakeLogin";
            _sqlUserRepositoryMock.Setup(m => m.GetUserByLoginAsync(fakeLogin)).ReturnsAsync(null);
            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync(fakeLogin, Password);

            //Assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_Fed_Auth_MustBeUsed_AuthenticationException()
        {
            //Arrange
            _instanceSettings.IsSamlEnabled = true;
            _loginUser.IsFallbackAllowed = false;

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            //Exception
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_DatabaseUser_Success()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            var result = await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_LdapIntegrationDisabled_AuthenticationException()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Windows;
            _instanceSettings.IsLdapIntegrationEnabled = false;

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            //Exception
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_WindowsUser_Success()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Windows;
            _instanceSettings.IsLdapIntegrationEnabled = true;

            _ldapRepositoryMock.Setup(m => m.AuthenticateLdapUserAsync(Login, Password, false))
                .ReturnsAsync(AuthenticationStatus.Success);

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            var result = await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_UnknownAuthenticationSource_AuthenticationException()
        {
            //Arrange
            _loginUser.Source = (UserGroupSource)999;
            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_WrongPassword_InvalidCredentialException()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;
            const string dummyPassword = "dummyPassword";
            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync(Login, dummyPassword);

            //Assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_UserLockedOut_AuthenticationException()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.IsEnabled = false;
            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_PasswordExpired_AuthenticationException()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.ExpirePassword = true;
            _loginUser.LastPasswordChangeTimestamp = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2));
            _instanceSettings.PasswordExpirationInDays = 1;

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            //Exception
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_PasswordNotExpired_Success()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.ExpirePassword = true;
            _loginUser.LastPasswordChangeTimestamp = DateTime.UtcNow;
            _instanceSettings.PasswordExpirationInDays = 1;

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            var result = await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_PasswordNotExpired_ExpirePasswordDisabled_Success()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.ExpirePassword = false;
            _instanceSettings.PasswordExpirationInDays = 1;

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            var result = await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_PasswordNotExpired_LastPasswordChangeTimestampNull_Success()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.ExpirePassword = true;
            _loginUser.LastPasswordChangeTimestamp = null;
            _instanceSettings.PasswordExpirationInDays = 1;

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            var result = await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_WindowsUser_UnknownError_AuthenticationException()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Windows;
            _instanceSettings.IsLdapIntegrationEnabled = true;

            _ldapRepositoryMock.Setup(m => m.AuthenticateLdapUserAsync(Login, Password, false))
                .ReturnsAsync(AuthenticationStatus.Error);

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateUserAsync(Login, Password);

            //Assert
            //Exception
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_LockUser()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.InvalidLogonAttemptsNumber = _instanceSettings.MaximumInvalidLogonAttempts;
            const string dummyPassword = "dummyPassword";

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act & Assert
            try
            {
                await authenticationRepository.AuthenticateUserAsync(Login, dummyPassword);
            }
            catch
            {
                _sqlUserRepositoryMock.Verify(m => m.UpdateUserOnInvalidLoginAsync(It.Is<LoginUser>(u => u.IsEnabled == false)));
            }
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_UserNotBeLockedOutDueInvalidCredentials()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;
            _instanceSettings.MaximumInvalidLogonAttempts = 0;
            const string dummyPassword = "dummyPassword";

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act & Assert
            try
            {
                await authenticationRepository.AuthenticateUserAsync(Login, dummyPassword);
            }
            catch
            {
                _sqlUserRepositoryMock.Verify(m => m.UpdateUserOnInvalidLoginAsync(_loginUser), Times.Never);
            }
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_ResetInvalidLogonAttempts()
        {
            //Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.InvalidLogonAttemptsNumber = 999;
            _loginUser.LastInvalidLogonTimeStamp = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2));

            const string dummyPassword = "dummyPassword";

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act & Assert
            try
            {
                await authenticationRepository.AuthenticateUserAsync(Login, dummyPassword);
            }
            catch
            {
                _sqlUserRepositoryMock.Verify(m => m.UpdateUserOnInvalidLoginAsync(It.Is<LoginUser>(u => u.InvalidLogonAttemptsNumber == 1)));
            }
        }

        #endregion

        #region AuthenticateSamlUserAsync

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateSamlUserAsync_SamlDisabled_AuthenticationException()
        {
            //Arrange
            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateSamlUserAsync("fakeSamlResponce");

            //Assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateSamlUserAsync_NoFedAuthSettings_AuthenticationException()
        {
            //Arrange
            _instanceSettings.IsSamlEnabled = true;

            _sqlSettingsRepositoryMock.Setup(m => m.GetFederatedAuthenticationSettingsAsync())
                .ReturnsAsync(null);
            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            await authenticationRepository.AuthenticateSamlUserAsync("fakeSamlResponce");

            //Assert
            //Exception
        }

        [TestMethod]
        public async Task AuthenticateSamlUserAsync_Success()
        {
            //Arrange
            _instanceSettings.IsSamlEnabled = true;
            const string samlEncodedResponse = "fakeSamlResponce";

            var xml = SerializationHelper.Serialize(new SerializationHelper.FASettings());
            var fedAuthSettings = new FederatedAuthenticationSettings(xml, null);
            _sqlSettingsRepositoryMock.Setup(m => m.GetFederatedAuthenticationSettingsAsync())
                .ReturnsAsync(fedAuthSettings);
            var identityMock = new Mock<IIdentity>();
            identityMock.SetupGet(p => p.Name).Returns(Login);
            var principalMock = new Mock<IPrincipal>();
            principalMock.SetupGet(p => p.Identity).Returns(identityMock.Object);

            _samlRepositoryMock.Setup(m => m.ProcessEncodedResponse(samlEncodedResponse, fedAuthSettings)).Returns(principalMock.Object);

            var authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                        _sqlSettingsRepositoryMock.Object,
                                                                        _ldapRepositoryMock.Object,
                                                                        _samlRepositoryMock.Object);
            //Act
            var result = await authenticationRepository.AuthenticateSamlUserAsync(samlEncodedResponse);

            //Assert
            Assert.AreEqual(_loginUser, result);
        }

        #endregion
    }
}
