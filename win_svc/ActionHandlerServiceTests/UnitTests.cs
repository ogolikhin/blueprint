using System;
using ActionHandlerService;
using ActionHandlerService.MessageHandlers;
using BluePrintSys.ActionMessaging.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NServiceBus.Testing;

namespace ActionHandlerServiceTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void NotificationMessageHandler_CompletesSuccessfully()
        {
            const ActionType messageActionType = ActionType.Notification;

            var actionHandlerHelperMock = new Mock<IActionHandlerHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Returns(true);
            ActionHandlerService.ActionHandlerService.Instance.ActionHandlerHelper = actionHandlerHelperMock.Object;

            Test.Handler<NotificationMessageHandler>().OnMessage<NotificationMessage>(message => message.ActionType = messageActionType);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void NotificationMessageHandler_RethrowsException()
        {
            const ActionType messageActionType = ActionType.Notification;

            var actionHandlerHelperMock = new Mock<IActionHandlerHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Throws(new Exception());
            ActionHandlerService.ActionHandlerService.Instance.ActionHandlerHelper = actionHandlerHelperMock.Object;

            Test.Handler<NotificationMessageHandler>().OnMessage<NotificationMessage>(message => message.ActionType = messageActionType);
        }

        [TestMethod]
        public void GenerateDescendantsMessageHandler_CompletesSuccessfully()
        {
            const ActionType messageActionType = ActionType.GenerateDescendants;

            var actionHandlerHelperMock = new Mock<IActionHandlerHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Returns(true);
            ActionHandlerService.ActionHandlerService.Instance.ActionHandlerHelper = actionHandlerHelperMock.Object;

            Test.Handler<GenerateDescendantsMessageHandler>().OnMessage<GenerateDescendantsMessage>(message => message.ActionType = messageActionType);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GenerateDescendantsMessageHandler_RethrowsException()
        {
            const ActionType messageActionType = ActionType.GenerateDescendants;

            var actionHandlerHelperMock = new Mock<IActionHandlerHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Throws(new Exception());
            ActionHandlerService.ActionHandlerService.Instance.ActionHandlerHelper = actionHandlerHelperMock.Object;

            Test.Handler<GenerateDescendantsMessageHandler>().OnMessage<GenerateDescendantsMessage>(message => message.ActionType = messageActionType);
        }

        [TestMethod]
        public void GenerateTestsMessageHandler_CompletesSuccessfully()
        {
            const ActionType messageActionType = ActionType.GenerateTests;

            var actionHandlerHelperMock = new Mock<IActionHandlerHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Returns(true);
            ActionHandlerService.ActionHandlerService.Instance.ActionHandlerHelper = actionHandlerHelperMock.Object;

            Test.Handler<GenerateTestsMessageHandler>().OnMessage<GenerateTestsMessage>(message => message.ActionType = messageActionType);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GenerateTestsMessageHandler_RethrowsException()
        {
            const ActionType messageActionType = ActionType.GenerateTests;

            var actionHandlerHelperMock = new Mock<IActionHandlerHelper>();
            actionHandlerHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInfo>())).Throws(new Exception());
            ActionHandlerService.ActionHandlerService.Instance.ActionHandlerHelper = actionHandlerHelperMock.Object;

            Test.Handler<GenerateTestsMessageHandler>().OnMessage<GenerateTestsMessage>(message => message.ActionType = messageActionType);
        }
    }
}
