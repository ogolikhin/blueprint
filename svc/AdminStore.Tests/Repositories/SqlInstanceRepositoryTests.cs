using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlInstanceRepositoryTests
    {

        private SqlConnectionWrapperMock _connection;
        private SqlInstanceRepository _instanceRepository;
        private const int ProjectId = 1;
        private const int ParentFolderId = 88;
        private const int UserId = 10;
        private IEnumerable<RoleAssignment> _projectRolesAssignments;
        private TabularData _tabularData;
        private InstanceItem[] _instanceItems;
        private RoleAssignmentDTO roleAssignment;
        private int errorCode;
        private int roleAssignmentId;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqlConnectionWrapperMock();
            _instanceRepository = new SqlInstanceRepository(_connection.Object);

            _projectRolesAssignments = new List<RoleAssignment>
            {
                new RoleAssignment {Id = 1, RoleName = "Role1", GroupName = "Group1"}
            };

            _tabularData = new TabularData
            {
                Pagination = new Pagination { Limit = 10, Offset = 0 },
                Sorting = new Sorting { Order = SortOrder.Asc, Sort = "groupName" }
            };

            _instanceItems = new[] { new InstanceItem { Id = ProjectId, Name = "My Project", IsAccesible = true } };

            roleAssignment = new RoleAssignmentDTO() { GroupId = 1, RoleId = 1 };
            errorCode = 0;
            roleAssignmentId = 1;
        }

        #region GetInstanceFolderAsync

        [TestMethod]
        public async Task GetInstanceFolderAsync_Found()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 99;
            var userId = 9;
            InstanceItem[] result = { new InstanceItem { Id = folderId, Name = "Blueprint", ParentFolderId = 88 } };
            cxn.SetupQueryAsync("GetInstanceFolderById", new Dictionary<string, object> { { "folderId", folderId } }, result);

            // Act
            var folder = await repository.GetInstanceFolderAsync(folderId, userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), folder);
            Assert.AreEqual(folder.Type, InstanceItemTypeEnum.Folder);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetInstanceFolderAsync_NotFound()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 99;
            var userId = 9;
            InstanceItem[] result = null;
            cxn.SetupQueryAsync("GetInstanceFolderById", new Dictionary<string, object> { { "folderId", folderId } }, result);

            // Act
            await repository.GetInstanceFolderAsync(folderId, userId);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetInstanceFolderAsync_InvalidFolderId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);

            // Act
            await repository.GetInstanceFolderAsync(0, 9);

            // Assert
        }

        #endregion

        #region GetInstanceFolderChildrenAsync

        [TestMethod]
        public async Task GetInstanceFolderChildrenAsync_Found()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            int folderId = 99;
            int userId = 10;
            InstanceItem[] result =
            {
                new InstanceItem { Id = 10, Name = "z", Type = InstanceItemTypeEnum.Project },
                new InstanceItem { Id = 11, Name = "y", Type = InstanceItemTypeEnum.Project },
                new InstanceItem { Id = 12, Name = "b", Type = InstanceItemTypeEnum.Folder },
                new InstanceItem { Id = 13, Name = "a", Type = InstanceItemTypeEnum.Folder },
            };
            cxn.SetupQueryAsync("GetInstanceFolderChildren", new Dictionary<string, object> { { "folderId", folderId }, { "userId", userId } }, result);

            // Act
            var children = await repository.GetInstanceFolderChildrenAsync(folderId, userId);

            // Assert
            cxn.Verify();
            var expected = result.OrderBy(i => i.Type).ThenBy(i => i.Name).Select(i => i.Id).ToList();
            var actual = children.Select(i => i.Id).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetInstanceFolderChildrenAsync_InvalidFolderId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);

            // Act
            var folder = await repository.GetInstanceFolderChildrenAsync(0, 10);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetInstanceFolderChildrenAsync_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);

            // Act
            var folder = await repository.GetInstanceFolderChildrenAsync(10, 0);

            // Assert
        }

        #endregion

        #region GetInstanceProjectAsync

        [TestMethod]
        public async Task GetInstanceProjectAsync_Found()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            int projectId = 99;
            int userId = 10;
            InstanceItem[] result = { new InstanceItem { Id = projectId, Name = "My Project", ParentFolderId = 88, IsAccesible = true } };
            cxn.SetupQueryAsync("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var project = await repository.GetInstanceProjectAsync(projectId, userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), project);
        }

        [TestMethod]
        public async Task GetInstanceProjectAsyncFromAdminPortal_Found()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            int projectId = 99;
            int userId = 10;
            InstanceItem[] result = { new InstanceItem { Id = projectId, Name = "My Project", ParentFolderId = 88, IsAccesible = true } };
            cxn.SetupQueryAsync("GetProjectDetails", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var project = await repository.GetInstanceProjectAsync(projectId, userId, fromAdminPortal: true);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), project);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetInstanceProjectAsync_InvalidProjectId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);

            // Act
            var folder = await repository.GetInstanceProjectAsync(0, 10);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetInstanceProjectAsyncFromAdminPortal_InvalidProjectId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);

            // Act
            var folder = await repository.GetInstanceProjectAsync(0, 10, fromAdminPortal: true);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetInstanceProjectAsync_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);

            // Act
            var folder = await repository.GetInstanceProjectAsync(10, 0);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetInstanceProjectAsyncFromAdminPortal_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);

            // Act
            var folder = await repository.GetInstanceProjectAsync(10, 0, fromAdminPortal: true);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetInstanceProjectAsync_NotFound()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            int projectId = 99;
            int userId = 10;
            InstanceItem[] result = { };
            cxn.SetupQueryAsync("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var project = await repository.GetInstanceProjectAsync(projectId, userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), project);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetInstanceProjectAsyncFromAdminPortal_NotFound()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            int projectId = 99;
            int userId = 10;
            InstanceItem[] result = { };
            cxn.SetupQueryAsync("GetProjectDetails", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var project = await repository.GetInstanceProjectAsync(projectId, userId, fromAdminPortal: true);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), project);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetInstanceProjectAsync_Unauthorized()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            int projectId = 99;
            int userId = 10;
            InstanceItem[] result = { new InstanceItem { Id = projectId, Name = "My Project", ParentFolderId = 88, IsAccesible = false } };
            cxn.SetupQueryAsync("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var project = await repository.GetInstanceProjectAsync(projectId, userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), project);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetInstanceProjectAsyncFromAdminPortal_Unauthorized()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            int projectId = 99;
            int userId = 10;
            InstanceItem[] result = { new InstanceItem { Id = projectId, Name = "My Project", ParentFolderId = 88, IsAccesible = false } };
            cxn.SetupQueryAsync("GetProjectDetails", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var project = await repository.GetInstanceProjectAsync(projectId, userId, fromAdminPortal: true);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), project);
        }

        #endregion

        #region GetProjectNavigationPathAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectNavigationPathAsync_InvalidProjectId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);

            // Act
            await repository.GetProjectNavigationPathAsync(0, 10);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectNavigationPathAsync_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);

            // Act
            await repository.GetProjectNavigationPathAsync(10, 0);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectNavigationPathAsync_NotFound()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            const int projectId = 99;
            const int userId = 10;
            cxn.SetupQueryAsync("GetProjectNavigationPath", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, new List<ArtifactsNavigationPath>());

            // Act
            await repository.GetProjectNavigationPathAsync(projectId, userId);

            // Assert
        }

        [TestMethod]
        public async Task GetProjectNavigationPathAsync_includeProjectItself_Found()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            const int projectId = 99;
            const int userId = 10;
            var result = GetProjectNavigationPathSample();
            cxn.SetupQueryAsync("GetProjectNavigationPath", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var navigationPaths = await repository.GetProjectNavigationPathAsync(projectId, userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(navigationPaths.Count, 2);
            Assert.AreEqual(result.First().Name, navigationPaths.Last());
        }

        [TestMethod]
        public async Task GetProjectNavigationPathAsync_WithoutProjectItself_Found()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            const int projectId = 99;
            const int userId = 10;
            var result = GetProjectNavigationPathSample();
            cxn.SetupQueryAsync("GetProjectNavigationPath", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var navigationPaths = await repository.GetProjectNavigationPathAsync(projectId, userId, false);

            // Assert
            cxn.Verify();
            Assert.AreEqual(navigationPaths.Count, 1);
            Assert.AreEqual(result.Last().Name, navigationPaths.Last());
        }

        private List<ArtifactsNavigationPath> GetProjectNavigationPathSample()
        {
            return new List<ArtifactsNavigationPath>
            {
                new ArtifactsNavigationPath { Level = 0, ArtifactId = 1, Name = "ProjectName"},
                new ArtifactsNavigationPath { Level = 1, ArtifactId = 2, Name = "Blueprint"}
            };
        }

        #endregion

        #region GetInstanceRolesAsync

        [TestMethod]
        public async Task GetInstanceRolesAsync_SuccessfulGettingInstanceRoles_ReturnInstanceRolesList()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var adminRoles = new List<AdminRole>
            {
                new AdminRole
                {
                    Id = 10,
                    Description = "Can manage standard properties and artifact types.",
                    Name = "Instance Standards Manager",
                    Privileges = 197313
                }
            };

            cxn.SetupQueryAsync("GetInstanceAdminRoles", null, adminRoles);

            // Act
            var result = await repository.GetInstanceRolesAsync();

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, adminRoles);
        }

        #endregion GetInstanceRolesAsync

        #region CreateFolderAsync

        [TestMethod]
        public async Task CreateFolderAsync_SuccessfulCreationOfFolder_ReturnCreatedFolderId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 1;
            var folder = new FolderDto { Name = "Folder1", ParentFolderId = 1 };

            cxn.SetupExecuteScalarAsync("CreateFolder", It.IsAny<Dictionary<string, object>>(), folderId, new Dictionary<string, object> { { "ErrorCode", 0 } });

            // Act
            var result = await repository.CreateFolderAsync(folder);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, folderId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task CreateFolderAsync_FolderWithSuchNameExists_ReturnConflictError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 1;
            var folder = new FolderDto { Name = "Folder1", ParentFolderId = 1 };

            cxn.SetupExecuteScalarAsync("CreateFolder", It.IsAny<Dictionary<string, object>>(), folderId, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.FolderWithSuchNameExistsInParentFolder } });

            // Act
            await repository.CreateFolderAsync(folder);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task CreateFolderAsync_ParentFolderNotExists_ReturnResourceNotFoundError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 1;
            var folder = new FolderDto { Name = "Folder1", ParentFolderId = 1 };

            cxn.SetupExecuteScalarAsync("CreateFolder", It.IsAny<Dictionary<string, object>>(), folderId, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.ParentFolderNotExists } });

            // Act
            await repository.CreateFolderAsync(folder);

            // Assert
            // Exception
        }

        #endregion CreateFolderAsync

        #region SearchFolder

        [TestMethod]
        public async Task GetFoldersByName_WeHaveFoldersWithSimilarName_ReturnFolders()
        {
            // arrange
            var name = "folderName";
            var result = new List<InstanceItem> { new InstanceItem { Id = 1 } };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            cxn.SetupQueryAsync("GetFoldersByName", new Dictionary<string, object> { { "name", name } }, result);

            // act
            var response = await repository.GetFoldersByName(name);

            // assert
            cxn.Verify();
            Assert.AreEqual(response.ToList().Count, result.Count);
            Assert.AreEqual(result.Last().Id, result.Last().Id);

        }

        [TestMethod]
        public async Task GetFoldersByName_ThereIsNoFoldersWithSuchAName_ReturnEmptyResult()
        {
            // arrange
            var name = "someName";
            var result = new List<InstanceItem>();
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            cxn.SetupQueryAsync("GetFoldersByName", new Dictionary<string, object> { { "name", name } }, result);

            // act
            var response = await repository.GetFoldersByName(name);

            // assert
            cxn.Verify();
            Assert.AreEqual(response.ToList().Count, result.Count);
        }

        #endregion

        #region DeleteFolderAsync

        [TestMethod]
        public async Task DeleteFolderAsync_SuccessfulDeletionOfFolder_ReturnCountOfDeletedFolder()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var deletedFolderCount = 1;

            cxn.SetupExecuteScalarAsync("DeleteFolder", It.IsAny<Dictionary<string, object>>(), deletedFolderCount, new Dictionary<string, object> { { "ErrorCode", 0 } });

            // Act
            var result = await repository.DeleteInstanceFolderAsync(instanceFolderId: 1);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, deletedFolderCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task DeleteFolderAsync_FolderContainsChildrenItems_ReturnConflictError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var deletedFolderCount = 0;

            cxn.SetupExecuteScalarAsync("DeleteFolder", It.IsAny<Dictionary<string, object>>(), deletedFolderCount, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.InstanceFolderContainsChildrenItems } });

            // Act
            await repository.DeleteInstanceFolderAsync(instanceFolderId: 1);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task DeleteFolderAsync_FolderNotExist_ReturnResourceNotFoundError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var deletedFolderCount = 0;

            cxn.SetupExecuteScalarAsync("DeleteFolder", It.IsAny<Dictionary<string, object>>(), deletedFolderCount, new Dictionary<string, object> { { "ErrorCode", 0 } });

            // Act
            await repository.DeleteInstanceFolderAsync(instanceFolderId: 1);

            // Assert
            // Exception
        }

        #endregion

        #region UpdateFolder

        [TestMethod]
        public async Task UpdateFolderAsync_SuccessfulUpdationOfFolder_ReturnNoError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 1;
            var folder = new FolderDto { Name = "Folder1", ParentFolderId = 1 };

            cxn.SetupExecuteScalarAsync("UpdateFolder", 
                                        It.IsAny<Dictionary<string, object>>(), 
                                        folderId, 
                                        new Dictionary<string, object> { { "ErrorCode", 0 } });

            // Act
            await repository.UpdateFolderAsync(folderId, folder);

            // Assert
            cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UpdateFolderAsync_FolderWithSuchNameExistsInParentFolder_ReturnConflictError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 1;
            var folder = new FolderDto { Name = "Folder1", ParentFolderId = 1 };

            cxn.SetupExecuteScalarAsync("UpdateFolder", It.IsAny<Dictionary<string, object>>(), folderId, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.FolderWithSuchNameExistsInParentFolder } });

            // Act
            await repository.UpdateFolderAsync(folderId, folder);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UpdateFolderAsync_ParentFolderIdReferenceToDescendantItem_ReturnConflictError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 1;
            var folder = new FolderDto { Name = "Folder1", ParentFolderId = 1 };

            cxn.SetupExecuteScalarAsync("UpdateFolder", It.IsAny<Dictionary<string, object>>(), folderId, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.ParentFolderIdReferenceToDescendantItem } });

            // Act
            await repository.UpdateFolderAsync(folderId, folder);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateFolderAsync_ParentFolderNotExists_ReturnResourceNotFoundError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 1;
            var folder = new FolderDto { Name = "Folder1", ParentFolderId = 1 };

            cxn.SetupExecuteScalarAsync("UpdateFolder", It.IsAny<Dictionary<string, object>>(), folderId, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.ParentFolderNotExists } });

            // Act
            await repository.UpdateFolderAsync(folderId, folder);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateFolderAsync_FolderWithCurrentIdNotExist_ReturnResourceNotFoundError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 1;
            var folder = new FolderDto { Name = "Folder1", ParentFolderId = 1 };

            cxn.SetupExecuteScalarAsync("UpdateFolder", It.IsAny<Dictionary<string, object>>(), folderId, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.FolderWithCurrentIdNotExist } });

            // Act
            await repository.UpdateFolderAsync(folderId, folder);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateFolderAsync_EditRootFolderIsForbidden_ReturnBadRequestError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 1;
            var folder = new FolderDto { Name = "Folder1" };

            cxn.SetupExecuteScalarAsync("UpdateFolder", It.IsAny<Dictionary<string, object>>(), folderId, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.EditRootFolderIsForbidden } });

            // Act
            await repository.UpdateFolderAsync(folderId, folder);

            // Assert
            // Exception
        }

        #endregion

        #region DeleteProject

        [TestMethod]
        public async Task DeleteProject_AllParametersCorrect_SuccessfulDeletionOfProject()
        {
            // Arrange            
            _instanceItems.First().ParentFolderId = ParentFolderId;
            _connection.SetupQueryAsync("GetProjectDetails", It.IsAny<Dictionary<string, object>>(), _instanceItems);

            // Act
            await _instanceRepository.DeleteProject(UserId, ProjectId);

            // Assert
            _connection.Verify();
        }

        [TestMethod]
        public async Task DeleteProject_ProjectStatusImporting_SuccessfulPurgeOfProject()
        {
            // Arrange
            int? errorCode = 0;
            _instanceItems.First().ParentFolderId = ParentFolderId;
            _instanceItems.First().ProjectStatus = "I";
            _connection.SetupQueryAsync("GetProjectDetails", It.IsAny<Dictionary<string, object>>(), _instanceItems);
            _connection.SetupExecuteScalarAsync("PurgeProject",
                It.IsAny<Dictionary<string, object>>(), errorCode.Value,
                new Dictionary<string, object> { { "result", errorCode } });

            // Act
            await _instanceRepository.DeleteProject(UserId, ProjectId);

            // Assert
            _connection.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task DeleteProject_ProjectWasDeletedByAnotherUser_ReturnResourceNotFoundException()
        {
            // Arrange

            _connection.SetupQueryAsync("GetProjectDetails", It.IsAny<Dictionary<string, object>>(), _instanceItems);

            // Act
            await _instanceRepository.DeleteProject(UserId, ProjectId);

            // Assert
        }

        [TestMethod]
        public async Task DeleteProject_UnhandledStatusOfProject_ReturnException()
        {
            // Arrange

            _instanceItems.First().ParentFolderId = ParentFolderId;
            _instanceItems.First().ProjectStatus = string.Empty;
            _connection.SetupQueryAsync("GetProjectDetails", It.IsAny<Dictionary<string, object>>(), _instanceItems);

            // Act            
            try
            {
                await _instanceRepository.DeleteProject(UserId, ProjectId);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.UnhandledStatusOfProject, _instanceItems.First().ProjectStatus), ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task DeleteProject_ProjectStatusImporting_ConflictExceptionOnPurgeOfProject()
        {
            // Arrange
            int? errorCode = -2;
            _instanceItems.First().ParentFolderId = ParentFolderId;
            _instanceItems.First().ProjectStatus = "I";
            _connection.SetupQueryAsync("GetProjectDetails", It.IsAny<Dictionary<string, object>>(), _instanceItems);
            _connection.SetupExecuteScalarAsync("PurgeProject",
                It.IsAny<Dictionary<string, object>>(), errorCode.Value,
                new Dictionary<string, object> { { "result", errorCode } });

            // Act
            await _instanceRepository.DeleteProject(UserId, ProjectId);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task DeleteProject_ProjectStatusImporting_ResourceNotFoundExceptionOnPurgeOfProject()
        {
            // Arrange
            int? errorCode = -1;
            _instanceItems.First().ParentFolderId = ParentFolderId;
            _instanceItems.First().ProjectStatus = "I";
            _connection.SetupQueryAsync("GetProjectDetails", It.IsAny<Dictionary<string, object>>(), _instanceItems);
            _connection.SetupExecuteScalarAsync("PurgeProject",
                It.IsAny<Dictionary<string, object>>(), errorCode.Value,
                new Dictionary<string, object> { { "result", errorCode } });

            // Act
            await _instanceRepository.DeleteProject(UserId, ProjectId);

            // Assert
        }

        [TestMethod]
        public async Task DeleteProject_ProjectStatusImporting_DefaultExceptionOnPurgeOfProject()
        {
            // Arrange
            int? errorCode = -3;
            _instanceItems.First().ParentFolderId = ParentFolderId;
            _instanceItems.First().ProjectStatus = "I";
            _connection.SetupQueryAsync("GetProjectDetails", It.IsAny<Dictionary<string, object>>(), _instanceItems);
            _connection.SetupExecuteScalarAsync("PurgeProject",
                It.IsAny<Dictionary<string, object>>(), errorCode.Value,
                new Dictionary<string, object> { { "result", errorCode } });

            // Act
            try
            {
                await _instanceRepository.DeleteProject(UserId, ProjectId);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.AreEqual(ErrorMessages.GeneralErrorOfUpdatingProject, ex.Message);
            }
        }

        #endregion

        #region Project Roles

        [TestMethod]
        public async Task GetProjectRolesAsync_RolesFound_NoErrors()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var projectId = 100;
            int errorCode = 0;

            ProjectRole[] projectRoles =
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

            cxn.SetupQueryAsync("GetProjectRoles",
                                        new Dictionary<string, object> { { "projectId", projectId } },
                                        projectRoles,
                                        new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await repository.GetProjectRolesAsync(projectId);

            // Assert
            cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectRolesAsync_ProjectNotFound_NotFoundError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var projectId = 1;
            int errorCode = 50016; // there are no project with this projectId

            ProjectRole[] projectRoles = {};

            cxn.SetupQueryAsync("GetProjectRoles",
                new Dictionary<string, object>
                {
                    {
                        "projectId", projectId
                    }
                },
                projectRoles,
                new Dictionary<string, object> {{"ErrorCode", errorCode}});

            // Act
            await repository.GetProjectRolesAsync(projectId);
        }

        #endregion

        #region GetProjectRoleAssignmentsAsync

        [TestMethod]
        public async Task GetProjectRoleAssignmentsAsync_ProjectRolesAssignmentsFound_SuccessfulResult()
        {
            // Arrange
            var errorCode = 0;

            _connection.SetupQueryAsync("GetProjectRoleAssignments",
                                        It.IsAny<Dictionary<string, object>>(),
                                        _projectRolesAssignments,
                                        new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            var projectRolesAssignmentsResult = await
                _instanceRepository.GetProjectRoleAssignmentsAsync(ProjectId, _tabularData,
                    SortingHelper.SortProjectRolesAssignments);

            // Assert
            Assert.IsNotNull(projectRolesAssignmentsResult);
            Assert.AreEqual(projectRolesAssignmentsResult.Items, _projectRolesAssignments);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectRoleAssignmentsAsync_ProjectNotFound_NotFoundError()
        {
            // Arrange
            var errorCode = SqlErrorCodes.ProjectWithCurrentIdNotExist;

            _connection.SetupQueryAsync("GetProjectRoleAssignments",
                                        It.IsAny<Dictionary<string, object>>(),
                                        _projectRolesAssignments,
                                        new Dictionary<string, object> { { "ErrorCode", (int)errorCode } });

            // Act
            await
                _instanceRepository.GetProjectRoleAssignmentsAsync(ProjectId, _tabularData,
                    SortingHelper.SortProjectRolesAssignments);

            // Exception
        }
        #endregion

        #region DeleteRoleAssignmentsAsync

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task DeleteRoleAssignmentsAsync_ProjectNotFound_NotFoundError()
        {
            // Arrange
            var errorCode = SqlErrorCodes.ProjectWithCurrentIdNotExist;
            var deleteRoleAssignmentCount = 0;
            var scope = new OperationScope() { SelectAll = false, Ids = new List<int>() { 2, 3 } };

            _connection.SetupExecuteScalarAsync("DeleteProjectRoleAssigments",
                It.IsAny<Dictionary<string, object>>(),
                deleteRoleAssignmentCount,
                new Dictionary<string, object>() { { "ErrorCode", (int)errorCode } });

            // Act
            await _instanceRepository.DeleteRoleAssignmentsAsync(ProjectId, scope, null);

            // Exception
        }

        [TestMethod]
        public async Task DeleteRoleAssignmentsAsync_SuccessfulDeletionOfRoleAssignment_ReturnCountOfDeletedRoleAssignment()
        {
            // Arrange
            var deleteRoleAssignmentCount = 1;
            var scope = new OperationScope() { SelectAll = false, Ids = new List<int>() { 2, 3 } };

            _connection.SetupExecuteScalarAsync("DeleteProjectRoleAssigments",
                It.IsAny<Dictionary<string, object>>(),
                deleteRoleAssignmentCount);

            // Act
            var result = await _instanceRepository.DeleteRoleAssignmentsAsync(ProjectId, scope, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(deleteRoleAssignmentCount, result);
        }

        #endregion

        #region HasProjectExternalLocksAsync

        [TestMethod]
        public async Task HasProjectExternalLocksAsync_ExistingUserAndProject_ReturnIntValueCheckingIfProjectHasExternalLocks()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var userId = 1;
            var hasProjectExternalLocksAsync = 1;
            cxn.SetupExecuteScalarAsync("IsProjectHasForeignLocks", It.IsAny<Dictionary<string, object>>(), hasProjectExternalLocksAsync);

            // Act
            var result = await repository.HasProjectExternalLocksAsync(userId, ProjectId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, hasProjectExternalLocksAsync);
        }

        #endregion

        #region CreateRoleAssignmentAsync

        [TestMethod]
        public async Task CreateRoleAssignment_SuccessfulRoleAssignmentCreation_ReturnCreatedRoleAssignmentId()
        {
            // Arrange
            var createdRoleAssignmentId = 1;
            RoleAssignmentDTO roleAssignment = new RoleAssignmentDTO() {GroupId = 1, RoleId = 1};

            _connection.SetupExecuteScalarAsync("CreateProjectRoleAssignment",
                                        new Dictionary <string, object>
                                        {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId }
                                        }, 
                                        createdRoleAssignmentId, 
                                        new Dictionary<string, object> { { "ErrorCode", 0 } });

            // Act
            var result = await _instanceRepository.CreateRoleAssignmentAsync(ProjectId, roleAssignment);

            // Assert
            _connection.Verify();
            Assert.AreEqual(result, createdRoleAssignmentId);
        }


        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task CreateRoleAssignment_GroupNotExists_ReturnResourceNotFoundError()
        {
            // Arrange
            int errorCode = 50006; // there are no groups with given Id

            int createdRoleAssignmentId = 0;

            RoleAssignmentDTO roleAssignment = new RoleAssignmentDTO()
            {
                GroupId = 0/*missing Id*/,
                RoleId = 1
            };

            _connection.SetupExecuteScalarAsync("CreateProjectRoleAssignment",
                            new Dictionary<string, object>
                            {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId }
                            },
                            createdRoleAssignmentId,
                            new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.CreateRoleAssignmentAsync(ProjectId, roleAssignment);

        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task CreateRoleAssignment_RoleNotExists_ReturnResourceNotFoundError()
        {
            // Arrange
            int errorCode = 50020; // there are no roles with given Id

            int createdRoleAssignmentId = 0;

            RoleAssignmentDTO roleAssignment = new RoleAssignmentDTO()
            {
                GroupId = 1,
                RoleId = 0/*missing Id*/
            };

            _connection.SetupExecuteScalarAsync("CreateProjectRoleAssignment",
                            new Dictionary<string, object>
                            {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId }
                            },
                            createdRoleAssignmentId,
                            new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.CreateRoleAssignmentAsync(ProjectId, roleAssignment);

        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task CreateRoleAssignment_ProjectNotExists_ReturnResourceNotFoundError()
        {
            // Arrange
            int errorCode = 50016; // there are no projects with given Id

            int createdRoleAssignmentId = 0;

            RoleAssignmentDTO roleAssignment = new RoleAssignmentDTO()
            {
                GroupId = 1,
                RoleId = 1
            };

            int projectId = 10000; // this id is not in the table yet

            _connection.SetupExecuteScalarAsync("CreateProjectRoleAssignment",
                            new Dictionary<string, object>
                            {
                                            { "ProjectId", projectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId }
                            },
                            createdRoleAssignmentId,
                            new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.CreateRoleAssignmentAsync(projectId, roleAssignment);

        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task CreateRoleAssignment_AssignmentAlreayExists_ReturnConflictExceptionError()
        {
            // Arrange
            int errorCode = 50021; // assignment with given Id already exists

            int createdRoleAssignmentId = 0;

            RoleAssignmentDTO roleAssignment = new RoleAssignmentDTO()
            {
                GroupId = 1,
                RoleId = 1
            };


            _connection.SetupExecuteScalarAsync("CreateProjectRoleAssignment",
                            new Dictionary<string, object>
                            {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId }
                            },
                            createdRoleAssignmentId,
                            new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.CreateRoleAssignmentAsync(ProjectId, roleAssignment);

        }

        #endregion

        #region GetProjectsAndFolder

        [TestMethod]
        public async Task GetProjectsAndFolders_AllParametersAreOk_ReturnNotEmptyQueryResult()
        {
            // arrange
            var total = 1;
            var spResult = new List<ProjectFolderSearchDto>() { new ProjectFolderSearchDto() { Id = 1, Location = "path"} };
            _connection.SetupQueryAsync("SearchProjectsAndFolders", It.IsAny<Dictionary<string, object>>(), spResult, new Dictionary<string, object> { { "Total", (int?)total } });

            // act
            var result =
                await _instanceRepository.GetProjectsAndFolders(1, _tabularData, SortingHelper.SortProjectFolders);
            // assert
            Assert.AreEqual(1, result.Total);
            Assert.AreEqual(spResult.First().Location, result.Items.ToList().First().Location);
        }

        [TestMethod]
        public async Task GetProjectsAndFolders_AllParametersAreOk_ReturnEmptyResult()
        {
            // arrange
            var total = 0;
            var spResult = new List<ProjectFolderSearchDto>();
            _connection.SetupQueryAsync("SearchProjectsAndFolders", It.IsAny<Dictionary<string, object>>(), spResult, new Dictionary<string, object> { { "Total", (int?)total } });

            // act
            var result =
                await _instanceRepository.GetProjectsAndFolders(1, _tabularData, SortingHelper.SortProjectFolders);
            // assert
            Assert.AreEqual(0, result.Total);
        }

        #endregion

        #region UpdateRoleAssignmentAsync

        [TestMethod]
        public async Task UpdateRoleAssignment_UpdateSuccessfull_ReturnNoError()
        {

            errorCode = 0;

            _connection.SetupExecuteScalarAsync("UpdateProjectRoleAssigment",
                                        new Dictionary<string, object>
                                        {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId },
                                            {"RoleAssignmentId", roleAssignmentId }
                                        },
                                        0,
                                        new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.UpdateRoleAssignmentAsync(ProjectId, roleAssignmentId, roleAssignment);

            // Assert
            _connection.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UpdateRoleAssignment_RoleAssignmentAlreadyExists_ThrowsConflictExceptionException()
        {

            errorCode = 50021;

            _connection.SetupExecuteScalarAsync("UpdateProjectRoleAssigment",
                                        new Dictionary<string, object>
                                        {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId },
                                            {"RoleAssignmentId", roleAssignmentId }
                                        },
                                        0,
                                        new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.UpdateRoleAssignmentAsync(ProjectId, roleAssignmentId, roleAssignment);

            _connection.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateRoleAssignment_RoleAssignmentNotExists_ThrowsResourceNotFoundException()
        {

            errorCode = 50022;

            _connection.SetupExecuteScalarAsync("UpdateProjectRoleAssigment",
                                        new Dictionary<string, object>
                                        {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId },
                                            {"RoleAssignmentId", roleAssignmentId }
                                        },
                                        0,
                                        new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.UpdateRoleAssignmentAsync(ProjectId, roleAssignmentId, roleAssignment);

            _connection.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateRoleAssignment_RoleNotExists_ThrowsResourceNotFoundException()
        {

            errorCode = 50020;

            _connection.SetupExecuteScalarAsync("UpdateProjectRoleAssigment",
                                        new Dictionary<string, object>
                                        {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId },
                                            {"RoleAssignmentId", roleAssignmentId }
                                        },
                                        0,
                                        new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.UpdateRoleAssignmentAsync(ProjectId, roleAssignmentId, roleAssignment);

            _connection.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateRoleAssignment_GroupNotExists_ThrowsResourceNotFoundException()
        {

            errorCode = 50006;

            _connection.SetupExecuteScalarAsync("UpdateProjectRoleAssigment",
                                        new Dictionary<string, object>
                                        {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId },
                                            {"RoleAssignmentId", roleAssignmentId }
                                        },
                                        0,
                                        new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.UpdateRoleAssignmentAsync(ProjectId, roleAssignmentId, roleAssignment);

            _connection.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateRoleAssignment_ProjectNotExists_ThrowsResourceNotFoundException()
        {
            
            errorCode = 50016;

            _connection.SetupExecuteScalarAsync("UpdateProjectRoleAssigment",
                                        new Dictionary<string, object>
                                        {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId },
                                            {"RoleAssignmentId", roleAssignmentId }
                                        },
                                        0,
                                        new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.UpdateRoleAssignmentAsync(ProjectId, roleAssignmentId, roleAssignment);

            _connection.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UpdateRoleAssignment_GeneralSQLError_ThrowsException()
        {

            errorCode = 50000;

            _connection.SetupExecuteScalarAsync("UpdateProjectRoleAssigment",
                                        new Dictionary<string, object>
                                        {
                                            { "ProjectId", ProjectId },
                                            {"GroupId", roleAssignment.GroupId },
                                            {"RoleId", roleAssignment.RoleId },
                                            {"RoleAssignmentId", roleAssignmentId }
                                        },
                                        0,
                                        new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await _instanceRepository.UpdateRoleAssignmentAsync(ProjectId, roleAssignmentId, roleAssignment);

            _connection.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task UpdateRoleAssignment_ProjectIdNotValid_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int projectId = 0;

            // Act
            await _instanceRepository.UpdateRoleAssignmentAsync(projectId, roleAssignmentId, roleAssignment);

            // Assert
            _connection.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task UpdateRoleAssignment_RoleAssignmentNotValid_ThrowsArgumentOutOfRangeException()
        {
            roleAssignment = null;

            // Act
            await _instanceRepository.UpdateRoleAssignmentAsync(ProjectId, roleAssignmentId, roleAssignment);

            // Assert
            _connection.Verify();
        }


        #endregion
    }
}
