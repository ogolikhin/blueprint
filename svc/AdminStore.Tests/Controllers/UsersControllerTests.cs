﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Exceptions;

namespace AdminStore.Controllers
{
    [TestClass]
    public class UsersControllerTests
    {
        private Mock<ISqlUserRepository> _usersRepoMock;
        private Mock<IServiceLogRepository> _logMock;
        private Mock<IAuthenticationRepository> _authRepoMock;
        private Mock<ISqlSettingsRepository> _settingsRepoMock;
        private Mock<IEmailHelper> _emailHelperMock;
        private UsersController _controller;

        [TestInitialize]
        public void Initialize()
        {
            var session = new Session { UserId = 1 };
            _usersRepoMock = new Mock<ISqlUserRepository>();
            _logMock = new Mock<IServiceLogRepository>();
            _authRepoMock = new Mock<IAuthenticationRepository>();
            _settingsRepoMock = new Mock<ISqlSettingsRepository>();
            _emailHelperMock = new Mock<IEmailHelper>();
            _controller = new UsersController(_authRepoMock.Object, _usersRepoMock.Object, _settingsRepoMock.Object, _emailHelperMock.Object, _logMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            _controller.Request.Properties[ServiceConstants.SessionProperty] = session;
        }

        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new UsersController();

            // Assert
            Assert.IsInstanceOfType(controller._userRepository, typeof(SqlUserRepository));
            Assert.IsInstanceOfType(controller._log, typeof(ServiceLogRepository));
        }

        #endregion

        #region GetUserIcon

