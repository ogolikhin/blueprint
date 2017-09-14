using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.GenerateDescendants;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
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

        private Mock<IGenerateActionsRepository> _actionHandMock;
        private Mock<ISqlItemTypeRepository> _itemTypeRepoMock;
        private Mock<IUsersRepository> _userRepoMock; 
        private Mock<IJobsRepository> _jobsRepoMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _actionHandMock = new Mock<IGenerateActionsRepository>(MockBehavior.Strict);
            _itemTypeRepoMock = new Mock<ISqlItemTypeRepository>(MockBehavior.Strict);
            _jobsRepoMock = new Mock<IJobsRepository>(MockBehavior.Strict);
            _userRepoMock = new Mock<IUsersRepository>(MockBehavior.Strict);
            _actionHandMock.Setup(t => t.ItemTypeRepository).Returns(_itemTypeRepoMock.Object);
            _actionHandMock.Setup(t => t.JobsRepository).Returns(_jobsRepoMock.Object);
            _actionHandMock.Setup(t => t.UsersRepository).Returns(_userRepoMock.Object);
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_NullMessage_HandleActionReturnsFalse()
        {
            var actionHelper = new GenerateDescendantsActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_NullTenant_HandleActionReturnsFalse()
        {
            var actionHelper = new GenerateDescendantsActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_BoundardyIsReachedForProjectTenant_HandleActionReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _actionHandMock.Setup(t => t.IsBoundaryReached(ProjectId)).ReturnsAsync(true);

            //Act
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_ArtifactIdIsInvalid_HandleActionReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _actionHandMock.Setup(t => t.IsBoundaryReached(ProjectId)).ReturnsAsync(false);

            //Act
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_RevisionIdIsInvalid_HandleActionReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _actionHandMock.Setup(t => t.IsBoundaryReached(ProjectId)).ReturnsAsync(false);

            //Act
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_DesiredArtifactTypeIdIsInvalid_HandleActionReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _actionHandMock.Setup(t => t.IsBoundaryReached(ProjectId)).ReturnsAsync(false);

            //Act
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_UserNameIsInvalid_HandleActionReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _actionHandMock.Setup(t => t.IsBoundaryReached(ProjectId)).ReturnsAsync(false);

            //Act
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_DesiredItemTypeNotFound_HandleActionReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _actionHandMock.Setup(t => t.IsBoundaryReached(ProjectId)).ReturnsAsync(false);
            _itemTypeRepoMock.Setup(
                t => t.GetCustomItemTypeForProvidedStandardItemTypeIdInProject(ProjectId, DesiredArtifactTypeId))
                .ReturnsAsync((SqlItemType)null);

            //Act
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_CannotCreateJob_HandleActionReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _actionHandMock.Setup(t => t.IsBoundaryReached(ProjectId)).ReturnsAsync(false);
            _itemTypeRepoMock.Setup(
                t => t.GetCustomItemTypeForProvidedStandardItemTypeIdInProject(ProjectId, DesiredArtifactTypeId))
                .ReturnsAsync(new SqlItemType
                {
                    ProjectId = ProjectId,
                    ItemTypeId = DesiredArtifactTypeId,
                    Name = "Test"
                });

            _userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new List<SqlUser>
                {
                    new SqlUser
                    {
                        UserId = UserId,
                        Login = UserName
                    }
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

            //Act
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_CreatesJob_HandleActionReturnsTrue()
        {
            //Arrange
            var actionHelper = new GenerateDescendantsActionHelper();
            _actionHandMock.Setup(t => t.IsBoundaryReached(ProjectId)).ReturnsAsync(false);
            _itemTypeRepoMock.Setup(
                t => t.GetCustomItemTypeForProvidedStandardItemTypeIdInProject(ProjectId, DesiredArtifactTypeId))
                .ReturnsAsync(new SqlItemType
                {
                    ProjectId = ProjectId,
                    ItemTypeId = DesiredArtifactTypeId,
                    Name = "Test"
                });

            _userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new List<SqlUser>
                {
                    new SqlUser
                    {
                        UserId = UserId,
                        Login = UserName
                    }
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

            //Act
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsTrue(result, "Action should have succeeded");
        }

        [TestMethod]
        public async Task GenerateDescendantsActionHelper_ReturnsFalse_WhenAncestorInfiniteLoopExists()
        {
            //arrange
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
            _actionHandMock.Setup(m => m.IsBoundaryReached(It.IsAny<int>())).ReturnsAsync(false);
            var tenantInformation = new TenantInformation();
            var actionHelper = new GenerateDescendantsActionHelper();

            //act
            var result = await actionHelper.HandleAction(tenantInformation, generateDescendantsMessage, _actionHandMock.Object);

            //assert
            Assert.IsFalse(result, "Children should not have been generated.");
        }
    }
}
