using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AdminStore.Models;
using AdminStore.Models.DTO;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [TestClass]
    public class InstanceControllerTests
    {
        private const int UserId = 9;
        private const int FolderId = 1;
        private Mock<IInstanceRepository> _instanceRepositoryMock;
        private Mock<IServiceLogRepository> _logRepositoryMock;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepositoryMock;
        private Mock<IPrivilegesRepository> _privilegeRepositoryMock;
        private InstanceController _controller;
        private FolderDto _folder;

        [TestInitialize]
        public void Initialize()
        {
            _instanceRepositoryMock = new Mock<IInstanceRepository>();
            _logRepositoryMock = new Mock<IServiceLogRepository>();
            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            _privilegeRepositoryMock = new Mock<IPrivilegesRepository>();

            var request = new HttpRequestMessage();
            request.Properties[ServiceConstants.SessionProperty] = new Session { UserId = UserId };

            _controller = new InstanceController
            (
                _instanceRepositoryMock.Object,
                _logRepositoryMock.Object,
                _artifactPermissionsRepositoryMock.Object,
                _privilegeRepositoryMock.Object
            )
            {
                Request = request,
                Configuration = new HttpConfiguration()
            };

            _folder = new FolderDto { Name = "Folder1", ParentFolderId = 1 };
        }

        [TestMethod]
        public async Task GetInstanceFolderAsync_Success()
        {
            //Arrange
            var folderId = 99;
            var folder = new InstanceItem { Id = folderId };
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceFolderAsync(folderId, UserId))
                .ReturnsAsync(folder);

            //Act
            var result = await _controller.GetInstanceFolderAsync(folderId);

            //Assert
            Assert.AreSame(folder, result);
        }

        [TestMethod]
        public async Task GetInstanceFolderChildrenAsync_Success()
        {
            //Arrange
            var folderId = 99;
            var children = new List<InstanceItem>();
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceFolderChildrenAsync(folderId, UserId))
                .ReturnsAsync(children);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();

            //Act
            var result = await _controller.GetInstanceFolderChildrenAsync(folderId);

            //Assert
            Assert.AreSame(children, result);
        }

        [TestMethod]
        public async Task GetInstanceProjectAsync_Success()
        {
            //Arrange
            var projectId = 99;
            var project = new InstanceItem { Id = projectId };
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceProjectAsync(projectId, UserId))
                .ReturnsAsync(project);

            //Act
            var result = await _controller.GetInstanceProjectAsync(projectId);

            //Assert
            Assert.AreSame(project, result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetProjectNavigationPathAsync_InvalidPermission()
        {
            //Arrange
            const int projectId = 99;
            const bool includeProjectItself = true;
            var repositoryResult = new List<string> { "Blueprint", "ProjectName" };
            _instanceRepositoryMock
                .Setup(r => r.GetProjectNavigationPathAsync(projectId, UserId, includeProjectItself))
                .ReturnsAsync(repositoryResult);
            _artifactPermissionsRepositoryMock
                .Setup(r => r.GetArtifactPermissions(new List<int> { projectId}, UserId, false, int.MaxValue, true))
                .ReturnsAsync(new Dictionary<int, RolePermissions>());

            //Act
            await _controller.GetProjectNavigationPathAsync(projectId);
        }

        [TestMethod]
        public async Task GetProjectNavigationPathAsync_Success()
        {
            //Arrange
            const int projectId = 99;
            const bool includeProjectItself = true;
            var repositoryResult = new List<string> { "Blueprint", "ProjectName" };
            _instanceRepositoryMock
                .Setup(r => r.GetProjectNavigationPathAsync(projectId, UserId, includeProjectItself))
                .ReturnsAsync(repositoryResult);
            _artifactPermissionsRepositoryMock
                .Setup(r => r.GetArtifactPermissions(new List<int> { projectId }, UserId, false, int.MaxValue, true))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { 99, RolePermissions.Read } });

            //Act
            var result = await _controller.GetProjectNavigationPathAsync(projectId);

            //Assert
            Assert.AreSame(repositoryResult, result);
        }

        #region GetInstanceRoles

        [TestMethod]
        public async Task GetInstanceRoles_SuccessfulGettingInstanceRoles_ReturnInstanceRolesListResult()
        {
            // Arrange
            var instanceAdminRoles = new List<AdminRole>
            {
                new AdminRole
                {
                    Id = 10,
                    Description = "Can manage standard properties and artifact types.",
                    Name = "Instance Standards Manager",
                    Privileges = 197313
                }
            };
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AssignAdminRoles);
            _instanceRepositoryMock
                .Setup(repo => repo.GetInstanceRolesAsync())
                .ReturnsAsync(instanceAdminRoles);

            // Act
            var result = await _controller.GetInstanceRoles() as OkNegotiatedContentResult<IEnumerable<AdminRole>>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content, instanceAdminRoles);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetInstanceRoles_SomeError_ReturnBadRequestResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AssignAdminRoles);
            _instanceRepositoryMock
                .Setup(repo => repo.GetInstanceRolesAsync())
                .ThrowsAsync(new BadRequestException());

            // Act
            var result = await _controller.GetInstanceRoles();
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetInstanceRoles_NoViewUsersPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);
            _instanceRepositoryMock
                .Setup(repo => repo.GetInstanceRolesAsync())
                .ThrowsAsync(new BadRequestException());

            // Act
            var result = await _controller.GetInstanceRoles();
        }

        #endregion


        #region CreateFolder

        [TestMethod]
        public async Task CreateFolder_SuccessfulCreationOfFolder_ReturnCreatedFolderIdResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            _instanceRepositoryMock.Setup(repo => repo.CreateFolderAsync(It.IsAny<FolderDto>())).ReturnsAsync(FolderId);

            // Act
            var result = await _controller.CreateFolder(_folder);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task CreateFolder_NoPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _instanceRepositoryMock.Setup(repo => repo.CreateFolderAsync(It.IsAny<FolderDto>())).ReturnsAsync(FolderId);

            // Act
             await _controller.CreateFolder(_folder);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateFolder_FolderNameEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _folder.Name = string.Empty;
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            _instanceRepositoryMock.Setup(repo => repo.CreateFolderAsync(It.IsAny<FolderDto>())).ReturnsAsync(FolderId);

            // Act
            await _controller.CreateFolder(_folder);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateFolder_FolderNameOutOfLimit_ReturnBadRequestResult()
        {
            // Arrange
            _folder.Name = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis,.";
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            _instanceRepositoryMock.Setup(repo => repo.CreateFolderAsync(It.IsAny<FolderDto>())).ReturnsAsync(FolderId);

            // Act
            await _controller.CreateFolder(_folder);

            // Assert
            // Exception
        }


        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateFolder_ModelIsEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            _instanceRepositoryMock.Setup(repo => repo.CreateFolderAsync(It.IsAny<FolderDto>())).ReturnsAsync(FolderId);

            // Act
            await _controller.CreateFolder(null);

            // Assert
            // Exception
        }

        #endregion
    }
}
