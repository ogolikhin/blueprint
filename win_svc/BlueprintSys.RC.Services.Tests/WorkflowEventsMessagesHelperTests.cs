using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Webhooks;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the WorkflowEventsMessagesHelper
    /// </summary>
    [TestClass]
    public class WorkflowEventsMessagesHelperTests
    {
        private int _userId;
        private int _revisionId;
        private string _userName;
        private long _transactionId;
        private WorkflowEventTriggers _workflowEventTriggers;
        private WorkflowMessageArtifactInfo _baseArtifactVersionControlInfo;
        private string _projectName;
        private Dictionary<int, IList<Property>> _modifiedProperties;
        private string _artifactUrl;
        private string _baseUrl;
        private int[] _ancestorArtifactTypeIds;
        private Mock<IUsersRepository> _mockUsersRepository;
        private Mock<IServiceLogRepository> _mockServiceLogRepository;
        private Mock<IWebhooksRepository> _mockWebhooksRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            _userId = 0;
            _revisionId = 0;
            _userName = "";
            _transactionId = 123;
            _workflowEventTriggers = new WorkflowEventTriggers();
            _baseArtifactVersionControlInfo = new WorkflowMessageArtifactInfo();
            _projectName = "";
            _modifiedProperties = new Dictionary<int, IList<Property>>();
            _artifactUrl = "";
            _baseUrl = "";
            _ancestorArtifactTypeIds = new int[] { };
            _mockUsersRepository = new Mock<IUsersRepository>(MockBehavior.Strict);
            _mockServiceLogRepository = new Mock<IServiceLogRepository>(MockBehavior.Strict);
            _mockWebhooksRepository = new Mock<IWebhooksRepository>(MockBehavior.Strict);
        }

        [TestMethod]
        public async Task GenerateMessages_ReturnsNotificationMessage_WhenActionIsEmailNotificationAction()
        {
            // arrange
            var workflowEventTrigger = new WorkflowEventTrigger
            {
                Action = new EmailNotificationAction()
            };
            _workflowEventTriggers.Add(workflowEventTrigger);

            // act
            var messages = await WorkflowEventsMessagesHelper.GenerateMessages(_userId, _revisionId, _userName, _transactionId, _workflowEventTriggers, _baseArtifactVersionControlInfo, _projectName, _modifiedProperties, _artifactUrl, _baseUrl, _ancestorArtifactTypeIds, _mockUsersRepository.Object, _mockServiceLogRepository.Object, _mockWebhooksRepository.Object);

            // assert
            Assert.IsTrue(messages.Count == 1);
            Assert.IsTrue(messages.Single() is NotificationMessage);
        }

        [TestMethod]
        public async Task GetEmailValues_ReturnsEmails_WhenEmailNotificationActionContainsEmails()
        {
            // arrange
            const int numEmails = 5;
            var emailNotificationAction = new EmailNotificationAction();
            for (int i = 0; i < numEmails; i++)
            {
                emailNotificationAction.Emails.Add($"email{i}");
            }

            // act
            var emails = await WorkflowEventsMessagesHelper.GetEmailValues(_revisionId, 123, emailNotificationAction, _mockUsersRepository.Object);

            // assert
            Assert.AreEqual(numEmails, emails.Count);
        }

        [TestMethod]
        public async Task GetEmailValues_ReturnsEmails_WhenEmailNotificationActionHasPropertyTypeId()
        {
            // arrange
            const int numEmails = 7;
            var emailNotificationAction = new EmailNotificationAction
            {
                PropertyTypeId = 123
            };
            var userInfos = new List<UserInfo>();
            for (int i = 0; i < numEmails; i++)
            {
                var userInfo = new UserInfo()
                {
                    Email = $"email{i}"
                };
                userInfos.Add(userInfo);
            }
            _mockUsersRepository.Setup(m => m.GetUserInfoForWorkflowArtifactForAssociatedUserProperty(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDbTransaction>())).ReturnsAsync(userInfos);

            // act
            var emails = await WorkflowEventsMessagesHelper.GetEmailValues(_revisionId, 456, emailNotificationAction, _mockUsersRepository.Object);

            // assert
            Assert.AreEqual(numEmails, emails.Count);
        }

        [TestMethod]
        public async Task GenerateMessages_ReturnsGenerateDescendantsMessage_WhenActionIsGenerateChildrenAction()
        {
            // arrange
            var workflowEventTrigger = new WorkflowEventTrigger
            {
                Action = new GenerateChildrenAction()
            };
            _workflowEventTriggers.Add(workflowEventTrigger);

            // act
            var messages = await WorkflowEventsMessagesHelper.GenerateMessages(_userId, _revisionId, _userName, _transactionId, _workflowEventTriggers, _baseArtifactVersionControlInfo, _projectName, _modifiedProperties, _artifactUrl, _baseUrl, _ancestorArtifactTypeIds, _mockUsersRepository.Object, _mockServiceLogRepository.Object, _mockWebhooksRepository.Object);

            // assert
            Assert.IsTrue(messages.Count == 1);
            Assert.IsTrue(messages.Single() is GenerateDescendantsMessage);
        }

        [TestMethod]
        public async Task GenerateMessages_ReturnsGenerateTestsMessage_WhenActionIsGenerateTestCasesActionAndPredefinedTypeIsProcess()
        {
            // arrange
            var workflowEventTrigger = new WorkflowEventTrigger
            {
                Action = new GenerateTestCasesAction()
            };
            _workflowEventTriggers.Add(workflowEventTrigger);
            _baseArtifactVersionControlInfo.PredefinedType = ItemTypePredefined.Process;

            // act
            var messages = await WorkflowEventsMessagesHelper.GenerateMessages(_userId, _revisionId, _userName, _transactionId, _workflowEventTriggers, _baseArtifactVersionControlInfo, _projectName, _modifiedProperties, _artifactUrl, _baseUrl, _ancestorArtifactTypeIds, _mockUsersRepository.Object, _mockServiceLogRepository.Object, _mockWebhooksRepository.Object);

            // assert
            Assert.IsTrue(messages.Count == 1);
            Assert.IsTrue(messages.Single() is GenerateTestsMessage);
        }

        [TestMethod]
        public async Task GenerateMessages_ReturnsNoMessage_WhenActionIsGenerateTestCasesActionAndPredefinedTypeIsNotProcess()
        {
            // arrange
            var workflowEventTrigger = new WorkflowEventTrigger
            {
                Action = new GenerateTestCasesAction()
            };
            _workflowEventTriggers.Add(workflowEventTrigger);
            _baseArtifactVersionControlInfo.PredefinedType = ItemTypePredefined.None;
            _mockServiceLogRepository.Setup(m => m.LogInformation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(true));

            // act
            var messages = await WorkflowEventsMessagesHelper.GenerateMessages(_userId, _revisionId, _userName, _transactionId, _workflowEventTriggers, _baseArtifactVersionControlInfo, _projectName, _modifiedProperties, _artifactUrl, _baseUrl, _ancestorArtifactTypeIds, _mockUsersRepository.Object, _mockServiceLogRepository.Object, _mockWebhooksRepository.Object);

            // assert
            Assert.IsTrue(messages.Count == 0);
        }

        [TestMethod]
        public async Task GenerateMessages_ReturnsGenerateUserStoriesMessage_WhenActionIsGenerateUserStoriesActionAndPredefinedTypeIsProcess()
        {
            // arrange
            var workflowEventTrigger = new WorkflowEventTrigger
            {
                Action = new GenerateUserStoriesAction()
            };
            _workflowEventTriggers.Add(workflowEventTrigger);
            _baseArtifactVersionControlInfo.PredefinedType = ItemTypePredefined.Process;

            // act
            var messages = await WorkflowEventsMessagesHelper.GenerateMessages(_userId, _revisionId, _userName, _transactionId, _workflowEventTriggers, _baseArtifactVersionControlInfo, _projectName, _modifiedProperties, _artifactUrl, _baseUrl, _ancestorArtifactTypeIds, _mockUsersRepository.Object, _mockServiceLogRepository.Object, _mockWebhooksRepository.Object);

            // assert
            Assert.IsTrue(messages.Count == 1);
            Assert.IsTrue(messages.Single() is GenerateUserStoriesMessage);
        }

        [TestMethod]
        public async Task GenerateMessages_ReturnsNoMessage_WhenActionIsGenerateUserStoriesActionAndPredefinedTypeIsNotProcess()
        {
            // arrange
            var workflowEventTrigger = new WorkflowEventTrigger
            {
                Action = new GenerateUserStoriesAction()
            };
            _workflowEventTriggers.Add(workflowEventTrigger);
            _baseArtifactVersionControlInfo.PredefinedType = ItemTypePredefined.None;
            _mockServiceLogRepository.Setup(m => m.LogInformation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(true));

            // act
            var messages = await WorkflowEventsMessagesHelper.GenerateMessages(_userId, _revisionId, _userName, _transactionId, _workflowEventTriggers, _baseArtifactVersionControlInfo, _projectName, _modifiedProperties, _artifactUrl, _baseUrl, _ancestorArtifactTypeIds, _mockUsersRepository.Object, _mockServiceLogRepository.Object, _mockWebhooksRepository.Object);

            // assert
            Assert.IsTrue(messages.Count == 0);
        }
    }
}
