using System;
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
        private Mock<IInstanceSettingsRepository> _instanceSettingsRepositoryMock;

        private EmailOutgoingSettings _outgoingSettings;
        private EmailIncomingSettings _incomingSettings;
        private InstanceAdminPrivileges _adminPrivilege;
        private User _user;
        private EmailSettings _emailSettings;
        private EmailSettingsDto _emailSettingsDto;

        private const int UserId = 1;
        private const string TestEmailSubject = "Blueprint Test Email";

        private const string WebsiteAddress = "https://blueprintsys.net";
        private const string DecryptedPassword = "DECRYPTED_PASSWORD";
        private const string EncryptedPassword = "fQR9SncMLDQYBY2g0snDP3b63WixRjlmAMh1Ry54fLY=";

        [TestInitialize]
        public void Initialize()
        {
            Mock<IPrivilegesRepository> privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IWebsiteAddressService> websiteAddressServiceMock = new Mock<IWebsiteAddressService>();
            _instanceSettingsRepositoryMock = new Mock<IInstanceSettingsRepository>();
            _emailHelperMock = new Mock<IEmailHelper>();
            _incomingEmailServiceMock = new Mock<IIncomingEmailService>();

            _emailSettingsService = new EmailSettingsService(new PrivilegesManager(privilegesRepositoryMock.Object),
                                                             userRepositoryMock.Object,
                                                             _emailHelperMock.Object,
                                                             websiteAddressServiceMock.Object,
                                                             _instanceSettingsRepositoryMock.Object,
                                                             _incomingEmailServiceMock.Object);

            privilegesRepositoryMock.Setup(repo => repo.GetInstanceAdminPrivilegesAsync(UserId)).ReturnsAsync(() => _adminPrivilege);

            userRepositoryMock.Setup(repo => repo.GetUserAsync(UserId)).ReturnsAsync(() => _user);

            websiteAddressServiceMock.Setup(service => service.GetWebsiteAddress()).Returns(WebsiteAddress);

            _instanceSettingsRepositoryMock.Setup(repo => repo.GetEmailSettings()).ReturnsAsync(() => _emailSettings);

            //Setup Default Values
            _outgoingSettings = new EmailOutgoingSettings()
            {
                AuthenticatedSmtp = true,
                AccountPassword = "password",
                AccountUsername = "admin",
                AccountEmailAddress = "test@example.com",
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

            _emailSettings = new EmailSettings()
            {
                Password = EncryptedPassword,
                IncomingPassword = EncryptedPassword,
                Authenticated = true,
                EnableEmailDiscussion = true,
                EnableEmailReplies = true,
                EnableNotifications = true,
                EnableSSL = true,
                SenderEmailAddress = "example@test.com",
                HostName = "smtp.test.com",
                Port = 1234,
                UserName = "admin",
                IncomingEnableSSL = false,
                IncomingHostName = "pop3.test.com",
                IncomingPort = 2345,
                IncomingServerType = 0,
                IncomingUserName = "user"
            };

            _emailSettingsDto = new EmailSettingsDto()
            {
                EnableDiscussions = false,
                EnableEmailNotifications = false,
                EnableReviewNotifications = false,
                Incoming = new EmailIncomingSettings()
                {
                    AccountPassword = "12345",
                    AccountUsername = "admin",
                    EnableSsl = true,
                    IsPasswordDirty = true,
                    Port = 8765,
                    ServerAddress = "imap.test.com",
                    ServerType = EmailClientType.Pop3
                },
                Outgoing = new EmailOutgoingSettings()
                {
                    AccountEmailAddress = "admin@example.com",
                    AccountPassword = "apassword",
                    AccountUsername = "adminuser",
                    AuthenticatedSmtp = true,
                    EnableSsl = true,
                    IsPasswordDirty = true,
                    Port = 9876,
                    ServerAddress = "mail.test.com"
                }
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
            _outgoingSettings.AccountUsername = "";

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
            _outgoingSettings.AccountPassword = "";

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
        public async Task SendTestEmailAsync_Should_Throw_BadRequestException_When_EmailAddress_Is_Empty()
        {
            //Arrange
            _outgoingSettings.AccountEmailAddress = null;

            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptyEmailAddress);
                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Throw_BadRequestException_When_EmailAddress_Is_Not_An_Email()
        {
            //Arrange
            _outgoingSettings.AccountEmailAddress = "notanemail";

            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.InvalidEmailAddress);
                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
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
                   config.Password == (_outgoingSettings.AuthenticatedSmtp ? _outgoingSettings.AccountPassword : string.Empty) &&
                   config.Port == _outgoingSettings.Port &&
                   config.SenderEmailAddress == _user.Email &&
                   config.UserName == (_outgoingSettings.AuthenticatedSmtp ? _outgoingSettings.AccountUsername : string.Empty);
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
                   config.UserName == (_outgoingSettings.AuthenticatedSmtp ? _outgoingSettings.AccountUsername : string.Empty);
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Send_An_Email_To_The_User()
        {
            //Act
            await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);

            //Assert
            _emailHelperMock.Verify(helper => helper.SendEmail(_user.Email, TestEmailSubject, It.IsAny<string>()));
        }

        [TestMethod]
        public async Task SendTestEmailAsync_Should_Throw_Bad_Request_Exception_When_EmailHelper_Throws_EmailException()
        {
            //Arrange
            _emailHelperMock.Setup(helper => helper.SendEmail(_user.Email, TestEmailSubject, It.IsAny<string>())).Throws(new EmailException("Error Message", ErrorCodes.OutgoingMailError));
            
            //Act
            try
            {
                await _emailSettingsService.SendTestEmailAsync(UserId, _outgoingSettings);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual("Error Message", ex.Message);
                Assert.AreEqual(ErrorCodes.OutgoingMailError, ex.ErrorCode);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
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

        #region GetEmailSettingsAsync

        [TestMethod]
        public async Task GetEmailSettingsAsync_Should_Throw_AuthorizationException_When_User_Doesnt_Have_ViewInstanceSettings()
        {
            //Arrange
            _adminPrivilege = InstanceAdminPrivileges.ManageProjects;

            //Act
            try
            {
                await _emailSettingsService.GetEmailSettingsAsync(UserId);
            }
            //Assert
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.Forbidden);

                return;
            }

            Assert.Fail("AuthorizationException was not thrown.");
        }

        [TestMethod]
        public async Task GetEmailSettingsAsync_Should_Get_EmailSettings_Information_From_Repository()
        {
            //Arrange
            _adminPrivilege = InstanceAdminPrivileges.ViewInstanceSettings;

            //Act
            var emailSettingsDto = await _emailSettingsService.GetEmailSettingsAsync(UserId);

            //Assert
            Assert.AreEqual(_emailSettings.HostName, emailSettingsDto.Outgoing.ServerAddress);
            Assert.AreEqual(_emailSettings.Port, emailSettingsDto.Outgoing.Port);
            Assert.AreEqual(_emailSettings.EnableSSL, emailSettingsDto.Outgoing.EnableSsl);
            Assert.AreEqual(_emailSettings.Authenticated, emailSettingsDto.Outgoing.AuthenticatedSmtp);
            Assert.AreEqual(null, emailSettingsDto.Outgoing.AccountPassword);
            Assert.AreEqual(_emailSettings.UserName, emailSettingsDto.Outgoing.AccountUsername);
            Assert.AreEqual(_emailSettings.SenderEmailAddress, emailSettingsDto.Outgoing.AccountEmailAddress);
            Assert.AreEqual(false, emailSettingsDto.Outgoing.IsPasswordDirty);

            Assert.AreEqual(_emailSettings.IncomingHostName, emailSettingsDto.Incoming.ServerAddress);
            Assert.AreEqual(_emailSettings.IncomingPort, emailSettingsDto.Incoming.Port);
            Assert.AreEqual(_emailSettings.IncomingServerType, (int)emailSettingsDto.Incoming.ServerType);
            Assert.AreEqual(_emailSettings.IncomingEnableSSL, emailSettingsDto.Incoming.EnableSsl);
            Assert.AreEqual(_emailSettings.IncomingUserName, emailSettingsDto.Incoming.AccountUsername);
            Assert.AreEqual(null, emailSettingsDto.Incoming.AccountPassword);
            Assert.AreEqual(false, emailSettingsDto.Incoming.IsPasswordDirty);

            Assert.AreEqual(_emailSettings.EnableNotifications, emailSettingsDto.EnableReviewNotifications);
            Assert.AreEqual(_emailSettings.EnableEmailDiscussion, emailSettingsDto.EnableEmailNotifications);
            Assert.AreEqual(_emailSettings.EnableEmailReplies, emailSettingsDto.EnableDiscussions);
        }

        #endregion

        #region UpdateEmailSettingsAsync

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_AuthorizationException_When_User_Doesnt_Have_ManageInstanceSettings()
        {
            //Arrange
            _adminPrivilege = InstanceAdminPrivileges.ViewInstanceSettings;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.Forbidden);

                return;
            }

            Assert.Fail("AuthorizationException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_BadRequestException_When_EnableDiscussions_Is_True_But_EnableReplies_Is_False()
        {
            //Arrange
            _emailSettingsDto.EnableDiscussions = true;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.CannotEnableDiscussions);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_BadRequestException_When_EnableDiscussions_Is_True_And_Incoming_ServerAddress_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.EnableDiscussions = true;
            _emailSettingsDto.Incoming.ServerAddress = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.IncomingEmptyMailServer);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_BadRequestException_When_EnableDiscussions_Is_True_And_Incoming_Ports_Is_Less_Than_1()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.EnableDiscussions = true;
            _emailSettingsDto.Incoming.Port = 0;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.IncomingPortOutOfRange);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_BadRequestException_When_EnableDiscussions_Is_True_And_Incoming_Ports_Is_Greater_Than_65535()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.EnableDiscussions = true;
            _emailSettingsDto.Incoming.Port = 65536;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.IncomingPortOutOfRange);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_BadRequestException_When_EnableDiscussions_Is_True_And_Incoming_AccountUsername_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.EnableDiscussions = true;
            _emailSettingsDto.Incoming.AccountUsername = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptyEmailUsername);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_BadRequestException_When_EnableDiscussions_Is_True_And_Incoming_AccountPassword_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.EnableDiscussions = true;
            _emailSettingsDto.Incoming.AccountPassword = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptyEmailPassword);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Not_Throw_When_EnableDiscussions_Is_False_And_Incoming_Data_Is_Null()
        {
            //Arrange
            _emailSettingsDto.EnableDiscussions = false;
            _emailSettingsDto.Incoming.AccountPassword = null;
            _emailSettingsDto.Incoming.AccountUsername = null;
            _emailSettingsDto.Incoming.ServerAddress = null;
            _emailSettingsDto.Incoming.Port = 0;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (Exception ex)
            {
                Assert.Fail($"An exception of type {ex.GetType()} was thrown, but no exception was expected.");
            }
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableReviewNotifications_Is_True_And_Outgoing_ServerAddress_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableReviewNotifications = true;
            _emailSettingsDto.Outgoing.ServerAddress = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.OutgoingEmptyMailServer);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableReviewNotifications_Is_True_And_Outgoing_Port_Is_Less_Than_1()
        {
            //Arrange
            _emailSettingsDto.EnableReviewNotifications = true;
            _emailSettingsDto.Outgoing.Port = 0;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.OutgoingPortOutOfRange);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableReviewNotifications_Is_True_And_Outgoing_Port_Is_Greater_Than_65535()
        {
            //Arrange
            _emailSettingsDto.EnableReviewNotifications = true;
            _emailSettingsDto.Outgoing.Port = 65536;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.OutgoingPortOutOfRange);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableReviewNotifications_Is_True_And_Outgoing_Authentication_Is_True_And_Outgoing_Username_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableReviewNotifications = true;
            _emailSettingsDto.Outgoing.AccountUsername = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptySmtpAdministratorUsername);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableReviewNotifications_Is_True_And_Outgoing_Authentication_Is_True_And_Outgoing_Password_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableReviewNotifications = true;
            _emailSettingsDto.Outgoing.AccountPassword = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptySmtpAdministratorPassword);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableReviewNotifications_Is_True_And_Outgoing_EmailAddress_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableReviewNotifications = true;
            _emailSettingsDto.Outgoing.AccountEmailAddress = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptyEmailAddress);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableReviewNotifications_Is_True_And_Outgoing_EmailAddress_Is_Not_A_Valid_Email()
        {
            //Arrange
            _emailSettingsDto.EnableReviewNotifications = true;
            _emailSettingsDto.Outgoing.AccountEmailAddress = "notanemail";

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.InvalidEmailAddress);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableEmailNotifications_Is_True_And_Outgoing_ServerAddress_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.Outgoing.ServerAddress = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.OutgoingEmptyMailServer);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableEmailNotifications_Is_True_And_Outgoing_Port_Is_Less_Than_1()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.Outgoing.Port = 0;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.OutgoingPortOutOfRange);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableEmailNotifications_Is_True_And_Outgoing_Port_Is_Greater_Than_65535()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.Outgoing.Port = 65536;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.OutgoingPortOutOfRange);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableEmailNotifications_Is_True_And_Authentication_Is_True_And_Username_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.Outgoing.AuthenticatedSmtp = true;
            _emailSettingsDto.Outgoing.AccountUsername = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptySmtpAdministratorUsername);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableEmailNotifications_Is_True_And_Authentication_Is_True_And_Password_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.Outgoing.AuthenticatedSmtp = true;
            _emailSettingsDto.Outgoing.AccountPassword = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptySmtpAdministratorPassword);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableEmailNotifications_Is_True_And_EmailAdress_Is_Empty()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.Outgoing.AccountEmailAddress = null;

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptyEmailAddress);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Throw_When_EnableEmailNotifications_Is_True_And_EmailAdress_Is_Not_An_Email()
        {
            //Arrange
            _emailSettingsDto.EnableEmailNotifications = true;
            _emailSettingsDto.Outgoing.AccountEmailAddress = "notanemail";

            //Act
            try
            {
                await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
            }
            //Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.InvalidEmailAddress);

                return;
            }

            Assert.Fail("BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Update_Via_The_Repository()
        {
            //Act
            await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);
           
            //Assert
            _instanceSettingsRepositoryMock.Verify(repo => repo.UpdateEmailSettingsAsync(_emailSettings));
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Update_Settings_From_The_Dto()
        {
            //Act
            await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);

            //Assert
            Assert.AreEqual(_emailSettings.EnableEmailDiscussion, _emailSettingsDto.EnableDiscussions);
            Assert.AreEqual(_emailSettings.EnableEmailReplies, _emailSettingsDto.EnableEmailNotifications);
            Assert.AreEqual(_emailSettings.EnableNotifications, _emailSettingsDto.EnableReviewNotifications);

            Assert.AreEqual(_emailSettings.Authenticated, _emailSettingsDto.Outgoing.AuthenticatedSmtp);
            Assert.AreEqual(_emailSettings.EnableSSL, _emailSettingsDto.Outgoing.EnableSsl);
            Assert.AreEqual(_emailSettings.HostName, _emailSettingsDto.Outgoing.ServerAddress);
            Assert.AreEqual(_emailSettings.Port, _emailSettingsDto.Outgoing.Port);
            Assert.AreEqual(_emailSettings.SenderEmailAddress, _emailSettingsDto.Outgoing.AccountEmailAddress);
            Assert.AreEqual(_emailSettings.UserName, _emailSettingsDto.Outgoing.AccountUsername);

            Assert.AreEqual(_emailSettings.IncomingEnableSSL, _emailSettingsDto.Incoming.EnableSsl);
            Assert.AreEqual(_emailSettings.IncomingHostName, _emailSettingsDto.Incoming.ServerAddress);
            Assert.AreEqual(_emailSettings.IncomingPort, _emailSettingsDto.Incoming.Port);
            Assert.AreEqual(_emailSettings.IncomingServerType, (int)_emailSettingsDto.Incoming.ServerType);
            Assert.AreEqual(_emailSettings.IncomingUserName, _emailSettingsDto.Incoming.AccountUsername);
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Encrypt_Outgoing_Password()
        {
            //Arrange
            var encryptedOutgoingPassword = "ksZKZH6kl2wKJ4Nb/OlFmw==";

            //Act
            await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);

            //Assert
            Assert.AreEqual(encryptedOutgoingPassword, _emailSettings.Password);
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Encrypt_Incoming_Password()
        {
            //Arrange
            var encryptedIncomingPassword = "uNPPwhOB27mMxpqxMKjMqw==";

            //Act
            await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);

            //Assert
            Assert.AreEqual(encryptedIncomingPassword, _emailSettings.IncomingPassword);
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Not_Update_Password_If_Outgoing_Password_Is_Not_Dirty()
        {
            //Arrange
            string oldPassword = _emailSettings.Password;
            _emailSettingsDto.Outgoing.IsPasswordDirty = false;

            //Act
            await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);

            //Assert
            Assert.AreEqual(_emailSettings.Password, oldPassword);
        }

        [TestMethod]
        public async Task UpdateEmailSettingsAsync_Should_Not_Update_IncomingPassword_If_Incoming_Password_Is_Not_Dirty()
        {
            //Arrange
            string oldPassword = _emailSettings.IncomingPassword;
            _emailSettingsDto.Incoming.IsPasswordDirty = false;

            //Act
            await _emailSettingsService.UpdateEmailSettingsAsync(UserId, _emailSettingsDto);

            //Assert
            Assert.AreEqual(_emailSettings.IncomingPassword, oldPassword);
        }

        #endregion
    }
}
