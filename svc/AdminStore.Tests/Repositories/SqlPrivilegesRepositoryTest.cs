using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
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
        public async Task GetUserPermissionsAsync_SuccessfulGettingOfPermissions_ReturnUsersPermissions()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlPrivilegesRepository(cxn.Object);
            var permissions = 100;
            var userId = 1;
            cxn.SetupQueryAsync<int>("GetInstancePermissionsForUser", It.IsAny<Dictionary<string, object>>(), new List<int>() { permissions });

            // Act
            var result = await repository.GetUserPermissionsAsync(userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, permissions);
        }

        #endregion GetUserPermissionsAsync
    }
}
