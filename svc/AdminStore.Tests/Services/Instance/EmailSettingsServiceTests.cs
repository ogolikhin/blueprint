using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.Emails;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Services;

namespace AdminStore.Services.Instance
{
    [TestClass]
    public class EmailSettingsServiceTests
    {
        private EmailSettingsService _emailSettingsService;
        private Mock<IEmailHelper> _emailHelperMock;

        private EmailOutgoingSettings _outgoingSettings;
        private InstanceAdminPrivileges _adminPrivilege;
        private User _user;

        private const int UserId = 1;
        private const string WebsiteAddress = "https://blueprintsys.net";

        [TestInitialize]
        public void Initialize()
        {
            Mock<IPrivilegesRepository> privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IWebsiteAddressService> websiteAddressServiceMock = new Mock<IWebsiteAddressService>();
            _emailHelperMock = new Mock<IEmailHelper>();

            _emailSettingsService = new EmailSettingsService(new PrivilegesManager(privilegesRepositoryMock.Object),
                                                             userRepositoryMock.Object,
                                                             _emailHelperMock.Object,
                                                             websiteAddressServiceMock.Object);

            privilegesRepositoryMock.Setup(repo => repo.GetInstanceAdminPrivilegesAsync(UserId)).ReturnsAsync(() => _adminPrivilege);

            userRepositoryMock.Setup(repo => repo.GetUserAsync(UserId)).ReturnsAsync(() => _user);

            websiteAddressServiceMock.Setup(service => service.GetWebsiteAddress()).Returns(WebsiteAddress);

            //Setup Default Values
            _outgoingSettings = new EmailOutgoingSettings()
            {
                AuthenticatedSmtp = true,
                AuthenticatedSmtpPassword = "password",
                AuthenticatedSmtpUsername = "admin",
                EnableSsl = true,
                Port = 2,
                ServerAddress = "smtp.blueprintsys.com"
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
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.EmptyMailServer);
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
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.PortOutOfRange);
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
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.PortOutOfRange);
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
                   config.Password == (_outgoingSettings.AuthenticatedSmtp ? _outgoingSettings.AuthenticatedSmtpPassword : String.Empty) &&
                   config.Port == _outgoingSettings.Port &&
                   config.SenderEmailAddress == _user.Email &&
                   config.UserName == (_outgoingSettings.AuthenticatedSmtp ? _outgoingSettings.AuthenticatedSmtpUsername : String.Empty);
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
    }
}
