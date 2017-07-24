using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlInstanceRepositoryTests
    {
        [TestMethod]
        public async Task GetInstanceFolderAsync_Found()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var folderId = 99;
            var userId = 9;
            InstanceItem[] result = { new InstanceItem { Id = folderId, Name = "Blueprint", ParentFolderId = 88} };
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

        [TestMethod]
        public async Task GetInstanceProjectAsync_Found()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            int projectId = 99;
            int userId = 10;
            InstanceItem[] result = { new InstanceItem { Id = projectId, Name = "My Project", ParentFolderId = 88, IsAccesible = true} };
            cxn.SetupQueryAsync("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var project = await repository.GetInstanceProjectAsync(projectId, userId);

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
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetInstanceProjectAsync_NotFound()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            int projectId = 99;
            int userId = 10;
            InstanceItem[] result = {};
            cxn.SetupQueryAsync("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            var project = await repository.GetInstanceProjectAsync(projectId, userId);

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

        private List<ArtifactsNavigationPath> GetProjectNavigationPathSample()
        {
            return new List<ArtifactsNavigationPath>
            {
                new ArtifactsNavigationPath { Level = 0, ArtifactId = 1, Name = "ProjectName"},
                new ArtifactsNavigationPath { Level = 1, ArtifactId = 2, Name = "Blueprint"}
            };
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

        #region GetInstanceRolesAsync

        [TestMethod]
        public async Task GetInstanceRolesAsync_SuccessfulGettingInstanceRoles_ReturnInstanceRolesList()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlInstanceRepository(cxn.Object);
            var adminRoles = new List<AdminRole>()
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
            var folder = new FolderDto {Name = "Folder1", ParentFolderId = 1};

            cxn.SetupExecuteScalarAsync("CreateFolder", It.IsAny<Dictionary<string, object>>(), folderId, new Dictionary<string, object> { { "ErrorCode", 0 } });

            // Act
            var result = await repository.CreateFolderAsync(folder);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, folderId);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task CreateFolderAsync_FolderNotFound_ReturnNotFoundError()
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
            //Exception
        }

        #endregion CreateFolderAsync
    }
}
