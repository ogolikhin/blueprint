﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers;
using BlueprintSys.RC.Services.MessageHandlers.GenerateDescendants;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ItemType;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.GenerateDescendants
{
    /// <summary>
    /// Tests for the Generate Descendants Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class GenerateDescendantsActionHelperTests
    {
        private const int ProjectId = 1;
        private const int ArtifactId = 2;
        private const int UserId = 3;
        private const int DesiredArtifactTypeId = 4;
        private const int RevisionId = 5;
        private const string UserName = "testUser";

        private Mock<IGenerateActionsRepository> _generateActionsRepositoryMock;
        private Mock<ISqlItemTypeRepository> _itemTypeRepoMock;
        private Mock<IUsersRepository> _userRepoMock;
        private Mock<IJobsRepository> _jobsRepoMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _generateActionsRepositoryMock = new Mock<IGenerateActionsRepository>(MockBehavior.Strict);
            _itemTypeRepoMock = new Mock<ISqlItemTypeRepository>(MockBehavior.Strict);
            _jobsRepoMock = new Mock<IJobsRepository>(MockBehavior.Strict);
            _userRepoMock = new Mock<IUsersRepository>(MockBehavior.Strict);
            _generateActionsRepositoryMock.Setup(t => t.ItemTypeRepository).Returns(_itemTypeRepoMock.Object);
            _generateActionsRepositoryMock.Setup(t => t.JobsRepository).Returns(_jobsRepoMock.Object);
            _generateActionsRepositoryMock.Setup(t => t.UsersRepository).Returns(_userRepoMock.Object);
        }

        [TestMethod]
        public async Task HandleAction_NullMessage_ReturnsFalse()
        {
            var actionHelper = new GenerateDescendantsActionHelper();
            var result = await actionHelper.HandleAction(new TenantInformation(), null, null);
            Assert.IsFalse(result, "Action should have failed for null message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_NullTenant_ReturnsFalse()
        {
            var actionHelper = new GenerateDescendantsActionHelper();
            var result = await actionHelper.HandleAction(null, new GenerateDescendantsMessage(), null);
            Assert.IsFalse(result, "Action should have failed for null tenant");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_BoundardyIsReachedForProjectTenant_ReturnsFalse()
        {
            // Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _generateActionsRepositoryMock.Setup(t => t.IsProjectMaxArtifactBoundaryReached(ProjectId)).ReturnsAsync(true);

            // Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateDescendantsMessage
            {
                ArtifactId = ArtifactId,
                ProjectId = ProjectId,
                UserId = UserId,
                RevisionId = RevisionId,
                DesiredArtifactTypeId = DesiredArtifactTypeId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result, "Action should have failed when boundary reached");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_ArtifactIdIsInvalid_ReturnsFalse()
        {
            // Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _generateActionsRepositoryMock.Setup(t => t.IsProjectMaxArtifactBoundaryReached(ProjectId)).ReturnsAsync(false);

            // Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateDescendantsMessage
            {
                UserId = UserId,
                ProjectId = ProjectId,
                RevisionId = RevisionId,
                DesiredArtifactTypeId = DesiredArtifactTypeId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result, "Action should have failed for invalid message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_RevisionIdIsInvalid_ReturnsFalse()
        {
            // Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _generateActionsRepositoryMock.Setup(t => t.IsProjectMaxArtifactBoundaryReached(ProjectId)).ReturnsAsync(false);

            // Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateDescendantsMessage
            {
                ArtifactId = ArtifactId,
                ProjectId = ProjectId,
                UserId = UserId,
                DesiredArtifactTypeId = DesiredArtifactTypeId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result, "Action should have failed for invalid message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_DesiredArtifactTypeIdIsInvalid_ReturnsFalse()
        {
            // Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _generateActionsRepositoryMock.Setup(t => t.IsProjectMaxArtifactBoundaryReached(ProjectId)).ReturnsAsync(false);

            // Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateDescendantsMessage
            {
                ArtifactId = ArtifactId,
                UserId = UserId,
                RevisionId = RevisionId,
                ProjectId = ProjectId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result, "Action should have failed for invalid message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_UserNameIsInvalid_ReturnsFalse()
        {
            // Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _generateActionsRepositoryMock.Setup(t => t.IsProjectMaxArtifactBoundaryReached(ProjectId)).ReturnsAsync(false);

            // Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateDescendantsMessage
            {
                ArtifactId = ArtifactId,
                ProjectId = ProjectId,
                UserId = UserId,
                RevisionId = RevisionId,
                DesiredArtifactTypeId = DesiredArtifactTypeId
            },
            _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result, "Action should have failed for invalid message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_DesiredItemTypeNotFound_ReturnsFalse()
        {
            // Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _generateActionsRepositoryMock.Setup(t => t.IsProjectMaxArtifactBoundaryReached(ProjectId)).ReturnsAsync(false);
            _itemTypeRepoMock.Setup(
                t => t.GetCustomItemTypeForProvidedStandardItemTypeIdInProject(ProjectId, DesiredArtifactTypeId))
                .ReturnsAsync((SqlItemType)null);

            // Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateDescendantsMessage
            {
                ArtifactId = ArtifactId,
                UserId = UserId,
                RevisionId = RevisionId,
                ProjectId = ProjectId,
                DesiredArtifactTypeId = DesiredArtifactTypeId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result, "Action should have failed for invalid message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_CannotCreateJob_ReturnsFalse()
        {
            // Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _generateActionsRepositoryMock.Setup(t => t.IsProjectMaxArtifactBoundaryReached(ProjectId)).ReturnsAsync(false);
            _itemTypeRepoMock.Setup(
                t => t.GetCustomItemTypeForProvidedStandardItemTypeIdInProject(ProjectId, DesiredArtifactTypeId))
                .ReturnsAsync(new SqlItemType
                {
                    ProjectId = ProjectId,
                    ItemTypeId = DesiredArtifactTypeId,
                    Name = "Test"
                });
            var sqlUser = new SqlUser
            {
                UserId = UserId,
                Login = UserName
            };
            _generateActionsRepositoryMock.Setup(m => m.GetUser(It.IsAny<int>())).ReturnsAsync(sqlUser);
            _userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new List<SqlUser>
                {
                    sqlUser
                });

            _jobsRepoMock.Setup(t => t.AddJobMessage(JobType.GenerateDescendants,
                false,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                UserId,
                UserName,
                It.IsAny<string>())).ReturnsAsync((int?)null);

            // Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateDescendantsMessage
            {
                ArtifactId = ArtifactId,
                UserId = UserId,
                RevisionId = RevisionId,
                ProjectId = ProjectId,
                DesiredArtifactTypeId = DesiredArtifactTypeId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result, "Action should have failed when job creation failed");
            _jobsRepoMock.Verify(t => t.AddJobMessage(JobType.GenerateDescendants, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task HandleAction_CreatesJob_ReturnsTrue()
        {
            // Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _generateActionsRepositoryMock.Setup(t => t.IsProjectMaxArtifactBoundaryReached(ProjectId)).ReturnsAsync(false);
            _itemTypeRepoMock.Setup(
                t => t.GetCustomItemTypeForProvidedStandardItemTypeIdInProject(ProjectId, DesiredArtifactTypeId))
                .ReturnsAsync(new SqlItemType
                {
                    ProjectId = ProjectId,
                    ItemTypeId = DesiredArtifactTypeId,
                    Name = "Test"
                });
            var sqlUser = new SqlUser
            {
                UserId = UserId,
                Login = UserName
            };
            _generateActionsRepositoryMock.Setup(m => m.GetUser(It.IsAny<int>())).ReturnsAsync(sqlUser);
            _userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new List<SqlUser>
                {
                    sqlUser
                });

            _jobsRepoMock.Setup(t => t.AddJobMessage(JobType.GenerateDescendants,
                false,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                UserId,
                UserName,
                It.IsAny<string>())).ReturnsAsync(2);

            // Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateDescendantsMessage
            {
                ArtifactId = ArtifactId,
                UserId = UserId,
                RevisionId = RevisionId,
                ProjectId = ProjectId,
                DesiredArtifactTypeId = DesiredArtifactTypeId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsTrue(result, "Action should have succeeded");
            _jobsRepoMock.Verify(t => t.AddJobMessage(JobType.GenerateDescendants, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task HandleAction_WhenAncestorInfiniteLoopExists_ReturnsFalse()
        {
            // arrange
            const int duplicateId = 11;
            var ancestorsWithDuplicate = new[]
            {
                duplicateId,
                12,
                13,
                14,
                duplicateId
            };
            var generateDescendantsMessage = new GenerateDescendantsMessage
            {
                AncestorArtifactTypeIds = ancestorsWithDuplicate,
                ArtifactId = ArtifactId,
                BaseHostUri = "uri",
                ChildCount = 1,
                DesiredArtifactTypeId = DesiredArtifactTypeId,
                ProjectId = ProjectId,
                ProjectName = "project",
                RevisionId = RevisionId,
                TypePredefined = 1,
                UserId = UserId,
                UserName = UserName
            };
            _generateActionsRepositoryMock.Setup(m => m.IsProjectMaxArtifactBoundaryReached(It.IsAny<int>())).ReturnsAsync(false);
            var tenantInformation = new TenantInformation();
            var actionHelper = new GenerateDescendantsActionHelper();

            // act
            var result = await actionHelper.HandleAction(tenantInformation, generateDescendantsMessage, _generateActionsRepositoryMock.Object);

            // assert
            Assert.IsFalse(result, "Children should not have been generated.");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
