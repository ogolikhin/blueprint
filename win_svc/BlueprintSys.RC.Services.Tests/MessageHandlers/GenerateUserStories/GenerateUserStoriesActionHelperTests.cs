﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers;
using BlueprintSys.RC.Services.MessageHandlers.GenerateUserStories;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.GenerateUserStories
{
    /// <summary>
    /// Tests for the Generate User Stories Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class GenerateUserStoriesActionHelperTests
    {
        private Mock<IJobsRepository> _repositoryMock;
        private TenantInformation _tenantInformation;

        [TestInitialize]
        public void TestInitialize()
        {
            _repositoryMock = new Mock<IJobsRepository>();

            int jobId = 10;
            _repositoryMock.Setup(m => m.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(jobId);
            _tenantInformation = new TenantInformation { BlueprintConnectionString = "", TenantId = "" };
        }

        [TestMethod]
        public async Task HandleAction_NullMessage_ReturnsFalse()
        {
            var actionHelper = new GenerateUserStoriesActionHelper();
            var result = await actionHelper.HandleAction(new TenantInformation(), null, null);
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task HandleAction_NullTenant_ReturnsFalse()
        {
            var actionHelper = new GenerateUserStoriesActionHelper();
            var result = await actionHelper.HandleAction(null, new GenerateUserStoriesMessage(), null);
            Assert.IsFalse(result, "Action should have failed for null tenant");
        }

        [TestMethod]
        public async Task HandleAction_CannotCreateJob_ReturnsFalse()
        {
            var message = new GenerateUserStoriesMessage
            {
                UserId = 123,
                UserName = "admin",
                ArtifactId = 23,
                BaseHostUri = "http://localhost:9801",
                ProjectId = 123,
                ProjectName = "my project",
                RevisionId = 123
            };
            var actionHelper = new GenerateUserStoriesActionHelper();
            var actionHandlerServiceRepositoryMock = new Mock<IGenerateActionsRepository>();
            var jobServicesMock = new Mock<IJobsRepository>();
            jobServicesMock.Setup(t => t.AddJobMessage(JobType.GenerateUserStories,
                false,
                It.IsAny<string>(),
                null,
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).ReturnsAsync((int?) null);
            var userRepoMock = new Mock<IUsersRepository>();
            actionHandlerServiceRepositoryMock.Setup(t => t.UsersRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new[] { new SqlUser
                {
                    UserId = 123,
                    Login = "admin"
                }});
            actionHandlerServiceRepositoryMock.Setup(t => t.JobsRepository).Returns(jobServicesMock.Object);

            //Act
            var result = await actionHelper.HandleAction(_tenantInformation, message, actionHandlerServiceRepositoryMock.Object);

            //Assert
            Assert.IsFalse(result, "Should return false if job creation fails.");
        }

        [TestMethod]
        public async Task HandleAction_WhenMessageIsValid_ReturnsTrue()
        {
            var message = new GenerateUserStoriesMessage
            {
                UserId = 1,
                UserName = "admin",
                ArtifactId = 23,
                BaseHostUri = "http://localhost:9801",
                ProjectId = 1,
                ProjectName = "test",
                RevisionId = 1
            };
            var actionHelper = new GenerateUserStoriesActionHelper();
            var actionHandlerServiceRepositoryMock = new Mock<IGenerateActionsRepository>();
            var jobServicesMock = new Mock<IJobsRepository>();
            jobServicesMock.Setup(t => t.AddJobMessage(JobType.GenerateUserStories,
                false,
                It.IsAny<string>(),
                null,
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                )).ReturnsAsync(1);
            var userRepoMock = new Mock<IUsersRepository>();
            actionHandlerServiceRepositoryMock.Setup(t => t.UsersRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new [] { new SqlUser
                {
                    UserId = 1,
                    Login = "admin"
                }});
            actionHandlerServiceRepositoryMock.Setup(t => t.JobsRepository).Returns(jobServicesMock.Object);

            //Act
            var result = await actionHelper.HandleAction(_tenantInformation, message, actionHandlerServiceRepositoryMock.Object);

            //Assert
            Assert.IsTrue(result);
        }
    }
}
