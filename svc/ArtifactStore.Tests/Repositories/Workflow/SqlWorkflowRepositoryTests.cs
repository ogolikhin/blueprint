using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
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
            cxn.SetupQueryAsync("GetTransitionsForState",
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
                    WorkflowEventId = 1,
                    ToStateId = 2,
                    ToStateName = "A",
                    FromStateId = 1,
                    WorkflowEventName = "TA"
                 },
                 new SqlWorkflowTransition
                 {
                    WorkflowEventId = 2,
                    ToStateId = 3,
                    ToStateName = "B",
                    FromStateId = 1,
                    WorkflowEventName = "TB"
                 },
                 new SqlWorkflowTransition
                 {
                    WorkflowEventId = 3,
                    ToStateId = 4,
                    ToStateName = "C",
                    FromStateId = 1,
                    WorkflowEventName = "TC"
                 }
             });
            // Act
            var result = (await repository.GetTransitionsAsync(1, 1, 1, 1));

            Assert.IsTrue(result.Count == 3, "Transitions could not be retrieved");
        }
        #endregion

        #region GetTransitionForAssociatedStatesAsync

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetTransitionForAssociatedStatesAsync_NotFoundArtifact_ThrowsException()
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
        public async Task GetTransitionForAssociatedStatesAsync_NoReadPermissions_ThrowsException()
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
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails() { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor } });
            // Act
            await repository.GetTransitionsAsync(1, 1, 1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetTransitionForAssociatedStatesAsync_IncorrectArtifactType_ThrowsException()
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
        public async Task GetTransitionForAssociatedStatesAsync_WithEditPermissions_SuccessfullyReturnsTransition()
        {
            // Arrange
            var permissionsRepository = CreatePermissionsRepositoryMock(new[] { 1 }, 1, RolePermissions.Edit);
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, permissionsRepository.Object);
            int userId = 1;
            int workflowId = 4;
            int fromStateId = 5;
            int toStateId = 6;
            
            var expected = new SqlWorkflowTransition
            {
                WorkflowEventId = 1,
                ToStateId = toStateId,
                ToStateName = "New",
                FromStateId = fromStateId,
                FromStateName = "Ready",
                WorkflowEventName = "New To Redy",
                WorkflowId = workflowId
            };

            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", userId},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor } });
            cxn.SetupQueryAsync("GetTransitionAssociatedWithStates",
             new Dictionary<string, object>
             {
                 {"workflowId", workflowId},
                 { "fromStateId", fromStateId },
                 { "toStateId", toStateId },
                 {"userId", userId}
             },
             new List<SqlWorkflowTransition>
             {
                 expected
             });
            // Act
            var result = (await repository.GetTransitionForAssociatedStatesAsync(userId, 1, workflowId, fromStateId, toStateId));

            //Assert
            Assert.AreEqual(workflowId, result.WorkflowId);
            Assert.AreEqual(fromStateId, result.FromState.Id);
            Assert.AreEqual(toStateId, result.ToState.Id);
            Assert.AreEqual(1, result.Id);
        }

        #endregion

        #region ChangeStateForArtifactAsync

        [TestMethod]
        public async Task ChangeStateForArtifactAsync_WithEditPermissions_SuccessfullyReturnsState()
        {
            // Arrange
            int userId = 1;
            int workflowId = 4;
            int artifactId = 1;
            int desiredStateId = 6;

            var permissionsRepository = CreatePermissionsRepositoryMock(new[] { artifactId }, userId, RolePermissions.Edit);
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, permissionsRepository.Object);
            
            var stateChangeParam = new WorkflowStateChangeParameterEx
            {
                ToStateId = desiredStateId
            };
            var expected = new WorkflowState
            {
                Id = desiredStateId,
                Name = "Ready",
                WorkflowId = workflowId
            };

            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", userId},
                    {"itemId", artifactId}
              },
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor } });
            cxn.SetupQueryAsync("ChangeStateForArtifact",
             new Dictionary<string, object>
             {
                 { "userId", userId},
                 { "artifactId", artifactId },
                 { "desiredStateId", desiredStateId },
                 { "result", null}
             },
             new List<SqlWorkFlowState>
             {
                 new SqlWorkFlowState()
                 {
                     WorkflowId = workflowId,
                     WorkflowStateId = desiredStateId,
                     WorkflowStateName = "Ready",
                     Result = 0
                 }
             });
            // Act
            var result = (await repository.ChangeStateForArtifactAsync(userId, artifactId, stateChangeParam));

            //Assert
            Assert.AreEqual(workflowId, result.WorkflowId);
            Assert.AreEqual(desiredStateId, result.Id);
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

            
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(new [] {1}, "Int32Collection", "Int32Value");
            
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor } });
            cxn.SetupQueryAsync("GetWorkflowStatesForArtifacts",
             new Dictionary<string, object>
             {
                 {"userId", 1},
                 {"artifactIds", artifactIdsTable },
                 {"revisionId", 2147483647},
                 {"addDrafts", true}
            },
             new List<SqlWorkFlowStateInformation>
             {
                 new SqlWorkFlowStateInformation
                 {
                    WorkflowStateId = 1,
                    WorkflowStateName = "A",
                    WorkflowId = 1
                 }
             });
            
            // Act
            var result = (await repository.GetStateForArtifactAsync(1, 1, int.MaxValue, true));
            
            Assert.IsTrue(result != null, "Workflow State is null");
        }
        [TestMethod]
        public async Task GetCurrentState_StoredProcedureReturnsNull_ReturnsFailureResult()
        {
            // Arrange
            var permissionsRepository = CreatePermissionsRepositoryMock(new[] { 1 }, 1, RolePermissions.Edit | RolePermissions.Read);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(new[] { 1 }, "Int32Collection", "Int32Value");

            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, permissionsRepository.Object);
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor } });
            cxn.SetupQueryAsync("GetWorkflowStatesForArtifacts",
             new Dictionary<string, object>
             {
                 {"userId", 1},
                 {"artifactIds", artifactIdsTable },
                 {"revisionId", 2147483647},
                 {"addDrafts", true}
            },
             new List<SqlWorkFlowState>());

            // Act
            var result = (await repository.GetStateForArtifactAsync(1, 1, int.MaxValue, true));
            
            Assert.IsTrue(result == null, "Workflow State is not null");
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
