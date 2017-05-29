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
        #region GetTransitions
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetTransitions_NotFoundArtifact_ThrowsException()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object);
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails>());

            // Act
            await repository.GetTransitions(1, 1, 1, 1);
        }
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetTransitions_NoReadPermissions_ThrowsException()
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
            await repository.GetTransitions(1, 1, 1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetTransitions_IncorrectArtifactType_ThrowsException()
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
            await repository.GetTransitions(1, 1, 1, 1);
        }

        [TestMethod]
        public async Task GetTransitions_WithEditPermissions_SuccessfullyReads()
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
             new List<Transition>
             {
                 new Transition
                 {
                    TriggerId = 1,
                    StateId = 2,
                    StateName = "A",
                    CurrentStateId = 1,
                    TriggerName = "TA"
                 },
                 new Transition
                 {
                    TriggerId = 2,
                    StateId = 3,
                    StateName = "B",
                    CurrentStateId = 1,
                    TriggerName = "TB"
                 },
                 new Transition
                 {
                    TriggerId = 3,
                    StateId = 4,
                    StateName = "C",
                    CurrentStateId = 1,
                    TriggerName = "TC"
                 }
             });
            // Act
            var result = (await repository.GetTransitions(1, 1, 1, 1));

            Assert.IsTrue(result.Total == 3, "Transitions could not be retrieved");
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
