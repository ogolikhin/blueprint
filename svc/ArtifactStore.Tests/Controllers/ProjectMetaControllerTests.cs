using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.ProjectMeta;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class ProjectMetaControllerTests
    {
        [TestMethod]
        public async Task GetProjectTypesAsync_Success()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserId = userId };
            var projectId = 10;
            var types = new ProjectTypes();
            var mockProjectMetaRepository = new Mock<ISqlProjectMetaRepository>();
            mockProjectMetaRepository.Setup(r => r.GetCustomProjectTypesAsync(projectId, userId)).ReturnsAsync(types);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var projectMetaController = new ProjectMetaController(mockProjectMetaRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            projectMetaController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await projectMetaController.GetProjectTypesAsync(projectId);

            // Assert
            Assert.AreSame(types, result);
        }
    }
}
