using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlUserRepositoryTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesConnectionToRaptorMain()
        {
            // Arrange

            // Act
            var repository = new SqlUserRepository();

            // Assert
            Assert.AreEqual(ServiceConstants.RaptorMain, repository._connectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region GetUserByLoginAsync

        [TestMethod]
        public async Task GetUserByLoginAsync_QueryReturnsUser_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            string login = "User";
            AuthenticationUser[] result = { new AuthenticationUser { Login = login } };
            cxn.SetupQueryAsync("GetUserByLogin", new Dictionary<string, object> { { "Login", login } }, result);

            // Act
            AuthenticationUser user = await repository.GetUserByLoginAsync(login);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), user);
        }

        [TestMethod]
        public async Task GetUserByLoginAsync_QueryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            string login = "User";
            AuthenticationUser[] result = { };
            cxn.SetupQueryAsync("GetUserByLogin", new Dictionary<string, object> { { "Login", login } }, result);

            // Act
            AuthenticationUser user = await repository.GetUserByLoginAsync(login);

            // Assert
            cxn.Verify();
            Assert.IsNull(user);
        }

        #endregion GetUserByLoginAsync

        #region GetEffectiveUserLicense
        [TestMethod]
        public async Task GetEffectiveUserLicenseAsync_QueryReturnsLicenseType_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            var userId = 1;
            int[] result = { 3 };
            cxn.SetupQueryAsync("GetEffectiveUserLicense", new Dictionary<string, object> { { "UserId", userId } }, result);

            // Act
            var licenseType = await repository.GetEffectiveUserLicenseAsync(userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), licenseType);
        }

        [TestMethod]
        public async Task GetEffectiveUserLicenseAsync_QueryReturnsEmpty_ReturnsZero()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            var userId = 1;
            int[] result = { };
            cxn.SetupQueryAsync("GetEffectiveUserLicense", new Dictionary<string, object> { { "UserId", userId } }, result);

            // Act
            var licenseType = await repository.GetEffectiveUserLicenseAsync(userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(0, licenseType);
        }
        #endregion

        #region GetLoginUserByIdAsync

        [TestMethod]
        public async Task GetLoginUserByIdAsync_QueryReturnsUser_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            int userId = 1;
            LoginUser[] result = { new LoginUser { Id = userId } };
            cxn.SetupQueryAsync("GetLoginUserById", new Dictionary<string, object> { { "UserId", userId } }, result);

            // Act
            LoginUser user = await repository.GetLoginUserByIdAsync(userId);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), user);
        }

        [TestMethod]
        public async Task GetLoginUserByIdAsync_QueryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            int userId = 5;
            LoginUser[] result = { };
            cxn.SetupQueryAsync("GetLoginUserById", new Dictionary<string, object> { { "UserId", userId } }, result);

            // Act
            LoginUser user = await repository.GetLoginUserByIdAsync(userId);

            // Assert
            cxn.Verify();
            Assert.IsNull(user);
        }

        #endregion GetLoginUserByIdAsync

        #region GetLicenseTransactionUserInfoAsync

        [TestMethod]
        public async Task GetLicenseTransactionUserInfoAsync_QueryReturnsUsers_ReturnsUsers()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            int[] userIds = { 1, 2, 3 };
            var userIdTable = SqlConnectionWrapper.ToDataTable(userIds, "Int32Collection", "Int32Value");
            LicenseTransactionUser[] result =
            {
                new LicenseTransactionUser { Id = 1, Login = "Login", Department = "Dept" },
                new LicenseTransactionUser { Id = 2, Login = "Login2", Department = null },
                new LicenseTransactionUser { Id = 3, Login = "Login3", Department = "Another Dept" }
            };
            cxn.SetupQueryAsync("GetLicenseTransactionUser", new Dictionary<string, object> { { "UserIds", userIdTable } }, result);

            // Act
            IEnumerable<LicenseTransactionUser> users = await repository.GetLicenseTransactionUserInfoAsync(userIds);

            // Assert
            cxn.Verify();
            CollectionAssert.AreEquivalent(result, users.ToList());
        }

        #endregion GetLicenseTransactionUserInfoAsync

        #region UpdateUserOnInvalidLoginAsync

        [TestMethod]
        public async Task UpdateUserOnInvalidLoginAsync_CallsProcedureWithCorrectParameters()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            AuthenticationUser user = new AuthenticationUser
            {
                Login = "User",
                IsEnabled = true,
                InvalidLogonAttemptsNumber = 1,
                LastInvalidLogonTimeStamp = new System.DateTime(0L)
            };
            cxn.SetupExecuteAsync(
                "UpdateUserOnInvalidLogin",
                new Dictionary<string, object>
                {
                    { "Login", user.Login },
                    { "Enabled", user.IsEnabled },
                    { "InvalidLogonAttemptsNumber", user.InvalidLogonAttemptsNumber },
                    { "LastInvalidLogonTimeStamp", user.LastInvalidLogonTimeStamp }
                },
                1);

            // Act
            await repository.UpdateUserOnInvalidLoginAsync(user);

            // Assert
            cxn.Verify();
        }

        #endregion UpdateUserOnInvalidLoginAsync

        #region UpdateUserOnPasswordResetAsync

        [TestMethod]
        public async Task UpdateUserOnPasswordResetAsync_Success()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            var user = new AuthenticationUser
            {
                Login = "User",
                Id = 99,
                Password = "Password",
                UserSalt = new Guid()
            };
            cxn.SetupExecuteAsync(
                "AddCurrentUserPasswordToHistory",
                new Dictionary<string, object>
                {
                    { "@userId", user.Id }
                },
                1);

            cxn.SetupExecuteAsync(
                "UpdateUserOnPasswordResetAsync",
                new Dictionary<string, object>
                {
                    { "@Login", user.Login },
                    { "@Password", user.Password },
                    { "@UserSALT", user.UserSalt }
                },
                1);

            // Act
            await repository.UpdateUserOnPasswordResetAsync(user);

            // Assert
            cxn.Verify();
        }

        #endregion UpdateUserOnPasswordResetAsync

        #region ValidateUserPasswordForHistoryAsync

        [TestMethod]
        public async Task ValidadeUserPasswordForHistoryAsync_Valid_True()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            const int userId = 99;
            const string newPassword = "NewPassword";
            var passwordHystory = new List<SqlUserRepository.HashedPassword>
            {
                new SqlUserRepository.HashedPassword { Password = "aaa", UserSALT = new Guid() }
            };
            cxn.SetupQueryAsync(
                "GetLastUserPasswords",
                new Dictionary<string, object>
                {
                    { "@userId", userId }
                },
                passwordHystory);

            // Act
            var result = await repository.ValidateUserPasswordForHistoryAsync(userId, newPassword);

            // Assert
            cxn.Verify();
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task ValidadeUserPasswordForHistoryAsync_Invalid_False()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);
            const int userId = 99;
            var userSalt = new Guid();
            const string newPassword = "NewPassword";
            var passwordHystory = new List<SqlUserRepository.HashedPassword>
            {
                new SqlUserRepository.HashedPassword { Password = HashingUtilities.GenerateSaltedHash(newPassword, userSalt), UserSALT = userSalt }
            };
            cxn.SetupQueryAsync(
                "GetLastUserPasswords",
                new Dictionary<string, object>
                {
                    { "@userId", userId }
                },
                passwordHystory);

            // Act
            var result = await repository.ValidateUserPasswordForHistoryAsync(userId, newPassword);

            // Assert
            cxn.Verify();
            Assert.AreEqual(false, result);
        }

        #endregion ValidateUserPasswordForHistoryAsync


        #region AddUserAsync

        [TestMethod]
        public async Task AddUserSucess()
        {
            var user = new User()
            {
                Login = "LoginUser",
                FirstName = "FirstNameValue",
                LastName = "LastNameValue",
                DisplayName = "DisplayNameValue",
                Email = "email@test.com",
                Source = UserGroupSource.Database,
                AllowFallback = false,
                Enabled = true,
                ExpirePassword = true,
                NewPassword = "dGVzdA==",
                UserSALT = Guid.NewGuid(),
                Title = "TitleValue",
                Department = "Departmentvalue",
                GroupMembership = new int[] { 1 },
                Guest = false
            };

            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);

            var userId = 100;
            cxn.SetupExecuteScalarAsync<int>( "AddUserAsync", It.IsAny<Dictionary<string, object>>(), userId);

            var result = await repository.AddUserAsync(user);

            cxn.Verify();
            Assert.AreEqual(result, userId);
        }

        #endregion AddUserAsync



        #region HasPermissionsAsync

        [TestMethod]
        public async Task HasPermissions_True()
        {
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);

            var instanceAdminPrivilegesInput = new InstanceAdminPrivileges[2]
            {InstanceAdminPrivileges.ManageUsers, InstanceAdminPrivileges.AssignAdminRoles};

            var instanceAdminPrivilegesOutPut = new List<PermissionsItem>() {new PermissionsItem {PermissionValue = 3072}, new PermissionsItem { PermissionValue = 31744 } };

            var userId = 100;
            cxn.SetupQueryAsync<PermissionsItem>("CheckPermissionsForUser", It.IsAny<Dictionary<string, object>>(), instanceAdminPrivilegesOutPut);

            var result = await repository.HasPermissionsAsync(userId, instanceAdminPrivilegesInput);

            cxn.Verify();
            Assert.IsTrue(result);
        }


        [TestMethod]
        public async Task HasPermissions_False()
        {
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object, cxn.Object);

            var instanceAdminPrivilegesInput = new InstanceAdminPrivileges[2]
            {InstanceAdminPrivileges.ManageUsers, InstanceAdminPrivileges.AssignAdminRoles};

            var instanceAdminPrivilegesOutPut = new List<PermissionsItem>() { new PermissionsItem { PermissionValue = 3072 }, new PermissionsItem { PermissionValue = 31745 } };

            var userId = 100;
            cxn.SetupQueryAsync<PermissionsItem>("CheckPermissionsForUser", It.IsAny<Dictionary<string, object>>(), instanceAdminPrivilegesOutPut);

            var result = await repository.HasPermissionsAsync(userId, instanceAdminPrivilegesInput);

            cxn.Verify();
            Assert.IsFalse(result);
        }

        #endregion HasPermissionsAsync
    }
}
