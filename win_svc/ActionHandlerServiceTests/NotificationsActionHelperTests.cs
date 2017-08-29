using System;
using System.Threading.Tasks;
using ActionHandlerService.MessageHandlers.Notifications;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Notifications Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class NotificationsActionHelperTests
    {
        [TestMethod]
        public async Task NotificationsActionHelper_HandleActionReturnsTrue()
        {
            //Arrange
            var tenantInformation = new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = ""
            };
            var message = new NotificationMessage
            {
                ArtifactId = 1,
                ArtifactName = "MyArtifact",
                ArtifactTypeId = 2,
                ArtifactTypePredefined = (int) ItemTypePredefined.Process,
                ArtifactUrl = "",
                From = "nw@blueprintsys.com",
                To = new[] { "nw@gmail.com"},
                Header = "header",
                MessageTemplate = "",
                ProjectId = 3,
                ProjectName = "MyProject",
                RevisionId = 5,
                Subject = "Test Email",
                UserId = 1
            };
            var notificationRepositoryMock = new Mock<INotificationRepository>();
            notificationRepositoryMock.Setup(t => t.GetEmailSettings()).ReturnsAsync(new EmailSettings
            {
                Id = "1",
                EnableSSL = true,
                Authenticated = true,
                EnableAllUsers = true,
                HostName = "localhost",
                UserName = "admin",
                Password = "",
                Port = 227,
                SenderEmailAddress = "nw@blueprintsys.com"
            });

            var actionHelper = new NotificationsActionHelper();

            //Act
            var result = await actionHelper.HandleAction(tenantInformation, message, notificationRepositoryMock.Object);

            //Assert
            Assert.IsTrue(result);
        }
    }
}
