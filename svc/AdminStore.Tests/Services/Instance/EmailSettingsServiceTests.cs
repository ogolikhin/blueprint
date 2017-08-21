using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.Emails;
using AdminStore.Repositories;
using AdminStore.Services.Email;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.InstanceSettings;
using ServiceLibrary.Services;

namespace AdminStore.Services.Instance
{
    [TestClass]
    public class EmailSettingsServiceTests
    {
        private EmailSettingsService _emailSettingsService;
        private Mock<IEmailHelper> _emailHelperMock;
        private Mock<IIncomingEmailService> _incomingEmailServiceMock;

        private EmailOutgoingSettings _outgoingSettings;
        private EmailIncomingSettings _incomingSettings;
        private InstanceAdminPrivileges _adminPrivilege;
        private User _user;

        private const int UserId = 1;
        private const string WebsiteAddress = "https://blueprintsys.net";
        private const string DecryptedPassword = "DECRYPTED_PASSWORD";
        private const string EncryptedPassword = "fQR9SncMLDQYBY2g0snDP3b63WixRjlmAMh1Ry54fLY=";

        [TestInitialize]
        public void Initialize()
        {
            Mock<IPrivilegesRepository> privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IWebsiteAddressService> websiteAddressServiceMock = new Mock<IWebsiteAddressService>();
            Mock<IInstanceSettingsRepository> instanceSettingsRepositoryMock = new Mock<IInstanceSettingsRepository>();
            _emailHelperMock = new Mock<IEmailHelper>();
            _incomingEmailServiceMock = new Mock<IIncomingEmailService>();

            _emailSettingsService = new EmailSettingsService(new PrivilegesManager(privilegesRepositoryMock.Object),
                                                             userRepositoryMock.Object,
                                                             _emailHelperMock.Object,
                                                             websiteAddressServiceMock.Object,
                                                             instanceSettingsRepositoryMock.Object,
                                                             _incomingEmailServiceMock.Object);

            privilegesRepositoryMock.Setup(repo => repo.GetInstanceAdminPrivilegesAsync(UserId)).ReturnsAsync(() => _adminPrivilege);

            userRepositoryMock.Setup(repo => repo.GetUserAsync(UserId)).ReturnsAsync(() => _user);

            websiteAddressServiceMock.Setup(service => service.GetWebsiteAddress()).Returns(WebsiteAddress);

            instanceSettingsRepositoryMock.Setup(repo => repo.GetEmailSettings()).ReturnsAsync(new EmailSettings()
            {
                Password = EncryptedPassword,
                IncomingPassword = EncryptedPassword
            });

            //Setup Default Values
            _outgoingSettings = new EmailOutgoingSettings()
            {
                AuthenticatedSmtp = true,
                AuthenticatedSmtpPassword = "password",
                AuthenticatedSmtpUsername = "admin",
                EnableSsl = true,
                Port = 2,
                ServerAddress = "smtp.blueprintsys.com",
                IsPasswordDirty = true
            };

            _incomingSettings = new EmailIncomingSettings()
            {
                AccountUsername = "admin",
                AccountPassword = "password",
                EnableSsl = true,
                Port = 2,
                ServerAddress = "mail.test.com",
                ServerType = EmailClientType.Imap,
                IsPasswordDirty = true
            };

            _adminPrivilege = InstanceAdminPrivileges.ManageInstanceSettings;

            _user = new User()
            {
                Email = "test@example.com"
            };
        }

