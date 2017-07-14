using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}
