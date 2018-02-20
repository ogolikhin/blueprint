using System;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.Notifications;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Email;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.Notifications
{
    /// <summary>
    /// Tests for the Notifications Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class NotificationsActionHelperTests
    {
        private TenantInformation _tenantInformation;
        private NotificationMessage _notificationMessage;
        private Mock<INotificationRepository> _notificationRepositoryMock;
        private NotificationsActionHelper _notificationsActionHelper;
        private EmailSettings _emailSettings;

        [TestInitialize]
        public void TestInitialize()
        {
            _tenantInformation = new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = ""
            };
            _notificationMessage = new NotificationMessage
            {
                ArtifactId = 1,
                ArtifactName = "MyArtifact",
                ArtifactTypeId = 2,
                ArtifactTypePredefined = (int)ItemTypePredefined.Process,
                ArtifactUrl = "",
                From = "nw@blueprintsys.com",
                To = new[]
                {
                    "nw@gmail.com"
                },
                Header = "header",
                Message = "",
                ProjectId = 3,
                ProjectName = "MyProject",
                RevisionId = 5,
                Subject = "Test Email",
                UserId = 1,
                BlueprintUrl = ""
            };
            _emailSettings = new EmailSettings
            {
                HostName = "localhost",
                Id = "1",
                EnableSSL = true,
                Authenticated = true,
                EnableAllUsers = true,
                UserName = "admin",
                Password = "",
                Port = 227,
                SenderEmailAddress = "nw@blueprintsys.com"
            };
            _notificationRepositoryMock = new Mock<INotificationRepository>(MockBehavior.Strict);
            _notificationsActionHelper = new NotificationsActionHelper();
        }

        [TestMethod]
        public async Task NotificationsActionHelper_HandleActionReturnsTrue_WhenInputAndEmailSettingsAreValid()
        {
            // Arrange
            _notificationRepositoryMock.Setup(m => m.GetEmailSettings()).ReturnsAsync(_emailSettings);
            _notificationRepositoryMock.Setup(m => m.SendEmail(It.IsAny<SMTPClientConfiguration>(), It.IsAny<Message>()));

            // Act
            var result = await _notificationsActionHelper.HandleAction(_tenantInformation, _notificationMessage, _notificationRepositoryMock.Object);

            // Assert
            Assert.IsTrue(result);
            _notificationRepositoryMock.Verify(m => m.GetEmailSettings(), Times.Once);
            _notificationRepositoryMock.Verify(m => m.SendEmail(It.IsAny<SMTPClientConfiguration>(), It.IsAny<Message>()), Times.Once);
        }

        [TestMethod]
        public async Task NotificationsActionHelper_HandleActionReturnsTrue_WhenMessageStringsAreNull()
        {
            // Arrange
            _notificationMessage.Header = null;
            _notificationMessage.ArtifactName = null;
            _notificationMessage.ArtifactUrl = null;
            _notificationMessage.Message = null;
            _notificationMessage.ProjectName = null;
            _notificationMessage.Subject = null;
            _notificationMessage.From = null;
            _notificationMessage.BlueprintUrl = null;
            _notificationRepositoryMock.Setup(m => m.GetEmailSettings()).ReturnsAsync(_emailSettings);
            _notificationRepositoryMock.Setup(m => m.SendEmail(It.IsAny<SMTPClientConfiguration>(), It.IsAny<Message>()));

            // Act
            var result = await _notificationsActionHelper.HandleAction(_tenantInformation, _notificationMessage, _notificationRepositoryMock.Object);

            // Assert
            // we should not encounter an exception
            Assert.IsTrue(result);
            _notificationRepositoryMock.Verify(m => m.GetEmailSettings(), Times.Once);
            _notificationRepositoryMock.Verify(m => m.SendEmail(It.IsAny<SMTPClientConfiguration>(), It.IsAny<Message>()), Times.Once);
        }

        [TestMethod]
        public async Task NotificationsActionHelper_HandleActionReturnsFalse_WhenEmailSettingsHostNameIsNull()
        {
            // Arrange
            _emailSettings.HostName = null;
            _notificationRepositoryMock.Setup(m => m.GetEmailSettings()).ReturnsAsync(_emailSettings);

            // Act
            var result = await _notificationsActionHelper.HandleAction(_tenantInformation, _notificationMessage, _notificationRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result);
            _notificationRepositoryMock.Verify(m => m.GetEmailSettings(), Times.Once);
            _notificationRepositoryMock.Verify(m => m.SendEmail(It.IsAny<SMTPClientConfiguration>(), It.IsAny<Message>()), Times.Never);
        }

        [TestMethod]
        public async Task NotificationsActionHelper_HandleActionReturnsFalse_WhenEmailSettingsHostNameIsEmpty()
        {
            // Arrange
            _emailSettings.HostName = string.Empty;
            _notificationRepositoryMock.Setup(m => m.GetEmailSettings()).ReturnsAsync(_emailSettings);

            // Act
            var result = await _notificationsActionHelper.HandleAction(_tenantInformation, _notificationMessage, _notificationRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result);
            _notificationRepositoryMock.Verify(m => m.GetEmailSettings(), Times.Once);
            _notificationRepositoryMock.Verify(m => m.SendEmail(It.IsAny<SMTPClientConfiguration>(), It.IsAny<Message>()), Times.Never);
        }

        [TestMethod]
        public async Task NotificationsActionHelper_HandleActionReturnsFalse_WhenEmailSettingsHostNameIsWhitespace()
        {
            // Arrange
            _emailSettings.HostName = " ";
            _notificationRepositoryMock.Setup(m => m.GetEmailSettings()).ReturnsAsync(_emailSettings);

            // Act
            var result = await _notificationsActionHelper.HandleAction(_tenantInformation, _notificationMessage, _notificationRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result);
            _notificationRepositoryMock.Verify(m => m.GetEmailSettings(), Times.Once);
            _notificationRepositoryMock.Verify(m => m.SendEmail(It.IsAny<SMTPClientConfiguration>(), It.IsAny<Message>()), Times.Never);
        }

        [TestMethod]
        public async Task NotificationsActionHelper_HandleActionReturnsFalse_WhenEmailSettingsAreNull()
        {
            // Arrange
            _notificationRepositoryMock.Setup(m => m.GetEmailSettings()).ReturnsAsync((EmailSettings)null);

            // Act
            var result = await _notificationsActionHelper.HandleAction(_tenantInformation, _notificationMessage, _notificationRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result);
            _notificationRepositoryMock.Verify(m => m.GetEmailSettings(), Times.Once);
            _notificationRepositoryMock.Verify(m => m.SendEmail(It.IsAny<SMTPClientConfiguration>(), It.IsAny<Message>()), Times.Never);
        }
    }
}
