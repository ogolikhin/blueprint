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
using AdminStore.Services.Instance;
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
        private const int ProjectId = 1;
        private Mock<IInstanceRepository> _instanceRepositoryMock;
        private Mock<IServiceLogRepository> _logRepositoryMock;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepositoryMock;
        private Mock<IPrivilegesRepository> _privilegeRepositoryMock;
        private Mock<IInstanceService> _instanceServiceMock;
        private InstanceController _controller;
        private FolderDto _folder;
        private ProjectDto _project;

        [TestInitialize]
        public void Initialize()
        {
            _instanceRepositoryMock = new Mock<IInstanceRepository>();
            _logRepositoryMock = new Mock<IServiceLogRepository>();
            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            _privilegeRepositoryMock = new Mock<IPrivilegesRepository>();
            _instanceServiceMock = new Mock<IInstanceService>();


            var request = new HttpRequestMessage();
            request.Properties[ServiceConstants.SessionProperty] = new Session { UserId = UserId };

            _controller = new InstanceController
            (
                _instanceRepositoryMock.Object,
                _logRepositoryMock.Object,
                _artifactPermissionsRepositoryMock.Object,
                _privilegeRepositoryMock.Object,
                _instanceServiceMock.Object
            )
            {
                Request = request,
                Configuration = new HttpConfiguration()
            };

            _folder = new FolderDto { Name = "Folder1", ParentFolderId = 2 };
            _project = new ProjectDto {Name = "Project1", Description = "Project1Description", ParentFolderId = 1};
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
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetInstanceFolderAsync_CheckViewProjectsPermissionsWithoutPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            var folderId = 99;
            var folder = new InstanceItem { Id = folderId };
            var fromAdminPortal = true;
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceFolderAsync(folderId, UserId))
                .ReturnsAsync(folder);
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewAdminRoles);

            // Act
            await _controller.GetInstanceFolderAsync(folderId, fromAdminPortal);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task GetInstanceFolderChildrenAsync_Success()
        {
            //Arrange
            var folderId = 99;
            var fromAdminPortal = false;
            var children = new List<InstanceItem>();
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceFolderChildrenAsync(folderId, UserId, fromAdminPortal))
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
            var fromAdminPortal = false;
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceProjectAsync(projectId, UserId, fromAdminPortal))
                .ReturnsAsync(project);

            //Act
            var result = await _controller.GetInstanceProjectAsync(projectId);

            //Assert
            Assert.AreSame(project, result);
        }

        [TestMethod]
        public async Task GetInstanceProjectAsyncFromAdminPortal_Success()
        {
            //Arrange
            var projectId = 99;
            var project = new InstanceItem { Id = projectId };
            var fromAdminPortal = true;
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceProjectAsync(projectId, UserId, fromAdminPortal))
                .ReturnsAsync(project);

            //Act
            var result = await _controller.GetInstanceProjectAsync(projectId, fromAdminPortal);

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

        #region SearchFolder

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task SearchFolderByName_NoPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            var response = new List<FolderDto>();
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _instanceServiceMock.Setup(repo => repo.GetFoldersByName(It.IsAny<string>())).ReturnsAsync(response);

            // Act
            await _controller.SearchFolderByName("test");

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task SearchFolderByName_PermissionaAreOkAndFolderIsExists_RenurnListOfFolders()
        {
            //arrange
            var response = new List<FolderDto>() {new FolderDto() {Id = 1} };
            var name = "folder";
            _privilegeRepositoryMock
              .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
              .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            _instanceServiceMock.Setup(repo => repo.GetFoldersByName(It.IsAny<string>())).ReturnsAsync(response);

            //act
            var result = await _controller.SearchFolderByName(name) as OkNegotiatedContentResult<IEnumerable<FolderDto>>;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(response, result.Content);

        }


        #endregion

        #region DeleteFolder

        [TestMethod]
        public async Task DeleteFolder_SuccessfulDeletionOfFolder_ReturnCountOfDeletedFolderResult()
        {
            // Arrange
            var totalDeletedItems = 1;
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.DeleteProjects);
            _instanceRepositoryMock.Setup(repo => repo.DeleteInstanceFolderAsync(It.IsAny<int>())).ReturnsAsync(totalDeletedItems);

            // Act
            var result = await _controller.DeleteInstanceFolder(1) as OkNegotiatedContentResult<DeleteResult>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(totalDeletedItems, result.Content.TotalDeleted);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task DeleteFolder_NoPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            var totalDeletedItems = 1;
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);
            _instanceRepositoryMock.Setup(repo => repo.DeleteInstanceFolderAsync(It.IsAny<int>())).ReturnsAsync(totalDeletedItems);

            // Act
            var result = await _controller.DeleteInstanceFolder(1) as OkNegotiatedContentResult<DeleteResult>;

            // Assert
            // Exception
        }

        #endregion

        #region UpdateFolder

        [TestMethod]
        public async Task UpdateFolder_AllRequirementsSatisfied_ReturnOkResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            // Act
            var result = await _controller.UpdateInstanceFolder(FolderId, _folder);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateFolder_FolderModelEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            await _controller.UpdateInstanceFolder(FolderId, null);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UpdateFolder_NoPermissions_ForbiddenResult()
        {
            //arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            //act
            await _controller.UpdateInstanceFolder(FolderId, _folder);

            //assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateFolder_FolderNameOutOfLimit_ReturnBadRequestResult()
        {
            // Arrange
            _folder.Name = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis,.";
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            await _controller.UpdateInstanceFolder(FolderId, _folder);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateFolder_EmptyFolderName_ReturnBadRequestResult()
        {
            // Arrange
            _folder.Name = string.Empty;
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            await _controller.UpdateInstanceFolder(FolderId, _folder);

            // Assert
            // Exception
        }

        #endregion

        #region UpdateProject

        [TestMethod]
        public async Task UpdateProject_AllRequirementsSatisfied_ReturnOkResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            // Act
            var result = await _controller.UpdateProject(ProjectId, _project);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateProject_ProjectModelEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            await _controller.UpdateProject(ProjectId, null);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UpdateProject_NoPermissions_ReturnForbiddenResult()
        {
            //arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            //act
            await _controller.UpdateProject(ProjectId, _project);

            //assert
            //Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateProject_PrjectNameOutOfLimit_ReturnBadRequestResult()
        {
            // Arrange
            _project.Name = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis,.";
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            await _controller.UpdateProject(ProjectId, _project);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateProject_EmptyProjectName_ReturnBadRequestResult()
        {
            // Arrange
            _project.Name = string.Empty;
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            await _controller.UpdateProject(ProjectId, _project);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateProject_ParentFolderIncorrect_ReturnBadRequestResult()
        {
            // Arrange
            _project.ParentFolderId = 0;
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            await _controller.UpdateProject(ProjectId, _project);

            // Assert
            // Exception
        }

        #endregion

        #region Project privileges

        [TestMethod]
        public async Task GetInstanceProjectPrivilegesAsync_AllParamsCorrect_ReturnPermissions()
        {
            //Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
                .ReturnsAsync(It.IsAny<ProjectAdminPrivileges>());

            //Act
            var result = await _controller.GetProjectAdminPermissions(ProjectId);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<ProjectAdminPrivileges>));
        }

        #endregion


        #region Project roles

        [TestMethod]
        public async Task GetProjectRolesAsync_Suttisfied_ReturnOkNegotiatedResult()
        {
            // Arrange
            var projectId = 100;
            var projectRoles = new List<ProjectRole>
            {
                new ProjectRole()
                {
                    Name = "Collaborator",
                    RoleId = 11
                },
                new ProjectRole()
                {
                    Name = "Author",
                    RoleId = 12
                },
                new ProjectRole()
                {
                    Name = "Viewer",
                    RoleId = 13
                },
                new ProjectRole()
                {
                    Name = "Project Administrator",
                    RoleId = 14
                },
                new ProjectRole()
                {
                    Name = "Blueprint Analytics",
                    RoleId = 15
                }
            };

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId)).ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            _privilegeRepositoryMock
                .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, projectId)).ReturnsAsync(ProjectAdminPrivileges.ViewGroupsAndRoles);
            _instanceRepositoryMock
                .Setup(repo => repo.GetProjectRolesAsync(projectId))
                .ReturnsAsync(projectRoles);

            // Act
            var result = await _controller.GetProjectRolesAsync(projectId) as OkNegotiatedContentResult<IEnumerable<ProjectRole>>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content, projectRoles);

        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetProjectRolesAsync_Failed_NoPermissions_ReturnForbiddenResult()
        {
            //arrange
            var projectId = 100;
            var projectRoles = new List<ProjectRole>
            {
                new ProjectRole()
                {
                    Name = "Collaborator",
                    RoleId = 11
                },
                new ProjectRole()
                {
                    Name = "Author",
                    RoleId = 12
                },
                new ProjectRole()
                {
                    Name = "Viewer",
                    RoleId = 13
                },
                new ProjectRole()
                {
                    Name = "Project Administrator",
                    RoleId = 14
                },
                new ProjectRole()
                {
                    Name = "Blueprint Analytics",
                    RoleId = 15
                }
            };

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _privilegeRepositoryMock
                .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, projectId)).ReturnsAsync(ProjectAdminPrivileges.None);

            _instanceRepositoryMock
                .Setup(repo => repo.GetProjectRolesAsync(projectId))
                .ReturnsAsync(projectRoles);

            await _controller.GetProjectRolesAsync(projectId);

        }

        #endregion

        #region Project Groups

        /*[TestMethod]
        public async Task GetProjectGroupsAsync_Suttisfied_ReturnOkNegotiatedResult()
        {
            // Arrange
            var projectId = 100;
            var projectGroups = new List<GroupDto>
            {
                new GroupDto()
                {
                    Name = "Group1"
                },
                new GroupDto()
                {
                    Name = "Group2"
                },
                new GroupDto()
                {
                    Name = "Group3"
                }
            };

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId)).ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            _privilegeRepositoryMock
                .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, projectId)).ReturnsAsync(ProjectAdminPrivileges.ViewGroupsAndRoles);
            _instanceRepositoryMock
                .Setup(repo => repo.GetProjectGroupsAsync(projectId))
                .ReturnsAsync(projectGroups);

            // Act
            var result = await _controller.GetProjectGroupsAsync(projectId) as OkNegotiatedContentResult<IEnumerable<GroupDto>>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content, projectGroups);

        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetProjectGroupsAsync_Failed_NoPermissions_ReturnForbiddenResult()
        {
            // Arrange
            var projectId = 100;
            var projectGroups = new List<GroupDto>
            {
                new GroupDto()
                {
                    Name = "Group1"
                },
                new GroupDto()
                {
                    Name = "Group2"
                },
                new GroupDto()
                {
                    Name = "Group3"
                }
            };

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _privilegeRepositoryMock
                .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, projectId)).ReturnsAsync(ProjectAdminPrivileges.None);

            _instanceRepositoryMock
                .Setup(repo => repo.GetProjectGroupsAsync(projectId))
                .ReturnsAsync(projectGroups);

            await _controller.GetProjectGroupsAsync(projectId);

        }*/

        #endregion
    }
}