        #region SendTestEmailAsync

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Throw_AuthorizationException_When_User_Doesnt_Have_ManageInstanceSettings_Privilege()
        {
            //Arrange
            _adminPrivilege = InstanceAdminPrivileges.ViewInstanceSettings;

            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
            //Assert
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.Forbidden);
                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Throw_BadRequestException_When_ServerName_Is_Empty()
        {
            //Arrange
            _outgoingSettings.ServerAddress = "";

            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
                //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.OutgoingEmptyMailServer);
                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Throw_BadRequestException_When_Port_Is_Less_Than_1()
        {
            //Arrange
            _outgoingSettings.Port = 0;

            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.OutgoingPortOutOfRange);
                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Throw_BadRequestException_When_Port_Is_Greater_Than_65535()
        {
            //Arrange
            _outgoingSettings.Port = 65536;

            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.OutgoingPortOutOfRange);
                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Throw_BadRequestException_When_AuthenticatedSmtp_Is_Enabled_And_Username_Is_Empty()
        {
            //Arrange
            _outgoingSettings.AuthenticatedSmtpUsername = "";

            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptySmtpAdministratorUsername);
                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Throw_BadRequestException_When_AuthenticatedSmtp_Is_Enabled_And_Password_Is_Empty()
        {
            //Arrange
            _outgoingSettings.AuthenticatedSmtpPassword = "";

            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptySmtpAdministratorPassword);
                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Throw_ConflictException_When_User_Has_No_Email_Address()
        {
            //Arrange
            _user.Email = null;

            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
            //Assert
            catch (ConflictException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.UserHasNoEmail);
                return;
            }

            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Initialize_The_Email_Helper()
        {
            //Act
            await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);

            //Assert
            _emailHelperMock.Verify(helper => helper.Initialize(It.Is<IEmailConfigInstanceSettings>(config => CheckSettings(config))));
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Config_UserName_And_Password_Should_Be_Empty_When_AuthenticatedSmtp_Is_False()
        {
            //Assert
            _outgoingSettings.AuthenticatedSmtp = false;

            //Act
            await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);

            //Assert
            _emailHelperMock.Verify(helper => helper.Initialize(It.Is<IEmailConfigInstanceSettings>(config => CheckSettings(config))));
        }

        private bool CheckSettings(IEmailConfigInstanceSettings config)
        {
            return config.Authenticated == _outgoingSettings.AuthenticatedSmtp &&
                   config.EnableSSL == _outgoingSettings.EnableSsl &&
                   config.HostName == _outgoingSettings.ServerAddress &&
                   config.Password == (_outgoingSettings.AuthenticatedSmtp ? _outgoingSettings.AuthenticatedSmtpPassword : string.Empty) &&
                   config.Port == _outgoingSettings.Port &&
                   config.SenderEmailAddress == _user.Email &&
                   config.UserName == (_outgoingSettings.AuthenticatedSmtp ? _outgoingSettings.AuthenticatedSmtpUsername : string.Empty);
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Config_Password_Should_Come_From_Database_If_Password_Is_Not_Dirty()
        {
            //Assert
            _outgoingSettings.IsPasswordDirty = false;

            //Act
            await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);

            //Assert
            _emailHelperMock.Verify(helper => helper.Initialize(It.Is<IEmailConfigInstanceSettings>(config => CheckSettingsPasswordNotDirty(config))));
        }

        private bool CheckSettingsPasswordNotDirty(IEmailConfigInstanceSettings config)
        {
            return config.Authenticated == _outgoingSettings.AuthenticatedSmtp &&
                   config.EnableSSL == _outgoingSettings.EnableSsl &&
                   config.HostName == _outgoingSettings.ServerAddress &&
                   config.Password == (_outgoingSettings.AuthenticatedSmtp ? DecryptedPassword : string.Empty) &&
                   config.Port == _outgoingSettings.Port &&
                   config.SenderEmailAddress == _user.Email &&
                   config.UserName == (_outgoingSettings.AuthenticatedSmtp ? _outgoingSettings.AuthenticatedSmtpUsername : string.Empty);
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Send_An_Email_To_The_User()
        {
            //Act
            await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);

            //Assert
            _emailHelperMock.Verify(helper => helper.SendEmail(_user.Email, It.IsAny<string>(), It.IsAny<string>()));
        }

        #endregion

        #region CheckingIncomingEmailConnectionAsync

        [TestMethod]
        public async Task CheckIncomingEmailConnectionAsync_Should_Throw_AuthorizationException_When_User_Doesnt_Have_ManageInstanceSettings_Permission()
        {
            //Arrange
            _adminPrivilege = InstanceAdminPrivileges.ViewInstanceSettings;

            //Act
            try
            {
                await _emailSettingsService.TestIncomingEmailConnectionAsync(UserId, _incomingSettings);
            }
            //Assert
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.Forbidden, ex.ErrorCode);

