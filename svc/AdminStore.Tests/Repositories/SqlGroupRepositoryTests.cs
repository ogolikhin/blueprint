using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
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
        public async Task AddUserAsync_SuccessfulCreationOfGroup_ReturnCreatedUserId()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName", Email = "GroupEmail", Source = UserGroupSource.Database, LicenseType = LicenseType.Author
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


        #region UpdateGroupAsync

        [TestMethod]
        public async Task UpdateGroupAsync_SuccessfulUpdateOfGroup_ReturnOk()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName",
                Email = "GroupEmail",
                Source = UserGroupSource.Database,
                LicenseType = LicenseType.Author,
                CurrentVersion = 0
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = 0;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("UpdateGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId }});

            // Act
            await repository.UpdateGroupAsync(groupId, group);

            // Assert
            cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UpdateGroupAsync_GeneralSqlExeption_ReturnException()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName",
                Email = "GroupEmail",
                Source = UserGroupSource.Database,
                LicenseType = LicenseType.Author,
                CurrentVersion = 0
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = (int)SqlErrorCodes.GeneralSqlError;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("UpdateGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.UpdateGroupAsync(groupId, group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateGroupAsync_GroupWithCurrentIdNotExist_ReturnNotFoundException()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName",
                Email = "GroupEmail",
                Source = UserGroupSource.Database,
                LicenseType = LicenseType.Author,
                CurrentVersion = 0
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = (int)SqlErrorCodes.GroupWithCurrentIdNotExist;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("UpdateGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.UpdateGroupAsync(groupId, group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UpdateGroupAsync_GroupVersionsNotEqual_ReturnConflictException()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName",
                Email = "GroupEmail",
                Source = UserGroupSource.Database,
                LicenseType = LicenseType.Author,
                CurrentVersion = 0
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = (int)SqlErrorCodes.GroupVersionsNotEqual;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("UpdateGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.UpdateGroupAsync(groupId, group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateGroupAsync_UpdateGroupWithExistingScope_ReturnBadRequestException()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName",
                Email = "GroupEmail",
                Source = UserGroupSource.Database,
                LicenseType = LicenseType.Author,
                CurrentVersion = 0
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = (int)SqlErrorCodes.GroupCanNotBeUpdatedWithExistingScope;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("UpdateGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.UpdateGroupAsync(groupId, group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateGroupAsync_GroupWithNameAndLicenseIdExist_BadRequestException()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName",
                Email = "GroupEmail",
                Source = UserGroupSource.Database,
                LicenseType = LicenseType.Author,
                CurrentVersion = 0
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = (int)SqlErrorCodes.GroupWithNameAndLicenseIdExist;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("UpdateGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.UpdateGroupAsync(groupId, group);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateGroupAsync_GroupWithNameAndScopeExist_BadRequestException()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName",
                Email = "GroupEmail",
                Source = UserGroupSource.Database,
                LicenseType = LicenseType.Author,
                CurrentVersion = 0
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = (int)SqlErrorCodes.GroupWithNameAndScopeExist;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("UpdateGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.UpdateGroupAsync(groupId, group);

            // Assert
            // Exception
        }

        #endregion UpdateGroupAsync
    }
}
