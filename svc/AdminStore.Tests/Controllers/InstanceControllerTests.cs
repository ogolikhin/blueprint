using AdminStore.Models;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Controllers
{
    [TestClass]
    public class InstanceControllerTests
    {
        [TestMethod]
        public async Task GetInstanceFolderAsync_Success()
        {
            //Arrange
            var folderId = 99;
            var userId = 9;
            var session = new Session { UserId = userId };
            var folder = new InstanceItem { Id = folderId };
            var mockInstanceRepository = new Mock<ISqlInstanceRepository>();
            mockInstanceRepository.Setup(r => r.GetInstanceFolderAsync(folderId, userId)).ReturnsAsync(folder);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var instanceController = new InstanceController(mockInstanceRepository.Object, mockServiceLogRepository.Object, null)
            {
                Request = new HttpRequestMessage()
            };
            instanceController.Request.Properties[ServiceConstants.SessionProperty] = session;

            //Act
            var result = await instanceController.GetInstanceFolderAsync(folderId);

            //Assert
            Assert.AreSame(folder, result);
        }

        [TestMethod]
        public async Task GetInstanceFolderChildrenAsync_Success()
        {
            //Arrange
            var userId = 88;
            var session = new Session { UserId = userId };
            var folderId = 99;
            var children = new List<InstanceItem>();
            var mockInstanceRepository = new Mock<ISqlInstanceRepository>();
            mockInstanceRepository.Setup(r => r.GetInstanceFolderChildrenAsync(folderId, userId)).ReturnsAsync(children);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var instanceController = new InstanceController(mockInstanceRepository.Object, mockServiceLogRepository.Object, null)
            {
                Request = new HttpRequestMessage()
            };
            instanceController.Request.Properties[ServiceConstants.SessionProperty] = session;

            //Act
            var result = await instanceController.GetInstanceFolderChildrenAsync(folderId);

            //Assert
            Assert.AreSame(children, result);
        }

        [TestMethod]
        public async Task GetInstanceProjectAsync_Success()
        {
            //Arrange
            var userId = 88;
            var session = new Session { UserId = userId };
            var projectId = 99;
            var project = new InstanceItem { Id = projectId };
            var mockInstanceRepository = new Mock<ISqlInstanceRepository>();
            mockInstanceRepository.Setup(r => r.GetInstanceProjectAsync(projectId, userId)).ReturnsAsync(project);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var instanceController = new InstanceController(mockInstanceRepository.Object, mockServiceLogRepository.Object, null)
            {
                Request = new HttpRequestMessage()
            };
            instanceController.Request.Properties[ServiceConstants.SessionProperty] = session;

            //Act
            var result = await instanceController.GetInstanceProjectAsync(projectId);

            //Assert
            Assert.AreSame(project, result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetProjectNavigationPathAsync_InvalidPermission()
        {
            //Arrange
            const int userId = 88;
            var session = new Session { UserId = userId };
            const int projectId = 99;
            const bool includeProjectItself = true;
            var repositoryResult = new List<string> { "Blueprint", "ProjectName" };
            var mockInstanceRepository = new Mock<ISqlInstanceRepository>();
            mockInstanceRepository.Setup(r => r.GetProjectNavigationPathAsync(projectId, userId, includeProjectItself))
                .ReturnsAsync(repositoryResult);
            var mockArtifactRepository = new Mock<IArtifactPermissionsRepository>();
            var permissionResult = new Dictionary<int, RolePermissions>();
            mockArtifactRepository.Setup(r => r.GetArtifactPermissions(new List<int> { projectId}, userId, false, int.MaxValue, true))
                .ReturnsAsync(permissionResult);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var instanceController = new InstanceController(mockInstanceRepository.Object,
                mockServiceLogRepository.Object, mockArtifactRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            instanceController.Request.Properties[ServiceConstants.SessionProperty] = session;

            //Act
            await instanceController.GetProjectNavigationPathAsync(projectId);

            //Assert
        }

        [TestMethod]
        public async Task GetProjectNavigationPathAsync_Success()
        {
            //Arrange
            const int userId = 88;
            var session = new Session { UserId = userId };
            const int projectId = 99;
            const bool includeProjectItself = true;
            var repositoryResult = new List<string> { "Blueprint", "ProjectName" };
            var mockInstanceRepository = new Mock<ISqlInstanceRepository>();
            mockInstanceRepository.Setup(r => r.GetProjectNavigationPathAsync(projectId, userId, includeProjectItself))
                .ReturnsAsync(repositoryResult);
            var mockArtifactRepository = new Mock<IArtifactPermissionsRepository>();
            var permissionResult = new Dictionary<int, RolePermissions>();
            permissionResult.Add(99, RolePermissions.Read);
            mockArtifactRepository.Setup(r => r.GetArtifactPermissions(new List<int> { projectId }, userId, false, int.MaxValue, true))
                .ReturnsAsync(permissionResult);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var instanceController = new InstanceController(mockInstanceRepository.Object,
                mockServiceLogRepository.Object, mockArtifactRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            instanceController.Request.Properties[ServiceConstants.SessionProperty] = session;

            //Act
            var result = await instanceController.GetProjectNavigationPathAsync(projectId);

            //Assert
            Assert.AreSame(repositoryResult, result);
        }
    }
}
