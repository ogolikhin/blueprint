using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories.Workflow
{
    [TestClass]
    public class SqlWorkflowRepositoryTests
    {
        #region GetTransitionsAsync
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetTransitionsAsync_NotFoundArtifact_ThrowsException()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, 
                new Mock<IArtifactPermissionsRepository>().Object);
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails>());

            // Act
            await repository.GetTransitionsAsync(1, 1, 1, 1);
        }
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetTransitionsAsync_NoReadPermissions_ThrowsException()
        {
            // Arrange
            var permissionsRepository = CreatePermissionsRepositoryMock(new[] { 1 }, 1, RolePermissions.None);
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, permissionsRepository.Object);
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails() { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor} });
            // Act
            await repository.GetTransitionsAsync(1, 1, 1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetTransitionsAsync_IncorrectArtifactType_ThrowsException()
        {
            // Arrange
            var permissionsRepository = CreatePermissionsRepositoryMock(new[] { 1 }, 1, RolePermissions.None);
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, permissionsRepository.Object);
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails() { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Project } });
            // Act
            await repository.GetTransitionsAsync(1, 1, 1, 1);
        }

        [TestMethod]
        public async Task GetTransitionsAsync_WithEditPermissions_SuccessfullyReads()
        {
            // Arrange
            var permissionsRepository = CreatePermissionsRepositoryMock(new[] { 1 }, 1, RolePermissions.Edit);
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, permissionsRepository.Object);
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor } });
            cxn.SetupQueryAsync("GetAvailableTransitions",
             new Dictionary<string, object>
             {
                 {"workflowId", 1},
                 { "stateId", 1 },
                 {"userId", 1}
             },
             new List<SqlWorkflowTransition>
             {
                 new SqlWorkflowTransition
                 {
                    TriggerId = 1,
                    StateId = 2,
                    StateName = "A",
                    CurrentStateId = 1,
                    TriggerName = "TA"
                 },
                 new SqlWorkflowTransition
                 {
                    TriggerId = 2,
                    StateId = 3,
                    StateName = "B",
                    CurrentStateId = 1,
                    TriggerName = "TB"
                 },
                 new SqlWorkflowTransition
                 {
                    TriggerId = 3,
                    StateId = 4,
                    StateName = "C",
                    CurrentStateId = 1,
                    TriggerName = "TC"
                 }
             });
            // Act
            var result = (await repository.GetTransitionsAsync(1, 1, 1, 1));

            Assert.IsTrue(result.Total == 3, "Transitions could not be retrieved");
        }
        #endregion

        #region GetCurrentState

        [TestMethod]
        public async Task GetCurrentState_WithEditPermissions_SuccessfullyReads()
        {
            // Arrange
            var permissionsRepository = CreatePermissionsRepositoryMock(new[] { 1 }, 1, RolePermissions.Edit | RolePermissions.Read);
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, permissionsRepository.Object);
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor } });
            cxn.SetupQueryAsync("GetCurrentWorkflowState",
             new Dictionary<string, object>
             {
                 {"userId", 1},
                 {"artifactId", 1 },
                 {"revisionId", 2147483647},
                 {"addDrafts", true}
            },
             new List<SqlWorkFlowState>
             {
                 new SqlWorkFlowState
                 {
                    WorkflowStateId = 1,
                    WorkflowStateName = "A",
                    WorkflowId = 1
                 }
             });
            
            // Act
            var result = (await repository.GetStateForArtifactAsync(1, 1, int.MaxValue, true));
            
            Assert.IsTrue(result.ResultCode == QueryResultCode.Success, "Result is not success");
            Assert.IsTrue(result.Item != null, "Workflow State is null");
        }
        [TestMethod]
        public async Task GetCurrentState_StoredProcedureReturnsNull_ReturnsFailureResult()
        {
            // Arrange
            var permissionsRepository = CreatePermissionsRepositoryMock(new[] { 1 }, 1, RolePermissions.Edit | RolePermissions.Read);
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, permissionsRepository.Object);
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor } });
            cxn.SetupQueryAsync("GetCurrentWorkflowState",
             new Dictionary<string, object>
             {
                 {"userId", 1},
                 {"artifactId", 1 },
                 {"revisionId", 2147483647},
                 {"addDrafts", true}
            },
             new List<SqlWorkFlowState>());

            // Act
            var result = (await repository.GetStateForArtifactAsync(1, 1, int.MaxValue, true));
            
            Assert.IsTrue(result.ResultCode == QueryResultCode.Failure, "Result is success");
            Assert.IsFalse(String.IsNullOrEmpty(result.Message), "Error message is null");
            Assert.IsTrue(result.Item == null, "Workflow State is not null");
        }
        #endregion

        private static Mock<IArtifactPermissionsRepository> CreatePermissionsRepositoryMock(int[] artifactIds, int userId, RolePermissions rolePermissions)
        {
            var permissions = artifactIds.ToDictionary(id => id, id => rolePermissions);
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(
                m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true))
                .ReturnsAsync(permissions);
            return mockArtifactPermissionsRepository;
        }
    }
}
