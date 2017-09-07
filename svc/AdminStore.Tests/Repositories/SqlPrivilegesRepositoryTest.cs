using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlPrivilegesRepositoryTest
    {
        #region GetUserPermissionsAsync

        [TestMethod]
        public async Task GetInstanceAdminPrivilegesAsync_ExistingUser_ReturnUsersPermissions()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlPrivilegesRepository(cxn.Object);
            var permissions = InstanceAdminPrivileges.AssignAdminRoles;
            var userId = 1;
            cxn.SetupExecuteScalarAsync("GetInstancePermissionsForUser", It.IsAny<Dictionary<string, object>>(), permissions);

            // Act
            var result = await repository.GetInstanceAdminPrivilegesAsync(userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, permissions);
        }

        #endregion GetUserPermissionsAsync

        #region GetProjectAdminPermissionsAsync

        [TestMethod]
        public async Task GetProjectAdminPermissionsAsync_ExistingUserAndProject_ReturnUsersPermissionsForProject()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlPrivilegesRepository(cxn.Object);
            var permissions = ProjectAdminPrivileges.ManageGroupsAndRoles;
            var userId = 1;
            var projectId = 1;
            cxn.SetupExecuteScalarAsync("GetProjectAdminPermissions", It.IsAny<Dictionary<string, object>>(), permissions, new Dictionary<string, object> { { "ErrorCode", 0 } });

            // Act
            var result = await repository.GetProjectAdminPermissionsAsync(userId, projectId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, permissions);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectAdminPermissionsAsync_ProjectNotExist_ReturnNotFoundException()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlPrivilegesRepository(cxn.Object);
            var permissions = ProjectAdminPrivileges.ManageGroupsAndRoles;
            var userId = 1;
            var projectId = 1;
            cxn.SetupExecuteScalarAsync("GetProjectAdminPermissions", It.IsAny<Dictionary<string, object>>(), permissions, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.ProjectWithCurrentIdNotExist } });

            // Act
            await repository.GetProjectAdminPermissionsAsync(userId, projectId);

            // Assert
            cxn.Verify();
        }

        #endregion
    }
}
