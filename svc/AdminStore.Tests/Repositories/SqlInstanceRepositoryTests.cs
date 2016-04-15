using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
using ServiceLibrary.Exceptions;

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
            int folderId = 99;
            InstanceItem[] result = { new InstanceItem { Id = folderId, Name = "Blueprint", ParentFolderId = 88} };
            cxn.SetupQueryAsync("GetInstanceFolderById", new Dictionary<string, object> { { "folderId", folderId } }, result);

            // Act
            var folder = await repository.GetInstanceFolderAsync(folderId);

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
            int folderId = 99;
            InstanceItem[] result = null;
            cxn.SetupQueryAsync("GetInstanceFolderById", new Dictionary<string, object> { { "folderId", folderId } }, result);

            // Act
            var folder = await repository.GetInstanceFolderAsync(folderId);

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
            var folder = await repository.GetInstanceFolderAsync(0);

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
    }
}