                return;
            }

            Assert.Fail("No AuthorizationException was thrown.");
        }

        [TestMethod]
        public async Task CheckIncomingEmailConnectionAsync_Should_Throw_BadRequestException_When_ServerAddress_Is_Empty()
        {
            //Arrange
            _incomingSettings.ServerAddress = null;

            //Act
            try
            {
                await _emailSettingsService.TestIncomingEmailConnectionAsync(UserId, _incomingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.IncomingEmptyMailServer, ex.ErrorCode);

                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }

        [TestMethod]
        public async Task CheckIncomingEmailConnectionAsync_Should_Throw_BadRequestException_When_Port_Is_Less_Than_1()
        {
            //Arrange
            _incomingSettings.Port = 0;

            //Act
            try
            {
                await _emailSettingsService.TestIncomingEmailConnectionAsync(UserId, _incomingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.IncomingPortOutOfRange, ex.ErrorCode);

                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }

        [TestMethod]
        public async Task CheckIncomingEmailConnectionAsync_Should_Throw_BadRequestException_When_Port_Is_Greater_Than_65535()
        {
            //Arrange
            _incomingSettings.Port = 65536;

            //Act
            try
            {
                await _emailSettingsService.TestIncomingEmailConnectionAsync(UserId, _incomingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.IncomingPortOutOfRange, ex.ErrorCode);

                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }

        [TestMethod]
        public async Task CheckIncomingEmailConnectionAsync_Should_Throw_BadRequestException_When_Username_Is_Empty()
        {
            //Arrange
            _incomingSettings.AccountUsername = null;

            //Act
            try
            {
                await _emailSettingsService.TestIncomingEmailConnectionAsync(UserId, _incomingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.EmptyEmailUsername, ex.ErrorCode);

                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }

        [TestMethod]
        public async Task CheckIncomingEmailConnectionAsync_Should_Throw_BadRequestException_When_Password_Is_Empty()
        {
            //Arrange
            _incomingSettings.AccountPassword = null;

            //Act
            try
            {
                await _emailSettingsService.TestIncomingEmailConnectionAsync(UserId, _incomingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.EmptyEmailPassword, ex.ErrorCode);

                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }

        [TestMethod]
        public async Task CheckIncomingEmailConnectionAsync_Should_TestEmailConnection()
        {
            //Act
            await _emailSettingsService.TestIncomingEmailConnectionAsync(UserId, _incomingSettings);

            //Assert
            _incomingEmailServiceMock.Verify(service => service.TryConnect(It.Is<EmailClientConfig>(config => CheckEmailClientConfig(config))));
        }

        private bool CheckEmailClientConfig(EmailClientConfig config)
        {
            return config.EnableSsl == _incomingSettings.EnableSsl &&
                   config.AccountPassword == _incomingSettings.AccountPassword &&
                   config.AccountUsername == _incomingSettings.AccountUsername &&
                   config.ClientType == _incomingSettings.ServerType &&
                   config.Port == _incomingSettings.Port &&
                   config.ServerAddress == _incomingSettings.ServerAddress;
        }

        [TestMethod]
        public async Task CheckIncomingEmailConnectionAsync_Should_Use_Password_From_Repository_When_IsPasswordDirty_Is_False()
        {
            //Arrange
            _incomingSettings.IsPasswordDirty = false;

            //Act
            await _emailSettingsService.TestIncomingEmailConnectionAsync(UserId, _incomingSettings);

            //Assert
            _incomingEmailServiceMock.Verify(service => service.TryConnect(It.Is<EmailClientConfig>(config => CheckEmailClientConfigWithDirtyPassword(config))));
        }

        private bool CheckEmailClientConfigWithDirtyPassword(EmailClientConfig config)
        {
            return config.EnableSsl == _incomingSettings.EnableSsl &&
                   config.AccountPassword == DecryptedPassword &&
                   config.AccountUsername == _incomingSettings.AccountUsername &&
                   config.ClientType == _incomingSettings.ServerType &&
                   config.Port == _incomingSettings.Port &&
                   config.ServerAddress == _incomingSettings.ServerAddress;
        }

        #endregion
    }
}
