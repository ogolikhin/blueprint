using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    [TestClass]
    public class InstanceRolesRepositoryTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesConnectionToRaptorMain()
        {
            // Arrange

            // Act
            var repository = new InstanceRolesRepository();

            // Assert
            Assert.AreEqual(ServiceConstants.RaptorMain, repository._connectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region GetInstanceRolesAsync

        [TestMethod]
        public async Task GetInstanceRolesAsync_SuccessfulGettingInstanceRoles_ReturnInstanceRolesList()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new InstanceRolesRepository(cxn.Object);
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

            cxn.SetupQueryAsync<AdminRole>("GetInstanceAdminRoles", null, adminRoles);

            // Act
            var result = await repository.GetInstanceRolesAsync();

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, adminRoles);
        }

        #endregion GetInstanceRolesAsync
    }
}