        [TestMethod]
        public async Task GetUserIcon_RepositoryReturnsIcon_ReturnsIcon()
        {
            var userId = 1;
            var content = new byte[] {0x20, 0x20, 0x20, 0x20};
            var userIcon = new UserIcon {UserId = userId, Content = content };
            _usersRepoMock
                .Setup(repo => repo.GetUserIconByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync(userIcon);

            // Act
            var result = await _controller.GetUserIcon(userId);

            // Assert
            var actualContent = await result.Content.ReadAsByteArrayAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(content.SequenceEqual(actualContent));
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetUserIcon_RepositoryReturnsNull_NotFoundResult()
        {
            // Arrange
            var userId = 1;
            _usersRepoMock
                .Setup(repo => repo.GetUserIconByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync(null);

            // Act
            try
            {
                var result = await _controller.GetUserIcon(userId);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsInstanceOfType(ex, typeof(ResourceNotFoundException));
                throw;
            }
        }

        [TestMethod]
        public async Task GetUserIcon_RepositoryReturnsEmptyContent_NoContentResult()
        {
            // Arrange
            var userId = 1;
            var userIcon = new UserIcon { UserId = userId, Content = null };
            _usersRepoMock
                .Setup(repo => repo.GetUserIconByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync(userIcon);

            // Act
            var result = await _controller.GetUserIcon(userId);

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetUserIcon_RepositoryThrowsException_InternalServerErrorResult()
        {
            // Arrange
            var userId = 1;
            _usersRepoMock
                .Setup(repo => repo.GetUserIconByUserIdAsync(It.IsAny<int>()))
                .Throws(new Exception());

            // Act
            await _controller.GetUserIcon(userId);

            // Assert
        }

        #endregion

        #region GetLoginUser

        [TestMethod]
        public async Task GetLoginUser_RepositoryReturnsUser_ReturnsUser()
        {
            // Arrange
            var loginUser = new LoginUser();
            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(loginUser);

            // Act
            var result = await _controller.GetLoginUser() as OkNegotiatedContentResult<LoginUser>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(loginUser, result.Content);
        }

        [TestMethod]
        public async Task GetLoginUser_RepositoryReturnsNull_UnauthorizedResult()
        {
            // Arrange
            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(null);

            // Act
            IHttpActionResult result = await _controller.GetLoginUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task GetLoginUser_RepositoryThrowsException_InternalServerErrorResult()
        {
            // Arrange
            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .Throws(new Exception());

            // Act
            IHttpActionResult result = await _controller.GetLoginUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetLoginUser

        #region PostReset

        [TestMethod]
        public async Task PostReset_RepositoryReturnsSuccessfully()
        {
            // Arrange
            Exception exception = null;
            var loginUser = new AuthenticationUser();
            var newPass = SystemEncryptions.EncodeTo64UTF8("123EWQ!@#");
            var oldPass = SystemEncryptions.EncodeTo64UTF8("changeme");
            _authRepoMock
                .Setup(repo => repo.ResetPassword(It.IsAny<AuthenticationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            _authRepoMock
               .Setup(repo => repo.AuthenticateUserForResetAsync(It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(loginUser);

            // Act
            try
            {
                await _controller.PostReset(SystemEncryptions.EncodeTo64UTF8("admin"), new ResetPostContent {NewPass = newPass, OldPass = oldPass});
            }
            catch (Exception ex)
            { 
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        public async Task PostReset_RepositoryReturnsNull_UnauthorizedResult()
        {
            // Arrange
            Exception exception = null;
            var loginUser = new AuthenticationUser();
            var newPass = SystemEncryptions.EncodeTo64UTF8("123EWQ!@#");
            var oldPass = SystemEncryptions.EncodeTo64UTF8("changeme");
            _authRepoMock
                .Setup(repo => repo.ResetPassword(It.IsAny<AuthenticationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new AuthenticationException(""));
            _authRepoMock
               .Setup(repo => repo.AuthenticateUserForResetAsync(It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(loginUser);

            // Act
            try
            {
                await _controller.PostReset(SystemEncryptions.EncodeTo64UTF8("admin"), new ResetPostContent { NewPass = newPass, OldPass = oldPass });
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsInstanceOfType(exception, typeof(AuthenticationException));
        }

        #endregion PostReset

        #region PasswordRecovery

        [TestMethod]
        public async Task PostRequestPasswordReset_RepositoriesReturnsSuccessfully()
        {
            // Arrange
            Exception exception = null;
            SetupMocksForRequestPasswordReset();

            // Act
            IHttpActionResult result = null;
            try
            {
                result = await _controller.PostRequestPasswordResetAsync("login");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_UserCannotResetPassword()
        {
            // Arrange
            Exception exception = null;
            SetupMocksForRequestPasswordReset();

            _usersRepoMock
                .Setup(repo => repo.CanUserResetPasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            IHttpActionResult result = null;
            try
            {
                result = await _controller.PostRequestPasswordResetAsync("login");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Conflict, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_UserHasExceededRequestLimit()
        {
            // Arrange
            Exception exception = null;
            SetupMocksForRequestPasswordReset();

            _usersRepoMock
                .Setup(repo => repo.HasUserExceededPasswordRequestLimitAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            IHttpActionResult result = null;
            try
            {
                result = await _controller.PostRequestPasswordResetAsync("login");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Conflict, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_InstanceEmailIsNotSetUp()
        {
            // Arrange
            Exception exception = null;
            SetupMocksForRequestPasswordReset();

            var instanceSettings = new InstanceSettings() { EmailSettingsDeserialized = null };

            _settingsRepoMock
                .Setup(repo => repo.GetInstanceSettingsAsync())
                .ReturnsAsync(instanceSettings);

            // Act
            IHttpActionResult result = null;
            try
            {
                result = await _controller.PostRequestPasswordResetAsync("login");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Conflict, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_CreatingRecoveryTokenThrowsException()
        {
            // Arrange
            Exception exception = null;
            SetupMocksForRequestPasswordReset();

            _usersRepoMock
                .Setup(repo => repo.UpdatePasswordRecoveryTokensAsync(It.IsAny<string>()))
                .Throws(new Exception("any"));

            // Act
            IHttpActionResult result = null;
            try
            {
                result = await _controller.PostRequestPasswordResetAsync("login");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        private void SetupMocksForRequestPasswordReset()
        {
            var emailConfigSettings = new Mock<IEmailConfigInstanceSettings>();
            emailConfigSettings
                .SetupGet(s => s.HostName)
                .Returns("http://myhostname");

            var instanceSettings = new InstanceSettings() { EmailSettingsDeserialized = emailConfigSettings.Object };

            _settingsRepoMock
                .Setup(repo => repo.GetInstanceSettingsAsync())
                .ReturnsAsync(instanceSettings);

            _usersRepoMock
                .Setup(repo => repo.GetUserByLoginAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationUser() { Email = "a@b.com" });

            _usersRepoMock
                .Setup(repo => repo.CanUserResetPasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _usersRepoMock
                .Setup(repo => repo.HasUserExceededPasswordRequestLimitAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _usersRepoMock
                .Setup(repo => repo.UpdatePasswordRecoveryTokensAsync(It.IsAny<string>()));

            _emailHelperMock
                .Setup(helper => helper.Initialize(It.IsAny<IEmailConfigInstanceSettings>()));

            _emailHelperMock
                .Setup(helper => helper.SendEmail(It.IsAny<string>()));
        }

        #endregion PasswordRecovery

    }
}
