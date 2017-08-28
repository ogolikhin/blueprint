using System;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.MessageHandlers;
using ActionHandlerService.MessageHandlers.ArtifactPublished;
using ActionHandlerService.MessageHandlers.GenerateDescendants;
using ActionHandlerService.MessageHandlers.GenerateTests;
using ActionHandlerService.MessageHandlers.GenerateUserStories;
using ActionHandlerService.MessageHandlers.Notifications;
using ActionHandlerService.MessageHandlers.PropertyChange;
using ActionHandlerService.MessageHandlers.StateTransition;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Models.Exceptions;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language.Flow;
using NServiceBus;
using NServiceBus.Testing;
using ServiceLibrary.Models.Enums;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Message Handlers in the Action Handler Service
    /// </summary>
    [TestClass]
    public class HandlerTests
    {
        private Mock<IActionHelper> _actionHelperMock;
        private Mock<IActionHandlerServiceRepository> _actionHandlerServiceRepositoryMock;
        private ISetup<IActionHelper, Task<bool>> _handleActionSetup;
        private TenantInfoRetriever _tenantInfoRetriever;
        private ConfigHelper _configHelper;
        private const string TenantId = "tenant0";

        [TestInitialize]
        public void TestInitialize()
        {
            _actionHelperMock = new Mock<IActionHelper>();
            _handleActionSetup = _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<ActionHandlerServiceRepository>()));
            _actionHandlerServiceRepositoryMock = new Mock<IActionHandlerServiceRepository>();
            _actionHandlerServiceRepositoryMock.Setup(a => a.GetTenantId()).ReturnsAsync(TenantId);
            _configHelper = new ConfigHelper();
            _tenantInfoRetriever = new TenantInfoRetriever(_actionHandlerServiceRepositoryMock.Object, _configHelper);
        }

        private static void TestHandlerAndMessageWithHeader<T>(BaseMessageHandler<T> handler, T message, string tenantId = TenantId) where T : ActionMessage
        {
            Test.Handler(handler).SetIncomingHeader(ActionMessageHeaders.TenantId, tenantId).SetIncomingHeader(Headers.MessageId, "0").SetIncomingHeader(Headers.TimeSent, "0").OnMessage(message);
        }

        [Serializable]
        public class TestException : Exception
        {
            public TestException(string message = "An exception used for testing the Action Handler Service") : base(message)
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedActionTypeException))]
        public void BaseMessageHandler_ThrowsUnsupportedActionTypeException_WhenActionTypeIsNotSupported()
        {
            var configHelperMock = new Mock<IConfigHelper>();
            configHelperMock.Setup(m => m.SupportedActionTypes).Returns(MessageActionType.None);
            var handler = new NotificationMessageHandler(null, null, configHelperMock.Object);
            var message = new NotificationMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }
        
        [TestMethod]
        [ExpectedException(typeof(MessageHeaderValueNotFoundException))]
        public void BaseMessageHandler_ThrowsMessageHeaderValueNotFoundException_WhenHeaderValueIsNotFound()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new NotificationMessage();
            Test.Handler(handler).OnMessage(message);
        }

        [TestMethod]
        public void ArtifactsPublishedMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new ArtifactsPublishedMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void ArtifactsPublishedMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new ArtifactsPublishedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new ArtifactsPublishedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void ArtifactsPublishedMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new ArtifactsPublishedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new ArtifactsPublishedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void NotificationMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new NotificationMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void NotificationMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new NotificationMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void NotificationMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new NotificationMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void GenerateDescendantsMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new GenerateDescendantsMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void GenerateDescendantsMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new GenerateDescendantsMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new GenerateDescendantsMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void GenerateDescendantsMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new GenerateDescendantsMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new GenerateDescendantsMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void GenerateTestsMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new GenerateTestsMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void GenerateTestsMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new GenerateTestsMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new GenerateTestsMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void GenerateTestsMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new GenerateTestsMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new GenerateTestsMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void GenerateUserStoriesMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new GenerateUserStoriesMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new GenerateUserStoriesMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void GenerateUserStoriesMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new GenerateUserStoriesMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new GenerateUserStoriesMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void StateTransitionMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new StateTransitionMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void StateTransitionMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new StateTransitionMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new StateChangeMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void StateTransitionMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new StateTransitionMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new StateChangeMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void PropertyChangeMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new PropertyChangeMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void PropertyChangeMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new PropertyChangeMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new GenerateUserStoriesMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void PropertyChangeMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new PropertyChangeMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new GenerateUserStoriesMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }
    }
}
