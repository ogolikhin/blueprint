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
using NServiceBus;
using NServiceBus.Testing;
using ServiceLibrary.Models.Enums;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the Message Handlers
    /// </summary>
    [TestClass]
    public class HandlerTests
    {
        private Mock<IActionHelper> _actionHelperMock;
        private Mock<IConfigHelper> _configHelperMock;
        private Mock<ITenantInfoRetriever> _tenantInfoRetrieverMock;
        private Mock<ITransactionValidator> _transactionValidatorMock;
        private const string TenantId = "tenant0";

        [TestInitialize]
        public void TestInitialize()
        {
            _actionHelperMock = new Mock<IActionHelper>(MockBehavior.Strict);

            _configHelperMock = new Mock<IConfigHelper>(MockBehavior.Strict);
            _configHelperMock.Setup(m => m.SupportedActionTypes).Returns(MessageActionType.All);

            _tenantInfoRetrieverMock = new Mock<ITenantInfoRetriever>(MockBehavior.Strict);
            var tenant = new TenantInformation
            {
                TenantId = TenantId,
                BlueprintConnectionString = "test connection string"
            };
            var tenants = new Dictionary<string, TenantInformation>
            {
                { tenant.TenantId, tenant }
            };
            _tenantInfoRetrieverMock.Setup(m => m.GetTenants()).ReturnsAsync(tenants);

            _transactionValidatorMock = new Mock<ITransactionValidator>(MockBehavior.Strict);
            _transactionValidatorMock.Setup(m => m.GetStatus(It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>(), It.IsAny<BaseRepository>())).ReturnsAsync(TransactionStatus.Committed);
        }

        private static void TestHandlerAndMessageWithHeader<T>(BaseMessageHandler<T> handler, T message, string tenantId = TenantId) where T : ActionMessage
        {
            Test.Handler(handler).SetIncomingHeader(ActionMessageHeaders.TenantId, tenantId).SetIncomingHeader(Headers.MessageId, "0").SetIncomingHeader(Headers.TimeSent, "0").OnMessage(message);
        }

        [Serializable]
        public class TestException : Exception
        {
            public TestException(string message = "An exception used for testing") : base(message)
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedActionTypeException))]
        public void BaseMessageHandler_ThrowsUnsupportedActionTypeException_WhenActionTypeIsNotSupported()
        {
            //arrange
            _configHelperMock.Setup(m => m.SupportedActionTypes).Returns(MessageActionType.None);
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new NotificationMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(MessageHeaderValueNotFoundException))]
        public void BaseMessageHandler_ThrowsMessageHeaderValueNotFoundException_WhenHeaderValueIsNotFound()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new NotificationMessage();
            //act
            //sends the message without setting headers
            Test.Handler(handler).OnMessage(message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void BaseMessageHandler_ThrowsException_WhenNoTenantsAreFound()
        {
            //arrange
            var noTenants = new Dictionary<string, TenantInformation>();
            _tenantInfoRetrieverMock.Setup(m => m.GetTenants()).ReturnsAsync(noTenants);
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new NotificationMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void BaseMessageHandler_ThrowsException_WhenTheRightTenantIsNotFound()
        {
            //arrange
            var tenant = new TenantInformation
            {
                TenantId = TenantId + "different"
            };
            var wrongTenant = new Dictionary<string, TenantInformation>
            {
                { tenant.TenantId, tenant }
            };
            _tenantInfoRetrieverMock.Setup(m => m.GetTenants()).ReturnsAsync(wrongTenant);
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new NotificationMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Never);
        }

        [TestMethod]
        public void BaseMessageHandler_DiscardsMessage_WhenTransactionIsRolledBack()
        {
            //arrange
            _transactionValidatorMock.Setup(m => m.GetStatus(It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>(), It.IsAny<BaseRepository>())).ReturnsAsync(TransactionStatus.RolledBack);
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new NotificationMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Never);
        }

        [TestMethod]
        public void ArtifactsPublishedMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new ArtifactsPublishedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new ArtifactsPublishedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void ArtifactsPublishedMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new ArtifactsPublishedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new ArtifactsPublishedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        public void NotificationMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new NotificationMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void NotificationMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new NotificationMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new NotificationMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        public void GenerateDescendantsMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new GenerateDescendantsMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new GenerateDescendantsMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void GenerateDescendantsMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new GenerateDescendantsMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new GenerateDescendantsMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        public void GenerateTestsMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new GenerateTestsMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new GenerateTestsMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void GenerateTestsMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new GenerateTestsMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new GenerateTestsMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        public void GenerateUserStoriesMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new GenerateUserStoriesMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new GenerateUserStoriesMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void GenerateUserStoriesMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new GenerateUserStoriesMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new GenerateUserStoriesMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        public void ArtifactsChangedMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new ArtifactsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new ArtifactsChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void ArtifactsChangedMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new ArtifactsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new ArtifactsChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        public void ProjectsChangedMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new ProjectsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new ProjectsChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void ProjectsChangedMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new ProjectsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new ProjectsChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        public void PropertyItemTypesChangedMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new PropertyItemTypesChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new PropertyItemTypesChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void PropertyItemTypesChangedMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new PropertyItemTypesChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new PropertyItemTypesChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        public void UsersGroupsChangedMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new UsersGroupsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new UsersGroupsChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void UsersGroupsChangedMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new UsersGroupsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new UsersGroupsChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        public void WorkflowsChangedMessageHandler_HandlesMessageSuccessfully()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Returns(Task.FromResult(true));
            var handler = new WorkflowsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new WorkflowsChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            Assert.IsNotNull(handler);
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public void WorkflowsChangedMessageHandler_RethrowsException()
        {
            //arrange
            _actionHelperMock.Setup(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>())).Throws(new TestException());
            var handler = new WorkflowsChangedMessageHandler(_actionHelperMock.Object, _tenantInfoRetrieverMock.Object, _configHelperMock.Object, _transactionValidatorMock.Object);
            var message = new WorkflowsChangedMessage();
            //act
            TestHandlerAndMessageWithHeader(handler, message);
            //assert
            _actionHelperMock.Verify(m => m.HandleAction(It.IsAny<TenantInformation>(), It.IsAny<ActionMessage>(), It.IsAny<BaseRepository>()), Times.Once);
        }
    }
}
