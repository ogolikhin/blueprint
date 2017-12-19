using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsPublished;
using BlueprintSys.RC.Services.MessageHandlers.GenerateDescendants;
using BlueprintSys.RC.Services.MessageHandlers.GenerateTests;
using BlueprintSys.RC.Services.MessageHandlers.GenerateUserStories;
using BlueprintSys.RC.Services.MessageHandlers.Notifications;
using BlueprintSys.RC.Services.MessageHandlers.ProjectsChanged;
using BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged;
using BlueprintSys.RC.Services.MessageHandlers.UsersGroupsChanged;
using BlueprintSys.RC.Services.MessageHandlers.WorkflowsChanged;
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

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the Message Handlers in the Action Handler Service
    /// </summary>
    [TestClass]
    public class HandlerTests
    {
        private Mock<IActionHelper> _actionHelperMock;
        private Mock<IBaseRepository> _actionHandlerServiceRepositoryMock;
        private ISetup<IActionHelper, Task<bool>> _handleActionSetup;
        private TenantInfoRetriever _tenantInfoRetriever;
        private ConfigHelper _configHelper;
        private const string TenantId = "tenant0";

        [TestInitialize]
        public void TestInitialize()
        {
            _actionHelperMock = new Mock<IActionHelper>();
            _handleActionSetup = _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()));
            _actionHandlerServiceRepositoryMock = new Mock<IBaseRepository>();
            var tenants = new List<TenantInformation>
            {
                new TenantInformation
                {
                    TenantId = TenantId
                }
            };
            _actionHandlerServiceRepositoryMock.Setup(a => a.GetTenantsFromTenantsDb()).ReturnsAsync(tenants);
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
        [ExpectedException(typeof(EntityNotFoundException))]
        public void BaseMessageHandler_ThrowsException_WhenNoTenantsAreFound()
        {
            var noTenants = new List<TenantInformation>();
            _actionHandlerServiceRepositoryMock.Setup(a => a.GetTenantsFromTenantsDb()).ReturnsAsync(noTenants);
            //ActionHelper should not be called
            var handler = new NotificationMessageHandler(null, _tenantInfoRetriever, _configHelper);
            var message = new NotificationMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void BaseMessageHandler_ThrowsException_WhenTheRightTenantIsNotFound()
        {
            var wrongTenant = new List<TenantInformation>
            {
                new TenantInformation
                {
                    TenantId = TenantId + "different"
                }
            };
            _actionHandlerServiceRepositoryMock.Setup(a => a.GetTenantsFromTenantsDb()).ReturnsAsync(wrongTenant);
            //ActionHelper should not be called
            var handler = new NotificationMessageHandler(null, _tenantInfoRetriever, _configHelper);
            var message = new NotificationMessage();
            TestHandlerAndMessageWithHeader(handler, message);
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
        public void GenerateUserStoriesMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new GenerateUserStoriesMessageHandler();
            Assert.IsNotNull(handler);
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
        public void ArtifactsChangedMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new ArtifactsChangedMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void ArtifactsChangedMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new ArtifactsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new ArtifactsChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void ArtifactsChangedMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new ArtifactsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new ArtifactsChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void ProjectsChangedMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new ProjectsChangedMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void ProjectsChangedMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new ProjectsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new ProjectsChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void ProjectsChangedMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new ProjectsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new ProjectsChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void PropertyItemTypesChangedMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new PropertyItemTypesChangedMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void PropertyItemTypesChangedMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new PropertyItemTypesChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new PropertyItemTypesChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void PropertyItemTypesChangedMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new PropertyItemTypesChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new PropertyItemTypesChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void UsersGroupsChangedMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new UsersGroupsChangedMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void UsersGroupsChangedMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new UsersGroupsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new UsersGroupsChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void UsersGroupsChangedMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new UsersGroupsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new UsersGroupsChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        public void WorkflowsChangedMessageHandler_InstantiatesSuccessfully()
        {
            var handler = new WorkflowsChangedMessageHandler();
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WorkflowsChangedMessageHandler_HandlesMessageSuccessfully()
        {
            _handleActionSetup.Returns(Task.FromResult(true));
            var handler = new WorkflowsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new WorkflowsChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void WorkflowsChangedMessageHandler_RethrowsException()
        {
            _handleActionSetup.Throws(new TestException());
            var handler = new WorkflowsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetriever, _configHelper);
            var message = new WorkflowsChangedMessage();
            TestHandlerAndMessageWithHeader(handler, message);
        }
    }
}
