using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlPrivilegesRepositoryTest
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesConnectionToRaptorMain()
        {
            // Arrange

            // Act
            var repository = new SqlPrivilegesRepository();

            // Assert
            Assert.AreEqual(ServiceConstants.RaptorMain, repository._connectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

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
