using System;
using ActionHandlerService;
using ActionHandlerService.MessageHandlers;
using ActionHandlerService.MessageHandlers.ArtifactPublished;
using ActionHandlerService.MessageHandlers.GenerateDescendants;
using ActionHandlerService.MessageHandlers.GenerateTests;
using ActionHandlerService.MessageHandlers.GenerateUserStories;
using ActionHandlerService.MessageHandlers.Notifications;
using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language.Flow;
using NServiceBus.Testing;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Message Handlers in the Action Handler Service
    /// </summary>
    [TestClass]
    public class HandlerTests
    {
        private Mock<IActionHelper> _actionHelperMock;
        private ISetup<IActionHelper, bool> _handleActionSetup;
        private const string TenantId = "0";

        [TestInitialize]
        public void TestInitialize()
        {
            _actionHelperMock = new Mock<IActionHelper>();
            _handleActionSetup = _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>()));
        }

        private static void TestHandler<T>(BaseMessageHandler<T> handler, T message) where T : ActionMessage
        {
            Test.Handler(handler).SetIncomingHeader(ActionMessageHeaders.TenantId, TenantId).OnMessage(message);
        }

        public class TestException : Exception
        {
        }

        [TestMethod]
        public void NotificationMessageHandler_CompletesSuccessfully()
        {
            _handleActionSetup.Returns(true);
            var handler = new NotificationMessageHandler(_actionHelperMock.Object);
            var message = new NotificationMessage();
            TestHandler(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void NotificationMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new NotificationMessageHandler(_actionHelperMock.Object);
            var message = new NotificationMessage();
            TestHandler(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(MessageHeaderValueNotFoundException))]
        public void NotificationMessageHandler_ThrowsMessageHeaderValueNotFoundException_WhenHeaderValueIsNotFound()
        {
            _handleActionSetup.Returns(true);
            var handler = new NotificationMessageHandler(_actionHelperMock.Object);
            var message = new NotificationMessage();
            Test.Handler(handler).OnMessage(message);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTenantIdException))]
        public void NotificationMessageHandler_ThrowsInvalidTenantIdExceptionException_WhenHeaderTenantIdIsNotAnInteger()
        {
            _handleActionSetup.Returns(true);
            var handler = new NotificationMessageHandler(_actionHelperMock.Object);
            var message = new NotificationMessage();
            Test.Handler(handler).SetIncomingHeader(ActionMessageHeaders.TenantId, "Not an integer").OnMessage(message);
        }

        [TestMethod]
        public void GenerateDescendantsMessageHandler_CompletesSuccessfully()
        {
            _handleActionSetup.Returns(true);
            var handler = new GenerateDescendantsMessageHandler(_actionHelperMock.Object);
            var message = new GenerateDescendantsMessage();
            TestHandler(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void GenerateDescendantsMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new GenerateDescendantsMessageHandler(_actionHelperMock.Object);
            var message = new GenerateDescendantsMessage();
            TestHandler(handler, message);
        }

        [TestMethod]
        public void GenerateTestsMessageHandler_CompletesSuccessfully()
        {
            _handleActionSetup.Returns(true);
            var handler = new GenerateTestsMessageHandler(_actionHelperMock.Object);
            var message = new GenerateTestsMessage();
            TestHandler(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void GenerateTestsMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new GenerateTestsMessageHandler(_actionHelperMock.Object);
            var message = new GenerateTestsMessage();
            TestHandler(handler, message);
        }

        [TestMethod]
        public void GenerateUserStoriesMessageHandler_CompletesSuccessfully()
        {
            _handleActionSetup.Returns(true);
            var handler = new GenerateUserStoriesMessageHandler(_actionHelperMock.Object);
            var message = new GenerateUserStoriesMessage();
            TestHandler(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void GenerateUserStoriesMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new GenerateUserStoriesMessageHandler(_actionHelperMock.Object);
            var message = new GenerateUserStoriesMessage();
            TestHandler(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void ArtifactsPublishedMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new ArtifactsPublishedMessageHandler(_actionHelperMock.Object);
            var message = new ArtifactsPublishedMessage();
            TestHandler(handler, message);
        }

        [TestMethod]
        public void ArtifactsPublishedMessageHandler_CompletesSuccessfully()
        {
            _handleActionSetup.Returns(true);
            var handler = new ArtifactsPublishedMessageHandler(_actionHelperMock.Object);
            var message = new ArtifactsPublishedMessage();
            TestHandler(handler, message);
        }
    }
}
