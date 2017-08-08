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
                Id = Guid.NewGuid().ToString(),
                Settings = "",
                ConnectionString = ""
            };
            var message = new NotificationMessage()
            {
                ArtifactId = 1,
                ArtifactName = "MyArtifact",
                ArtifactTypeId = 2,
                ArtifactTypePredefined = (int) ItemTypePredefined.Process,
                ArtifactUrl = "",
                From = "nw@blueprintsys.com",
                To = new[] { "nw@gmail.com"},
                MessageTemplate = "",
                ModifiedPropertiesInformation = new ModifiedPropertyInformation[0],
                ProjectId = 3,
                ProjectName = "MyProject",
                RevisionId = 5,
                Subject = "Test Email",
                UserId = 1
            };
            var actionHandlerServiceRepoMock = new Mock<INotificationActionHandlerServiceRepository>();
            actionHandlerServiceRepoMock.Setup(t => t.GetEmailSettings()).ReturnsAsync(new EmailSettings
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
            var result = await actionHelper.HandleAction(tenantInformation, message, actionHandlerServiceRepoMock.Object);

            //Assert
            Assert.IsTrue(result);
        }
    }
}
