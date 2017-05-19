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
            await repository.GetTransitions(1, 1);
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
              new List<ArtifactBasicDetails> { new ArtifactBasicDetails() });
            // Act
            await repository.GetTransitions(1, 1);
        }
        [TestMethod]
        public async Task GetTransitions_WithReadPermissions_SuccessfullyReads()
        {
            // Arrange
            var permissionsRepository = CreatePermissionsRepositoryMock(new[] { 1 }, 1, RolePermissions.Read);
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlWorkflowRepository(cxn.Object, permissionsRepository.Object);
            cxn.SetupQueryAsync("GetArtifactBasicDetails",
              new Dictionary<string, object>
              {
                    {"userId", 1},
                    {"itemId", 1}
              },
              new List<ArtifactBasicDetails>() { new ArtifactBasicDetails() });
            const int transitionId = 5;
            cxn.SetupQueryAsync("GetAvailableTransitions",
             new Dictionary<string, object>
             {
                    {"artifactId", 1},
                    {"userId", 1}
             },
             new List<WorkflowTransition>
             {
                 new WorkflowTransition
                 {
                     Id = transitionId
                 }
             });
            // Act
            var result = (await repository.GetTransitions(1, 1));

            Assert.IsTrue(result.Count > 0);
            Assert.IsTrue(result.Items.First().Id == transitionId);
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
