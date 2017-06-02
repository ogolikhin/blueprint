using System;
using System.Security.Principal;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Saml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Helpers;
using ServiceLibrary.Exceptions;
using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    [TestClass]
    public class AuthenticationRepositoryTests
    {
        private static Mock<ISqlUserRepository> _sqlUserRepositoryMock;
        private static Mock<ISqlSettingsRepository> _sqlSettingsRepositoryMock;
        private static Mock<ILdapRepository> _ldapRepositoryMock;
        private static Mock<ISamlRepository> _samlRepositoryMock;
        private static Mock<IServiceLogRepository> _logRepositoryMock;
        private static Mock<IApplicationSettingsRepository> _applicationSettingsRepositoryMock;
        private IAuthenticationRepository _authenticationRepository;

        private const string Login = "admin";
        private const string Password = "changeme";
        private const string NewPassword = "123EWQ!@#";
        private const string NewPreviouslyUsedPassword = "$PreviouslyUsed99";
        private const string HashedPassword = "ALqCo8odf0FtFQBndSz1dH8P2bSIDmSqjGjTj4fj+Ao=";
        private static readonly Guid UserSalt = Guid.Parse("39CAB5B0-076E-403F-B682-A761CF34E466");

        private const string PasswordChangeCooldownInHoursKey = "PasswordChangeCooldownInHours";
        private const string DefaultPasswordChangeCooldownInHours = "24";

        private static InstanceSettings _instanceSettings;
        private static IEnumerable<ApplicationSetting> _applicationSettings;
        private static AuthenticationUser _loginUser;

        [TestInitialize]
        public void Init()
        {
            _loginUser = new AuthenticationUser
            {
                Id = 1,
                Login = Login,
                UserSalt = UserSalt,
                Password = HashedPassword,
                IsEnabled = true
            };

            _sqlUserRepositoryMock = new Mock<ISqlUserRepository>();
            _sqlUserRepositoryMock
                .Setup(m => m.GetUserByLoginAsync(Login))
                .ReturnsAsync(_loginUser);
            _sqlUserRepositoryMock
                .Setup(m => m.ValidateUserPasswordForHistoryAsync(It.IsAny<int>(), It.Is<string>(p => p != NewPreviouslyUsedPassword)))
                .ReturnsAsync(true);
            _sqlUserRepositoryMock
                .Setup(m => m.ValidateUserPasswordForHistoryAsync(It.IsAny<int>(), It.Is<string>(p => p == NewPreviouslyUsedPassword)))
                .ReturnsAsync(false);

            _instanceSettings = new InstanceSettings
            {
                MaximumInvalidLogonAttempts = 5
            };

            _sqlSettingsRepositoryMock = new Mock<ISqlSettingsRepository>();
            _sqlSettingsRepositoryMock
                .Setup(m => m.GetInstanceSettingsAsync())
                .ReturnsAsync(_instanceSettings);

            _ldapRepositoryMock = new Mock<ILdapRepository>();
            _samlRepositoryMock = new Mock<ISamlRepository>();
            _logRepositoryMock = new Mock<IServiceLogRepository>();

            _applicationSettings = new ApplicationSetting[]
            {
                new ApplicationSetting
                {
                    Key = PasswordChangeCooldownInHoursKey,
                    Value = DefaultPasswordChangeCooldownInHours
                }
            };

            _applicationSettingsRepositoryMock = new Mock<IApplicationSettingsRepository>();
            _applicationSettingsRepositoryMock
                .Setup(m => m.GetSettingsAsync(false))
                .Returns(() => Task.Run(() => _applicationSettings));

            _authenticationRepository = new AuthenticationRepository(_sqlUserRepositoryMock.Object,
                                                                     _sqlSettingsRepositoryMock.Object,
                                                                     _ldapRepositoryMock.Object,
                                                                     _samlRepositoryMock.Object,
                                                                     _logRepositoryMock.Object,
                                                                     _applicationSettingsRepositoryMock.Object);
        }

        #region AuthenticateUserAsync

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_DatabaseUser_EmptyLogin_InvalidCredentialException()
        {
            // Arrange

            // Act
            await _authenticationRepository.AuthenticateUserAsync("", Password);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_EmptyPassword_InvalidCredentialException()
        {
            // Arrange

            // Act
            await _authenticationRepository.AuthenticateUserAsync(Login, "");

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_UserDoesNotExist_InvalidCredentialException()
        {
            // Arrange
            const string fakeLogin = "fakeLogin";
            _sqlUserRepositoryMock.Setup(m => m.GetUserByLoginAsync(fakeLogin)).ReturnsAsync(null);

            // Act
            await _authenticationRepository.AuthenticateUserAsync(fakeLogin, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_FedAuthMustBeUsed_AuthenticationException()
        {
            // Arrange
            _instanceSettings.IsSamlEnabled = true;
            _loginUser.IsFallbackAllowed = false;

            // Act
            await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_DatabaseUser_Success()
        {
            // Arrange
            var userLicense = 3;
            _sqlUserRepositoryMock.Setup(ur => ur.GetEffectiveUserLicenseAsync(It.IsAny<int>())).ReturnsAsync(userLicense);

            _instanceSettings.IsSamlEnabled = true;
            _loginUser.IsFallbackAllowed = true;
            _loginUser.Source = UserGroupSource.Database;

            // Act
            var result = await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            Assert.AreEqual(_loginUser, result);
            Assert.AreEqual(userLicense, result.LicenseType);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_LdapIntegrationDisabled_AuthenticationException()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Windows;
            _instanceSettings.IsLdapIntegrationEnabled = false;

            // Act
            await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_WindowsUser_Success()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Windows;
            _instanceSettings.IsLdapIntegrationEnabled = true;

            _ldapRepositoryMock.Setup(m => m.AuthenticateLdapUserAsync(Login, Password, false))
                .ReturnsAsync(AuthenticationStatus.Success);

            // Act
            var result = await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_WindowsUser_Success_DisabledUser()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Windows;
            _instanceSettings.IsLdapIntegrationEnabled = true;
            _loginUser.IsEnabled = false;

            _ldapRepositoryMock
                .Setup(m => m.AuthenticateLdapUserAsync(Login, Password, false))
                .ReturnsAsync(AuthenticationStatus.Success);

            // Act
            try
            {
                await _authenticationRepository.AuthenticateUserAsync(Login, Password);
            }
            catch (AuthenticationException ex)
            {
                Assert.IsTrue(ex.ErrorCode == ErrorCodes.AccountIsLocked);
                return;
            }
            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_UnknownAuthenticationSource_AuthenticationException()
        {
            // Arrange
            _loginUser.Source = (UserGroupSource)999;

            // Act
            await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_WrongPassword_InvalidCredentialException()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Database;
            const string dummyPassword = "dummyPassword";

            // Act
            await _authenticationRepository.AuthenticateUserAsync(Login, dummyPassword);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_UserLockedOut_AuthenticationException()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.IsEnabled = false;

            // Act
            await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_PasswordExpired_AuthenticationException()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.ExpirePassword = true;
            _loginUser.LastPasswordChangeTimestamp = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2));
            _instanceSettings.PasswordExpirationInDays = 1;

            // Act
            await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_PasswordNotExpired_Success()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.ExpirePassword = true;
            _loginUser.LastPasswordChangeTimestamp = DateTime.UtcNow;
            _instanceSettings.PasswordExpirationInDays = 1;

            // Act
            var result = await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_PasswordNotExpired_ExpirePasswordDisabled_Success()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.ExpirePassword = false;
            _instanceSettings.PasswordExpirationInDays = 1;

            // Act
            var result = await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_PasswordNotExpired_LastPasswordChangeTimestampNull_Success()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.ExpirePassword = true;
            _loginUser.LastPasswordChangeTimestamp = null;
            _instanceSettings.PasswordExpirationInDays = 1;

            // Act
            var result = await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserAsync_WindowsUser_UnknownError_AuthenticationException()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Windows;
            _instanceSettings.IsLdapIntegrationEnabled = true;

            _ldapRepositoryMock
                .Setup(m => m.AuthenticateLdapUserAsync(Login, Password, false))
                .ReturnsAsync(AuthenticationStatus.Error);

            // Act
            await _authenticationRepository.AuthenticateUserAsync(Login, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_LockUser()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.InvalidLogonAttemptsNumber = _instanceSettings.MaximumInvalidLogonAttempts;
            const string dummyPassword = "dummyPassword";

            // Act & Assert
            try
            {
                await _authenticationRepository.AuthenticateUserAsync(Login, dummyPassword);
            }
            catch
            {
                _sqlUserRepositoryMock.Verify(m => m.UpdateUserOnInvalidLoginAsync(It.Is<AuthenticationUser>(u => u.IsEnabled == false)));
            }
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_UserNotBeLockedOutDueInvalidCredentials()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Database;
            _instanceSettings.MaximumInvalidLogonAttempts = 0;
            const string dummyPassword = "dummyPassword";

            // Act & Assert
            try
            {
                await _authenticationRepository.AuthenticateUserAsync(Login, dummyPassword);
            }
            catch
            {
                _sqlUserRepositoryMock.Verify(m => m.UpdateUserOnInvalidLoginAsync(_loginUser), Times.Never);
            }
        }

        [TestMethod]
        public async Task AuthenticateUserAsync_ResetInvalidLogonAttempts()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Database;
            _loginUser.InvalidLogonAttemptsNumber = 999;
            _loginUser.LastInvalidLogonTimeStamp = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2));

            const string dummyPassword = "dummyPassword";

            // Act & Assert
            try
            {
                await _authenticationRepository.AuthenticateUserAsync(Login, dummyPassword);
            }
            catch
            {
                _sqlUserRepositoryMock.Verify(m => m.UpdateUserOnInvalidLoginAsync(It.Is<AuthenticationUser>(u => u.InvalidLogonAttemptsNumber == 1)));
            }
        }

        #endregion

        #region AuthenticateSamlUserAsync

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateSamlUserAsync_SamlDisabled_AuthenticationException()
        {
            // Arrange

            // Act
            await _authenticationRepository.AuthenticateSamlUserAsync("fakeSamlResponce");

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public async Task AuthenticateSamlUserAsync_SamlResponseIsNull_FormatException()
        {
            // Arrange

            // Act
            await _authenticationRepository.AuthenticateSamlUserAsync(null);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateSamlUserAsync_NoFedAuthSettings_AuthenticationException()
        {
            // Arrange
            _instanceSettings.IsSamlEnabled = true;

            _sqlSettingsRepositoryMock
                .Setup(m => m.GetFederatedAuthenticationSettingsAsync())
                .ReturnsAsync(null);

            // Act
            await _authenticationRepository.AuthenticateSamlUserAsync("fakeSamlResponce");

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task AuthenticateSamlUserAsync_UserIsEnabled_ReturnsUser()
        {
            // Arrange
            var userLicense = 2;
            _sqlUserRepositoryMock.Setup(ur => ur.GetEffectiveUserLicenseAsync(It.IsAny<int>())).ReturnsAsync(userLicense);

            _instanceSettings.IsSamlEnabled = true;
            const string samlEncodedResponse = "fakeSamlResponce";

            var xml = SerializationHelper.Serialize(new FederatedAuthenticationSettings.FASettings());
            var fedAuthSettings = new FederatedAuthenticationSettings(xml, null);
            _sqlSettingsRepositoryMock.Setup(m => m.GetFederatedAuthenticationSettingsAsync())
                .ReturnsAsync(fedAuthSettings);
            var identityMock = new Mock<IIdentity>();
            identityMock.SetupGet(p => p.Name).Returns(Login);
            var principalMock = new Mock<IPrincipal>();
            principalMock.SetupGet(p => p.Identity).Returns(identityMock.Object);

            _samlRepositoryMock.Setup(m => m.ProcessEncodedResponse(samlEncodedResponse, fedAuthSettings)).Returns(principalMock.Object);

            // Act
            var result = await _authenticationRepository.AuthenticateSamlUserAsync(samlEncodedResponse);

            // Assert
            Assert.AreEqual(_loginUser, result);
            Assert.AreEqual(userLicense, result.LicenseType);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateSamlUserAsync_UserIsDisabled_AuthenticationException()
        {
            // Arrange
            _instanceSettings.IsSamlEnabled = true;
            const string samlEncodedResponse = "fakeSamlResponce";

            _sqlUserRepositoryMock.Setup(m => m.GetUserByLoginAsync(Login)).ReturnsAsync(new AuthenticationUser { IsEnabled = false });
            var xml = SerializationHelper.Serialize(new FederatedAuthenticationSettings.FASettings());
            var fedAuthSettings = new FederatedAuthenticationSettings(xml, null);
            _sqlSettingsRepositoryMock.Setup(m => m.GetFederatedAuthenticationSettingsAsync())
                .ReturnsAsync(fedAuthSettings);
            var identityMock = new Mock<IIdentity>();
            identityMock.SetupGet(p => p.Name).Returns(Login);
            var principalMock = new Mock<IPrincipal>();
            principalMock.SetupGet(p => p.Identity).Returns(identityMock.Object);

            _samlRepositoryMock.Setup(m => m.ProcessEncodedResponse(samlEncodedResponse, fedAuthSettings)).Returns(principalMock.Object);

            // Act
            await _authenticationRepository.AuthenticateSamlUserAsync(samlEncodedResponse);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateSamlUserAsync_UserIsNull_AuthenticationException()
        {
            // Arrange
            _instanceSettings.IsSamlEnabled = true;
            const string samlEncodedResponse = "fakeSamlResponce";

            var xml = SerializationHelper.Serialize(new FederatedAuthenticationSettings.FASettings());
            var fedAuthSettings = new FederatedAuthenticationSettings(xml, null);
            _sqlSettingsRepositoryMock.Setup(m => m.GetFederatedAuthenticationSettingsAsync())
                .ReturnsAsync(fedAuthSettings);
            var identityMock = new Mock<IIdentity>();
            identityMock.SetupGet(p => p.Name).Returns("Unknown");
            var principalMock = new Mock<IPrincipal>();
            principalMock.SetupGet(p => p.Identity).Returns(identityMock.Object);

            _samlRepositoryMock.Setup(m => m.ProcessEncodedResponse(samlEncodedResponse, fedAuthSettings)).Returns(principalMock.Object);

            // Act
            await _authenticationRepository.AuthenticateSamlUserAsync(samlEncodedResponse);

            // Assert
        }

        [TestMethod]
        public async Task AuthenticateSamlUserAsync_NoDomain_ReturnsUser()
        {
            // Arrange
            var userLicense = 2;
            _sqlUserRepositoryMock.Setup(ur => ur.GetEffectiveUserLicenseAsync(It.IsAny<int>())).ReturnsAsync(userLicense);

            _instanceSettings.IsSamlEnabled = true;
            const string samlEncodedResponse = "fakeSamlResponce";
            const string customDomainName = "TEST";

            var xml = SerializationHelper.Serialize(new FederatedAuthenticationSettings.FASettings
            {
                DomainList = new List<FederatedAuthenticationSettings.FAAllowedDomian>
                {
                    new FederatedAuthenticationSettings.FAAllowedDomian { Name = customDomainName, OrderIndex = 0}
                },
                IsAllowingNoDomain = true
            });
            var fedAuthSettings = new FederatedAuthenticationSettings(xml, null);
            _sqlSettingsRepositoryMock.Setup(m => m.GetFederatedAuthenticationSettingsAsync())
                .ReturnsAsync(fedAuthSettings);
            var identityMock = new Mock<IIdentity>();
            identityMock.SetupGet(p => p.Name).Returns(Login);
            var principalMock = new Mock<IPrincipal>();
            principalMock.SetupGet(p => p.Identity).Returns(identityMock.Object);
            _sqlUserRepositoryMock
                .Setup(m => m.GetUserByLoginAsync($"{customDomainName}\\{Login}"))
                .ReturnsAsync(_loginUser);

            _samlRepositoryMock.Setup(m => m.ProcessEncodedResponse(samlEncodedResponse, fedAuthSettings)).Returns(principalMock.Object);

            // Act
            var result = await _authenticationRepository.AuthenticateSamlUserAsync(samlEncodedResponse);

            // Assert
            Assert.AreEqual(_loginUser, result);
            Assert.AreEqual(userLicense, result.LicenseType);
        }

        #endregion

        #region AuthenticateUserForResetAsync

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserForResetAsync_DatabaseUser_EmptyLogin_InvalidCredentialException()
        {
            // Arrange

            // Act
            await _authenticationRepository.AuthenticateUserForResetAsync("", Password);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserForResetAsync_UserDoesNotExist_InvalidCredentialException()
        {
            // Arrange
            const string fakeLogin = "fakeLogin";
            _sqlUserRepositoryMock.Setup(m => m.GetUserByLoginAsync(fakeLogin)).ReturnsAsync(null);

            // Act
            await _authenticationRepository.AuthenticateUserForResetAsync(fakeLogin, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserForResetAsync_FedAuthMustBeUsed_AuthenticationException()
        {
            // Arrange
            _instanceSettings.IsSamlEnabled = true;
            _loginUser.IsFallbackAllowed = false;

            // Act
            await _authenticationRepository.AuthenticateUserForResetAsync(Login, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task AuthenticateUserForResetAsync_DatabaseUser_Success()
        {
            // Arrange
            _instanceSettings.IsSamlEnabled = true;
            _loginUser.IsFallbackAllowed = true;
            _loginUser.Source = UserGroupSource.Database;

            // Act
            var result = await _authenticationRepository.AuthenticateUserForResetAsync(Login, Password);

            // Assert
            Assert.AreEqual(_loginUser, result);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserForResetAsync_LockedDatabaseUser_AuthenticationException()
        {
            // Arrange
            _instanceSettings.IsSamlEnabled = true;
            _loginUser.IsFallbackAllowed = true;
            _loginUser.IsEnabled = false;
            _loginUser.Source = UserGroupSource.Database;

            // Act
            await _authenticationRepository.AuthenticateUserForResetAsync(Login, Password);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserForResetAsync_WindowsSource_AuthenticationException()
        {
            // Arrange
            _loginUser.Source = UserGroupSource.Windows;

            // Act
            await _authenticationRepository.AuthenticateUserForResetAsync(Login, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task AuthenticateUserForResetAsync_UnknownSource_AuthenticationException()
        {
            // Arrange
            int wrongType = -1;
            _loginUser.Source = (UserGroupSource)wrongType;

            // Act
            await _authenticationRepository.AuthenticateUserForResetAsync(Login, Password);

            // Assert
            // Exception
        }

        #endregion

        #region ResetPassword

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task ResetPassword_EmptyPassword_BadRequestException()
        {
            // Arrange

            // Act
            await _authenticationRepository.ResetPassword(_loginUser, Password, "");

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task ResetPassword_RepeatingPassword_BadRequestException()
        {
            // Arrange

            // Act
            await _authenticationRepository.ResetPassword(_loginUser, Password, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task ResetPassword_InvalidPassword_BadRequestException()
        {
            // Arrange

            // Act
            await _authenticationRepository.ResetPassword(_loginUser, NewPassword, Password);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task ResetPassword_ValidPassword_Success()
        {
            // Arrange
            Exception exception = null;

            // Act
            try
            {
                await _authenticationRepository.ResetPassword(_loginUser, Password, NewPassword);
            }
            catch (Exception ex)
            {

                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        public async Task ResetPassword_CooldownInEffect_BadRequestException()
        {
            // Arrange
            ConflictException exception = null;
            _loginUser.LastPasswordChangeTimestamp = DateTime.UtcNow.AddHours(-12);
            _applicationSettingsRepositoryMock
                .Setup(repo => repo.GetValue(PasswordChangeCooldownInHoursKey, It.IsAny<int>()))
                .ReturnsAsync(24);

            // Act
            try
            {
                await _authenticationRepository.ResetPassword(_loginUser, Password, NewPassword);
            }
            catch(ConflictException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.ChangePasswordCooldownInEffect, exception.ErrorCode);
        }

        [TestMethod]
        public async Task ResetPassword_CooldownOverNoApplicationSettings_Success()
        {
            // Arrange
            Exception exception = null;

            _loginUser.LastPasswordChangeTimestamp = DateTime.UtcNow.AddHours(-24);

            _applicationSettingsRepositoryMock
                .Setup(m => m.GetSettingsAsync(false))
                .Returns(() => Task.Run(() => Enumerable.Empty<ApplicationSetting>()));

            // Act
            try
            {
                await _authenticationRepository.ResetPassword(_loginUser, Password, NewPassword);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        public async Task ResetPassword_CooldownOverInvalidApplicationSettings_Success()
        {
            // Arrange
            Exception exception = null;

            _loginUser.LastPasswordChangeTimestamp = DateTime.UtcNow.AddHours(-24);

            _applicationSettingsRepositoryMock
                .Setup(m => m.GetSettingsAsync(false))
                .Returns(() => Task.Run(() => new ApplicationSetting[] 
                {
                    new ApplicationSetting
                    {
                        Key = PasswordChangeCooldownInHoursKey,
                        Value = "value"
                    }
                }.AsEnumerable()));

            // Act
            try
            {
                await _authenticationRepository.ResetPassword(_loginUser, Password, NewPassword);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        public async Task ResetPassword_CooldownOver_Success()
        {
            // Arrange
            Exception exception = null;
            _loginUser.LastPasswordChangeTimestamp = DateTime.UtcNow.AddHours(-24);

            // Act
            try
            {
                await _authenticationRepository.ResetPassword(_loginUser, Password, NewPassword);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task ResetPassword_PreviouslyUsedPassword_BadRequestException()
        {
            // Arrange

            try
            {
                // Act
                await _authenticationRepository.ResetPassword(_loginUser, Password, NewPreviouslyUsedPassword);
            }
            catch (BadRequestException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.PasswordAlreadyUsedPreviously, e.ErrorCode);
                throw;
            }
        }

        #endregion
    }
}
