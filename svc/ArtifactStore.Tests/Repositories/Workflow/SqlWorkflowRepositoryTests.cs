using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    TriggerId = 1,
                    ToStateId = 2,
                    ToStateName = "A",
                    FromStateId = 1,
                    TriggerName = "TA"
                 },
                 new SqlWorkflowTransition
                 {
                    TriggerId = 2,
                    ToStateId = 3,
                    ToStateName = "B",
                    FromStateId = 1,
                    TriggerName = "TB"
                 },
                 new SqlWorkflowTransition
                 {
                    TriggerId = 3,
                    ToStateId = 4,
                    ToStateName = "C",
                    FromStateId = 1,
                    TriggerName = "TC"
                 }
             });
            // Act
            var result = (await repository.GetTransitionsAsync(1, 1, 1, 1));

            Assert.IsTrue(result.Count == 3, "Transitions could not be retrieved");
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
            
            Assert.IsTrue(result != null, "Workflow State is null");
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
