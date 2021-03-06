﻿using AdminStore.Models;
using AdminStore.Models.DTO;
using AdminStore.Repositories;
using AdminStore.Services.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

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
        private Pagination _pagination;
        private Sorting _sorting;
        private RoleAssignmentQueryResult<RoleAssignment> _rolesAssignmentsQueryResult;
        private int _roleAssignmentId;
        private RoleAssignmentDTO _roleAssignment;

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
                _instanceServiceMock.Object)
            {
                Request = request,
                Configuration = new HttpConfiguration()
            };

            _folder = new FolderDto { Name = "Folder1", ParentFolderId = 2 };
            _project = new ProjectDto { Name = "Project1", Description = "Project1Description", ParentFolderId = 1 };
            _pagination = new Pagination { Limit = 1, Offset = 0 };
            _sorting = new Sorting { Order = SortOrder.Asc, Sort = "Name" };

            var projectRolesAssignments = new List<RoleAssignment>
            {
               new RoleAssignment
               {
                   GroupName = "GroupName",
                   Id = 1,
                   RoleName = "RoleName"
               }
            };

            _rolesAssignmentsQueryResult = new RoleAssignmentQueryResult<RoleAssignment>
            {
                Items = projectRolesAssignments,
                Total = 1,
                ProjectName = "Project1"
            };

            _instanceRepositoryMock
                .Setup(repo => repo.GetProjectRoleAssignmentsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(_rolesAssignmentsQueryResult);

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectsAdmin);

            _roleAssignment = new RoleAssignmentDTO() { GroupId = 1, RoleId = 1 };
            _roleAssignmentId = 1;

        }

        #region GetInstanceFolder

        [TestMethod]
        public async Task GetInstanceFolderAsync_Success()
        {
            // Arrange
            var folderId = 99;
            var folder = new InstanceItem { Id = folderId };
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceFolderAsync(folderId, UserId, It.IsAny<bool>()))
                .ReturnsAsync(folder);

            // Act
            var result = await _controller.GetInstanceFolderAsync(folderId);

            // Assert
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
                .Setup(r => r.GetInstanceFolderAsync(folderId, UserId, It.IsAny<bool>()))
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
            // Arrange
            var folderId = 99;
            var fromAdminPortal = false;
            var children = new List<InstanceItem>();
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceFolderChildrenAsync(folderId, UserId, fromAdminPortal))
                .ReturnsAsync(children);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();

            // Act
            var result = await _controller.GetInstanceFolderChildrenAsync(folderId);

            // Assert
            Assert.AreSame(children, result);
        }

        #endregion

        #region GetInstanceProject

        [TestMethod]
        public async Task GetInstanceProjectAsync_Success()
        {
            // Arrange
            var projectId = 99;
            var project = new InstanceItem { Id = projectId };
            var fromAdminPortal = false;
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceProjectAsync(projectId, UserId, fromAdminPortal))
                .ReturnsAsync(project);

            // Act
            var result = await _controller.GetInstanceProjectAsync(projectId);

            // Assert
            Assert.AreSame(project, result);
        }

        [TestMethod]
        public async Task GetInstanceProjectAsyncFromAdminPortal_Success()
        {
            // Arrange
            var projectId = 99;
            var project = new InstanceItem { Id = projectId };
            var fromAdminPortal = true;
            _instanceRepositoryMock
                .Setup(r => r.GetInstanceProjectAsync(projectId, UserId, fromAdminPortal))
                .ReturnsAsync(project);

            // Act
            var result = await _controller.GetInstanceProjectAsync(projectId, fromAdminPortal);

            // Assert
            Assert.AreSame(project, result);
        }

        #endregion

        #region GetProjectNavigationPath

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetProjectNavigationPathAsync_InvalidPermission()
        {
            // Arrange
            const int projectId = 99;
            const bool includeProjectItself = true;
            var repositoryResult = new List<string> { "Blueprint", "ProjectName" };
            _instanceRepositoryMock
                .Setup(r => r.GetProjectNavigationPathAsync(projectId, UserId, includeProjectItself))
                .ReturnsAsync(repositoryResult);
            _artifactPermissionsRepositoryMock
                .Setup(r => r.GetArtifactPermissions(new List<int> { projectId }, UserId, false, int.MaxValue, true, null))
                .ReturnsAsync(new Dictionary<int, RolePermissions>());

            // Act
            await _controller.GetProjectNavigationPathAsync(projectId);
        }

        [TestMethod]
        public async Task GetProjectNavigationPathAsync_Success()
        {
            // Arrange
            const int projectId = 99;
            const bool includeProjectItself = true;
            var repositoryResult = new List<string> { "Blueprint", "ProjectName" };
            _instanceRepositoryMock
                .Setup(r => r.GetProjectNavigationPathAsync(projectId, UserId, includeProjectItself))
                .ReturnsAsync(repositoryResult);
            _artifactPermissionsRepositoryMock
                .Setup(r => r.GetArtifactPermissions(new List<int> { projectId }, UserId, false, int.MaxValue, true, null))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { 99, RolePermissions.Read } });

            // Act
            var result = await _controller.GetProjectNavigationPathAsync(projectId);

            // Assert
            Assert.AreSame(repositoryResult, result);
        }

        #endregion

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

        [TestMethod]
        public async Task CreateFolder_LocationNotSpecified_ThrowsBadRequestException()
        {
            // Arrange
            var folderToCreate = new FolderDto { Name = "New Folder 1", Path = "Blueprint" };
            _privilegeRepositoryMock
                .Setup(m => m.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            try
            {
                await _controller.CreateFolder(folderToCreate);
            }
            catch (BadRequestException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.BadRequest, ex.ErrorCode);
                Assert.AreEqual(ErrorMessages.LocationIsRequired, ex.Message);
                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }

        #endregion

        #region SearchFolder

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task SearchFolderByName_NoPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            var response = new List<InstanceItem>();

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _instanceServiceMock
                .Setup(repo => repo.GetFoldersByName(It.IsAny<string>()))
                .ReturnsAsync(response);

            // Act
            await _controller.SearchFolderByName("test");

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task SearchFolderByName_PermissionaAreOkAndFolderIsExists_RenurnListOfFolders()
        {
            // arrange
            var response = new List<InstanceItem> { new InstanceItem { Id = 1 } };
            var name = "folder";

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);
            _instanceServiceMock
                .Setup(repo => repo.GetFoldersByName(It.IsAny<string>()))
                .ReturnsAsync(response);

            // act
            var result = await _controller.SearchFolderByName(name) as OkNegotiatedContentResult<IEnumerable<InstanceItem>>;

            // assert
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
            // arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            // act
            await _controller.UpdateInstanceFolder(FolderId, _folder);

            // assert
            // Exception
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

        [TestMethod]
        public async Task UpdateInstanceFolder_LocationNotSpecified_ThrowsBadRequestException()
        {
            // Arrange
            var folderId = 1;
            var updatedFolder = new FolderDto { Name = "New Folder 1", Path = "Blueprint" };
            _privilegeRepositoryMock
                .Setup(m => m.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            try
            {
                await _controller.UpdateInstanceFolder(folderId, updatedFolder);
            }
            catch (BadRequestException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.BadRequest, ex.ErrorCode);
                Assert.AreEqual(ErrorMessages.LocationIsRequired, ex.Message);
                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }

        [TestMethod]
        public async Task UpdateInstanceFolder_LocationIsFolderItself_ThrowsConflictException()
        {
            // Arrange
            var folderId = 1;
            var updatedFolder = new FolderDto { Name = "New Folder 1", ParentFolderId = folderId, Path = "Blueprint/New Folder 1" };
            _privilegeRepositoryMock
                .Setup(m => m.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageProjects);

            // Act
            try
            {
                await _controller.UpdateInstanceFolder(folderId, updatedFolder);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.Conflict, ex.ErrorCode);
                Assert.AreEqual(ErrorMessages.FolderReferenceToItself, ex.Message);
                return;
            }

            Assert.Fail("No ConflictException was thrown.");
        }

        #endregion

        #region DeleteProject

        [TestMethod]
        public async Task DeleteProject_SuccessfulDeletionOfProject_ReturnNoContentResponse()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.DeleteProjects);

            // Act
            var result = await _controller.DeleteProject(ProjectId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task DeleteProject_NoPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            // Act
            await _controller.DeleteProject(ProjectId);

            // Assert
            // Exception
        }

        #endregion

        #region Project privileges

        [TestMethod]
        public async Task GetInstanceProjectPrivilegesAsync_AllParamsCorrect_ReturnPermissions()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
                .ReturnsAsync(It.IsAny<ProjectAdminPrivileges>());

            // Act
            var result = await _controller.GetProjectAdminPermissions(ProjectId);

            // Assert
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
                new ProjectRole
                {
                    Name = "Collaborator",
                    RoleId = 11
                },
                new ProjectRole
                {
                    Name = "Author",
                    RoleId = 12
                },
                new ProjectRole
                {
                    Name = "Viewer",
                    RoleId = 13
                },
                new ProjectRole
                {
                    Name = "Project Administrator",
                    RoleId = 14
                },
                new ProjectRole
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
            // arrange
            var projectId = 100;
            var projectRoles = new List<ProjectRole>
            {
                new ProjectRole
                {
                    Name = "Collaborator",
                    RoleId = 11
                },
                new ProjectRole
                {
                    Name = "Author",
                    RoleId = 12
                },
                new ProjectRole
                {
                    Name = "Viewer",
                    RoleId = 13
                },
                new ProjectRole
                {
                    Name = "Project Administrator",
                    RoleId = 14
                },
                new ProjectRole
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

        #region GetProjectRoleAssignments

        [TestMethod]
        public async Task GetProjectRoleAssignments_SuccessfulGettingProjectRoleAssignments_ProjectRoleAssignmentsOkResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectsAdmin);

            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ViewGroupsAndRoles);

            // Act
            var result = await _controller.GetProjectRoleAssignments(ProjectId, _pagination, _sorting) as OkNegotiatedContentResult<RoleAssignmentQueryResult<RoleAssignment>>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content, _rolesAssignmentsQueryResult);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetProjectRoleAssignments_IncorrectModel_ReturnBadRequestResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectsAdmin);

            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ViewGroupsAndRoles);

            // Act
            var result = await _controller.GetProjectRoleAssignments(ProjectId, null, _sorting) as OkNegotiatedContentResult<RoleAssignmentQueryResult<RoleAssignment>>;

            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetProjectRoleAssignments_IncorrectUserPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ViewAlmIntegration);

            // Act
            var result = await _controller.GetProjectRoleAssignments(ProjectId, _pagination, _sorting) as OkNegotiatedContentResult<RoleAssignmentQueryResult<RoleAssignment>>;

            // Exception
        }

        #endregion

        #region DeleteRoleAssignment

        [TestMethod]
        public async Task DeleteRoleAssignment_SuccessfulDeletionOfRoleAssignment_ReturnCountOfDeletedRoleAssignmentResult()
        {
            // Arrange
            var totalDeletedItems = 1;
            var scope = new OperationScope() { SelectAll = false, Ids = new List<int>() { 2, 3 } };

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectsAdmin);

            _privilegeRepositoryMock
                .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
                .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            _instanceRepositoryMock.Setup(repo => repo.DeleteRoleAssignmentsAsync(It.IsAny<int>(), It.Is<OperationScope>(a => a.Ids != null), It.IsAny<string>())).ReturnsAsync(totalDeletedItems);

            // Act
            var result = await _controller.DeleteRoleAssignment(ProjectId, scope, string.Empty) as OkNegotiatedContentResult<DeleteResult>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(totalDeletedItems, result.Content.TotalDeleted);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task DeleteRoleAssignment_UserdDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            var scope = new OperationScope() { SelectAll = false, Ids = new List<int>() { 2, 3 } };
            _privilegeRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);

            // act
            await _controller.DeleteRoleAssignment(ProjectId, scope);

            // assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task DeleteRoleAssignment_InvalidParameteres_BadRequest()
        {
            // arrange

            // act
            await _controller.DeleteRoleAssignment(ProjectId, null);

            // assert
            // Exception
        }

        #endregion

        #region HasProjectExternalLocks

        [TestMethod]
        public async Task HasProjectExternalLocks_AllParamsCorrect_ReturnBooleanValue()
        {
            // Arrange
            var hasProjectExternalLocks = 1;

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.DeleteProjects);
            _instanceRepositoryMock
                .Setup(repo => repo.HasProjectExternalLocksAsync(UserId, ProjectId))
                .ReturnsAsync(hasProjectExternalLocks);

            // Act
            var result = await _controller.HasProjectExternalLocks(ProjectId) as OkNegotiatedContentResult<int>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(hasProjectExternalLocks, result.Content);
        }

        #endregion

        #region CreateRoleAssignment

        [TestMethod]
        public async Task CreateRoleAssignment_SuccessfulCreationOfAssignment_ReturnCreatedRoleAssignmentIdResult()
        {
            RoleAssignmentDTO roleAssignment = new RoleAssignmentDTO() { GroupId = 1, RoleId = 1 };
            int roleAssignmentId = 1;

            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectsAdmin);

            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            _instanceRepositoryMock.Setup(repo => repo.CreateRoleAssignmentAsync(ProjectId, roleAssignment))
                                   .ReturnsAsync(roleAssignmentId);

            // Act
            var result = await _controller.CreateRoleAssignment(ProjectId, roleAssignment);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task CreateRoleAssignment_NoPermissions_ReturnForbiddenErrorResult()
        {
            RoleAssignmentDTO roleAssignment = new RoleAssignmentDTO() { GroupId = 1, RoleId = 1 };
            int roleAssignmentId = 1;

            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ViewGroupsAndRoles);

            _instanceRepositoryMock.Setup(repo => repo.CreateRoleAssignmentAsync(ProjectId, roleAssignment))
                                   .ReturnsAsync(roleAssignmentId);

            // Act
            var result = await _controller.CreateRoleAssignment(ProjectId, roleAssignment);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateRoleAssignment_RoleAssignmentInvalid_ReturnBadRequestResult()
        {
            RoleAssignmentDTO roleAssignment = null;
            int roleAssignmentId = 1;

            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectsAdmin);

            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            _instanceRepositoryMock.Setup(repo => repo.CreateRoleAssignmentAsync(ProjectId, roleAssignment))
                                   .ReturnsAsync(roleAssignmentId);

            // Act
            var result = await _controller.CreateRoleAssignment(ProjectId, roleAssignment);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateRoleAssignment_RoleIdInvalid_ReturnBadRequestResult()
        {
            RoleAssignmentDTO roleAssignment = new RoleAssignmentDTO() { GroupId = 1, RoleId = 0 };
            int roleAssignmentId = 1;

            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectsAdmin);

            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            _instanceRepositoryMock.Setup(repo => repo.CreateRoleAssignmentAsync(ProjectId, roleAssignment))
                                   .ReturnsAsync(roleAssignmentId);

            // Act
            var result = await _controller.CreateRoleAssignment(ProjectId, roleAssignment);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateRoleAssignment_GroupIdInvalid_ReturnBadRequestResult()
        {
            RoleAssignmentDTO roleAssignment = new RoleAssignmentDTO() { GroupId = 0, RoleId = 1 };
            int roleAssignmentId = 1;

            // Arrange
            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectsAdmin);

            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            _instanceRepositoryMock.Setup(repo => repo.CreateRoleAssignmentAsync(ProjectId, roleAssignment))
                                   .ReturnsAsync(roleAssignmentId);

            // Act
            var result = await _controller.CreateRoleAssignment(ProjectId, roleAssignment);
        }

        #endregion

        #region SearchProjectFolder

        [TestMethod]
        public async Task SearchProjectFolder_AllParametersAreValid_ReturnSuccessResult()
        {
            // arrange
            var request = new Pagination() { Limit = 10, Offset = 0 };
            var queryResult = new QueryResult<ProjectFolderSearchDto>() { Items = new List<ProjectFolderSearchDto>() { new ProjectFolderSearchDto() }, Total = 1 };
            _instanceRepositoryMock.Setup(
                repo =>
                    repo.GetProjectsAndFolders(It.IsAny<int>(),
                        It.Is<TabularData>(
                            t =>
                                t.Pagination != null && t.Pagination.Offset.HasValue && t.Pagination.Offset >= 0 &&
                                t.Pagination.Limit > 0), It.IsAny<Func<Sorting, string>>())).ReturnsAsync(queryResult);

            // act
            var result =
                await _controller.SearchProjectFolder(request) as
                    OkNegotiatedContentResult<QueryResult<ProjectFolderSearchDto>>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Content.Total);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task SearchProjectFolder_InvalidPagination_ReturnBadRequestResponse()
        {
            // arrange

            // act
            await _controller.SearchProjectFolder(new Pagination());

            // assert
        }
        #endregion

        #region UpdateRoleAssignment

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateRoleAssignment_InvalidRoleAssignment_ThrowsBadRequestException()
        {
            _roleAssignment = null;

            // Arrange
            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            // Act
            var result = await _controller.UpdateRoleAssignment(ProjectId, _roleAssignmentId, _roleAssignment);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateRoleAssignment_InvalidRoleId_ThrowsBadRequestException()
        {
            _roleAssignment.RoleId = 0;

            // Arrange
            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            // Act
            var result = await _controller.UpdateRoleAssignment(ProjectId, _roleAssignmentId, _roleAssignment);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateRoleAssignment_InvalidGroupId_ThrowsBadRequestException()
        {
            _roleAssignment.GroupId = 0;

            // Arrange
            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            // Act
            var result = await _controller.UpdateRoleAssignment(ProjectId, _roleAssignmentId, _roleAssignment);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateRoleAssignment_InvalidRoleAssignmentId_ThrowsBadRequestException()
        {
            _roleAssignmentId = 0;

            // Arrange
            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            // Act
            var result = await _controller.UpdateRoleAssignment(ProjectId, _roleAssignmentId, _roleAssignment);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UpdateRoleAssignment_InsufficientPermissions_ThrowsBadRequestException()
        {
            // Arrange
            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ViewGroupsAndRoles);

            _privilegeRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);

            // Act
            await _controller.UpdateRoleAssignment(ProjectId, _roleAssignmentId, _roleAssignment);

            // Assert
        }

        [TestMethod]
        public async Task UpdateRoleAssignment_SuccessfulUpdateOfAssignment_ReturnNoneContentSuccessResult()
        {
            // Arrange
            _privilegeRepositoryMock
               .Setup(r => r.GetProjectAdminPermissionsAsync(UserId, ProjectId))
               .ReturnsAsync(ProjectAdminPrivileges.ManageGroupsAndRoles);

            // Act
            var result = await _controller.UpdateRoleAssignment(ProjectId, _roleAssignmentId, _roleAssignment);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        #endregion
    }
}
