using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlGroupRepositoryTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesConnectionToRaptorMain()
        {
            // Arrange

            // Act
            var repository = new SqlGroupRepository();

            // Assert
            Assert.AreEqual(ServiceConstants.RaptorMain, repository._connectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region AddGroupAsync

        [TestMethod]
        public async Task AddUserAsync_SuccessfulCreationOfUser_ReturnCreatedUserId()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName", Email = "GroupEmail", GroupSource = UserGroupSource.Database, License = LicenseType.Author
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var groupId = 100;
            cxn.SetupExecuteScalarAsync("AddGroup", It.IsAny<Dictionary<string, object>>(), groupId);

            // Act
            await repository.AddGroupAsync(group);

            // Assert
            cxn.Verify();
        }

        #endregion AddGroupAsync
    }
}
