using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlUserRepositoryTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesConnectionToRaptorMain()
        {
            // Arrange

            // Act
            var repository = new SqlUserRepository();

            // Assert
            Assert.AreEqual(WebApiConfig.RaptorMain, repository._connectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region GetUserByLoginAsync

        [TestMethod]
        public async Task GetUserByLoginAsync_QueryReturnsUser_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object);
            string login = "User";
            LoginUser[] result = { new LoginUser { Login = login } };
            cxn.SetupQueryAsync("GetUserByLogin", new Dictionary<string, object> { { "Login", login } }, result);

            // Act
            LoginUser user = await repository.GetUserByLoginAsync(login);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), user);
        }

        [TestMethod]
        public async Task GetUserByLoginAsync_QueryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object);
            string login = "User";
            LoginUser[] result = { };
            cxn.SetupQueryAsync("GetUserByLogin", new Dictionary<string, object> { { "Login", login } }, result);

            // Act
            LoginUser user = await repository.GetUserByLoginAsync(login);

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
			var repository = new SqlUserRepository(cxn.Object);
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
			var repository = new SqlUserRepository(cxn.Object);
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
            var repository = new SqlUserRepository(cxn.Object);
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
            var repository = new SqlUserRepository(cxn.Object);
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

        #region UpdateUserOnInvalidLoginAsync

        [TestMethod]
        public async Task UpdateUserOnInvalidLoginAsync_CallsProcedureWithCorrectParameters()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlUserRepository(cxn.Object);
            LoginUser user = new LoginUser
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
    }
}
