﻿using AdminStore.Models;
using AdminStore.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlGroupRepositoryTests
    {
        #region AddGroupAsync

        [TestMethod]
        public async Task AddUserAsync_SuccessfulCreationOfGroup_ReturnCreatedUserId()
        {
            // Arrange
            var group = new GroupDto
            {
                Name = "GroupName",
                Email = "GroupEmail",
                Source = UserGroupSource.Database,
                LicenseType = LicenseType.Author
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var groupId = 100;
            cxn.SetupExecuteScalarAsync("AddGroup", It.IsAny<Dictionary<string, object>>(), groupId);

            // Act
            await repository.AddGroupAsync(group, null);

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

            cxn.SetupExecuteScalarAsync("UpdateGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.UpdateGroupAsync(groupId, group, null);

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
            await repository.UpdateGroupAsync(groupId, group, null);

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
            await repository.UpdateGroupAsync(groupId, group, null);

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
            await repository.UpdateGroupAsync(groupId, group, null);

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
            await repository.UpdateGroupAsync(groupId, group, null);

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
            await repository.UpdateGroupAsync(groupId, group, null);

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
            await repository.UpdateGroupAsync(groupId, group, null);

            // Assert
            // Exception
        }

        #endregion UpdateGroupAsync

        #region DeleteMembersFromGroupAsync

        [TestMethod]
        public async Task DeleteMembersFromGroupAsync_SuccessfulDeletingMembers_ReturnCountDeletedMembers()
        {
            // Arrange
            var assignScope = new AssignScope
            {
                SelectAll = true,
                Members = new List<KeyValuePair<int, UserType>> { new KeyValuePair<int, UserType>(1, UserType.User) }
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = 0;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("DeleteMembersFromGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.DeleteMembersFromGroupAsync(groupId, assignScope);

            // Assert
            cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task DeleteMembersFromGroupAsync_GeneralSqlExeption_ReturnException()
        {
            // Arrange
            var assignScope = new AssignScope
            {
                SelectAll = true,
                Members = new List<KeyValuePair<int, UserType>> { new KeyValuePair<int, UserType>(1, UserType.User) }
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = (int)SqlErrorCodes.GeneralSqlError;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("DeleteMembersFromGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.DeleteMembersFromGroupAsync(groupId, assignScope);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task DeleteMembersFromGroupAsync_GroupWithCurrentIdNotExist_ReturnNotFoundException()
        {
            // Arrange
            var assignScope = new AssignScope
            {
                SelectAll = true,
                Members = new List<KeyValuePair<int, UserType>> { new KeyValuePair<int, UserType>(1, UserType.User) }
            };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var errorId = (int)SqlErrorCodes.GroupWithCurrentIdNotExist;
            var groupId = 1;

            cxn.SetupExecuteScalarAsync("DeleteMembersFromGroup", It.IsAny<Dictionary<string, object>>(), 0, new Dictionary<string, object> { { "ErrorCode", errorId } });

            // Act
            await repository.DeleteMembersFromGroupAsync(groupId, assignScope);

            // Assert
            // Exception
        }
        #endregion DeleteMembersFromGroupAsync

        #region Project Group

        [TestMethod]
        public async Task GetProjectGroupsAsync_GroupsFound_NoErrors()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var projectId = 100;
            int errorCode = 0;

            Group[] projectGroups =
            {
                new Group()
                {
                    Name = "Group1"
                },
                new Group()
                {
                    Name = "Group2"
                },
                new Group()
                {
                    Name = "Group3"
                }
            };

            var tabularData = new TabularData
            {
                Pagination = new Pagination { Limit = 10, Offset = 0 },
                Sorting = new Sorting { Order = SortOrder.Asc, Sort = "name" }
            };

            cxn.SetupQueryAsync("GetAvailableGroupsForProject",
                new Dictionary<string, object> { { "projectId", projectId } },
                projectGroups,
                new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await repository.GetProjectGroupsAsync(projectId, tabularData, SortingHelper.SortProjectGroups);

            // Assert
            cxn.Verify();
        }


        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectGroupsAsync_ProjectNotFound_NotFoundError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlGroupRepository(cxn.Object);
            var projectId = 1;
            var tabularData = new TabularData
            {
                Pagination = new Pagination { Limit = 10, Offset = 0 },
                Sorting = new Sorting { Order = SortOrder.Asc, Sort = "name" }
            };
            int errorCode = 50016; // there are no project for this projectId

            Group[] projectGroups = { };

            cxn.SetupQueryAsync("GetAvailableGroupsForProject",
                new Dictionary<string, object> { { "projectId", projectId } },
                projectGroups,
                new Dictionary<string, object> { { "ErrorCode", errorCode } });

            // Act
            await repository.GetProjectGroupsAsync(projectId, tabularData);
        }
        #endregion
    }
}
