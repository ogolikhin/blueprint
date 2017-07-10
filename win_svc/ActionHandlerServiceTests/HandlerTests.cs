using System;
using ActionHandlerService;
using ActionHandlerService.MessageHandlers;
using ActionHandlerService.MessageHandlers.GenerateDescendants;
using ActionHandlerService.MessageHandlers.GenerateTests;
using ActionHandlerService.MessageHandlers.GenerateUserStories;
using ActionHandlerService.MessageHandlers.Notifications;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NServiceBus.Testing;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Message Handlers in the Action Handler Service
    /// </summary>
    [TestClass]
    public class HandlerTests
    {
        [TestMethod]
        public void NotificationMessageHandler_CompletesSuccessfully()
        {
            var actionHandlerHelperMock = new Mock<IActionHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Returns(true);
            var handler = new NotificationMessageHandler(actionHandlerHelperMock.Object);
            var message = new NotificationMessage(0, 0);
            Test.Handler(handler).OnMessage(message);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void NotificationMessageHandler_RethrowsException()
        {
            const string exceptionMessage = "test exception";
            var actionHandlerHelperMock = new Mock<IActionHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Throws(new Exception(exceptionMessage));
            var handler = new NotificationMessageHandler(actionHandlerHelperMock.Object);
            var message = new NotificationMessage(0, 0);
            try
            {
                Test.Handler(handler).OnMessage(message);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(exceptionMessage, ex.Message);
                throw;
            }
        }

        [TestMethod]
        public void GenerateDescendantsMessageHandler_CompletesSuccessfully()
        {
            var actionHandlerHelperMock = new Mock<IActionHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Returns(true);
            var handler = new GenerateDescendantsMessageHandler(actionHandlerHelperMock.Object);
            var message = new GenerateDescendantsMessage(0, 0);
            Test.Handler(handler).OnMessage(message);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GenerateDescendantsMessageHandler_RethrowsException()
        {
            const string exceptionMessage = "test exception";
            var actionHandlerHelperMock = new Mock<IActionHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Throws(new Exception(exceptionMessage));
            var handler = new GenerateDescendantsMessageHandler(actionHandlerHelperMock.Object);
            var message = new GenerateDescendantsMessage(0, 0);
            try
            {
                Test.Handler(handler).OnMessage(message);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(exceptionMessage, ex.Message);
                throw;
            }
        }

        [TestMethod]
        public void GenerateTestsMessageHandler_CompletesSuccessfully()
        {
            var actionHandlerHelperMock = new Mock<IActionHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Returns(true);
            var handler = new GenerateTestsMessageHandler(actionHandlerHelperMock.Object);
            var message = new GenerateTestsMessage(0, 0);
            Test.Handler(handler).OnMessage(message);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GenerateTestsMessageHandler_RethrowsException()
        {
            const string exceptionMessage = "test exception";
            var actionHandlerHelperMock = new Mock<IActionHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Throws(new Exception(exceptionMessage));
            var handler = new GenerateTestsMessageHandler(actionHandlerHelperMock.Object);
            var message = new GenerateTestsMessage(0, 0);
            try
            {
                Test.Handler(handler).OnMessage(message);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(exceptionMessage, ex.Message);
                throw;
            }
        }

        [TestMethod]
        public void GenerateUserStoriesMessageHandler_CompletesSuccessfully()
        {
            var actionHandlerHelperMock = new Mock<IActionHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Returns(true);
            var handler = new GenerateUserStoriesMessageHandler(actionHandlerHelperMock.Object);
            var message = new GenerateUserStoriesMessage(0, 0);
            Test.Handler(handler).OnMessage(message);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GenerateUserStoriesMessageHandler_RethrowsException()
        {
            const string exceptionMessage = "test exception";
            var actionHandlerHelperMock = new Mock<IActionHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Throws(new Exception(exceptionMessage));
            var handler = new GenerateUserStoriesMessageHandler(actionHandlerHelperMock.Object);
            var message = new GenerateUserStoriesMessage(0, 0);
            try
            {
                Test.Handler(handler).OnMessage(message);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(exceptionMessage, ex.Message);
                throw;
            }
        }
    }
}
