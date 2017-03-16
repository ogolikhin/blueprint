using System;
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
using System.Collections.Generic;
using System.Net.Http.Formatting;

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
        private Mock<IApplicationSettingsRepository> _applicationSettingsRepository;
        private UsersController _controller;
        private Mock<IHttpClientProvider> _httpClientProviderMock ;

        [TestInitialize]
        public void Initialize()
        {
            var session = new Session { UserId = 1 };
            _usersRepoMock = new Mock<ISqlUserRepository>();
            _logMock = new Mock<IServiceLogRepository>();
            _authRepoMock = new Mock<IAuthenticationRepository>();
            _settingsRepoMock = new Mock<ISqlSettingsRepository>();
            _emailHelperMock = new Mock<IEmailHelper>();
            _httpClientProviderMock = new Mock<IHttpClientProvider>();
            _applicationSettingsRepository = new Mock<IApplicationSettingsRepository>();
            _controller = new UsersController(_authRepoMock.Object, _usersRepoMock.Object, _settingsRepoMock.Object, 
                _emailHelperMock.Object, _applicationSettingsRepository.Object, _logMock.Object, _httpClientProviderMock.Object)
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
        public async Task PostPasswordReset_NoToken_ReturnsBadRequest()
        {
            // Arrange

            //This token simulates a token generated by the controller when no token is provided
            var emptyInputToken = new Guid("00000000-0000-0000-0000-000000000000");
          
            // Act
            IHttpActionResult result = null;
            Exception exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = emptyInputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
            Assert.AreEqual(ErrorCodes.PasswordResetEmptyToken, ((BadRequestException)exception).ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_TokenListEmpty_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>();
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(tokenList);

            // Act
            IHttpActionResult result = null;
            Exception exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
            Assert.AreEqual(ErrorCodes.PasswordResetTokenNotFound, ((ConflictException)exception).ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_TokenNotMostRecent_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf")},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-20),
                    Login = "testUser", RecoveryToken = inputToken},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4")}
            };
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(tokenList);


            // Act
            IHttpActionResult result = null;
            Exception exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
            Assert.AreEqual(ErrorCodes.PasswordResetTokenNotLatest, ((ConflictException)exception).ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_ExpiredToken_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-200),
                    Login = "testUser", RecoveryToken = inputToken},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf")},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4")}
            };
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(tokenList);


            // Act
            IHttpActionResult result = null;
            Exception exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
            Assert.AreEqual(ErrorCodes.PasswordResetTokenExpired, ((ConflictException)exception).ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_NullUser_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-2),
                    Login = "testUser", RecoveryToken = inputToken},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf")},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4")}
            };
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(tokenList);
            _usersRepoMock
                .Setup(repo => repo.GetUserByLoginAsync(It.IsAny<string>()))
                .ReturnsAsync(null);
            

            // Act
            IHttpActionResult result = null;
            Exception exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
            Assert.AreEqual(ErrorCodes.PasswordResetTokenInvalid, ((ConflictException)exception).ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_DisabledUser_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-2),
                    Login = "testUser", RecoveryToken = inputToken},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf")},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4")}
            };
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(tokenList);
            _usersRepoMock
                .Setup(repo => repo.GetUserByLoginAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationUser {IsEnabled = false});


            // Act
            IHttpActionResult result = null;
            Exception exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
            Assert.AreEqual(ErrorCodes.PasswordResetUserDisabled, ((ConflictException)exception).ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_RepositoriesReturnsSuccessfully()
        {
            // Arrange
            var uid = 3;
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-2),
                    Login = "testUser", RecoveryToken = inputToken},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf")},
                new PasswordRecoveryToken {CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4")}
            };
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(tokenList);
            _usersRepoMock
                .Setup(repo => repo.GetUserByLoginAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationUser {Id = uid, IsEnabled = true});
            _authRepoMock
                .Setup(a => a.ResetPassword(It.IsAny<AuthenticationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            var httpClientProvider = new TestHttpClientProvider(request => request.RequestUri.AbsolutePath.EndsWithOrdinal($"sessions/{uid}") ?
                     new HttpResponseMessage(HttpStatusCode.OK) : null);

            _controller = new UsersController(_authRepoMock.Object, _usersRepoMock.Object, _settingsRepoMock.Object,
                _emailHelperMock.Object, _applicationSettingsRepository.Object, _logMock.Object, httpClientProvider)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            // Act
            IHttpActionResult result = null;
            var resetContent = new ResetPasswordContent {Password = "MTIzNFJFV1EhQCMk", Token = inputToken};
            result = await _controller.PostPasswordResetAsync(resetContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_RepositoriesReturnsSuccessfully()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_UserCannotResetPassword()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            _usersRepoMock
                .Setup(repo => repo.CanUserResetPasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ResponseMessageResult));
            Assert.AreEqual(HttpStatusCode.Conflict, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_UserHasExceededRequestLimit()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            _usersRepoMock
                .Setup(repo => repo.HasUserExceededPasswordRequestLimitAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ResponseMessageResult));
            Assert.AreEqual(HttpStatusCode.Conflict, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_UserHasHitPasswordChangeLockout()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            _authRepoMock
                .Setup(repo => repo.IsChangePasswordCooldownInEffect(It.IsAny<AuthenticationUser>()))
                .ReturnsAsync(true);

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ResponseMessageResult));
            Assert.AreEqual(HttpStatusCode.Conflict, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_InstanceEmailIsNotSetUp()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            var instanceSettings = new InstanceSettings() { EmailSettingsDeserialized = null };

            _settingsRepoMock
                .Setup(repo => repo.GetInstanceSettingsAsync())
                .ReturnsAsync(instanceSettings);

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ResponseMessageResult));
            Assert.AreEqual(HttpStatusCode.Conflict, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_PasswordRecoveryDisabled()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            IEnumerable<ApplicationSetting> applicationSettings = new List<ApplicationSetting>() { new ApplicationSetting() { Key = "IsPasswordRecoveryEnabled", Value = "false" } };
            _applicationSettingsRepository
                .Setup(repo => repo.GetSettings())
                .ReturnsAsync(applicationSettings);

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ResponseMessageResult));
            Assert.AreEqual(HttpStatusCode.Conflict, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_PasswordRecoveryEntryMissingFromDatabase()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            IEnumerable<ApplicationSetting> applicationSettings = new List<ApplicationSetting>() { new ApplicationSetting() {Key = "unrelatedKey", Value = "value" } };
            _applicationSettingsRepository
                .Setup(repo => repo.GetSettings())
                .ReturnsAsync(applicationSettings);

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ResponseMessageResult));
            Assert.AreEqual(HttpStatusCode.Conflict, ((ResponseMessageResult)result).Response.StatusCode);
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_CreatingRecoveryTokenFails()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            _usersRepoMock
                .Setup(repo => repo.UpdatePasswordRecoveryTokensAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .Throws(new Exception("any"));

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
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
                .Setup(repo => repo.UpdatePasswordRecoveryTokensAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult<object>(null));

            _emailHelperMock
                .Setup(helper => helper.Initialize(It.IsAny<IEmailConfigInstanceSettings>()));

            _emailHelperMock
                .Setup(helper => helper.SendEmail(It.IsAny<AuthenticationUser>()));

            _authRepoMock
                .Setup(repo => repo.IsChangePasswordCooldownInEffect(It.IsAny<AuthenticationUser>()))
                .ReturnsAsync(false);

            IEnumerable<ApplicationSetting> applicationSettings = new List<ApplicationSetting>(){ new ApplicationSetting() { Key = "IsPasswordRecoveryEnabled", Value = "true" } };
            _applicationSettingsRepository
                .Setup(repo => repo.GetSettings())
                .ReturnsAsync(applicationSettings);
        }

        #endregion PasswordRecovery

    }
}
