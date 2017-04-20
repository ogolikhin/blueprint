using System;
using System.Collections.Generic;
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
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

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
        private Mock<IHttpClientProvider> _httpClientProviderMock;
        private Mock<ISqlPrivilegesRepository> _privilegesRepository;
        private CreationUserDto _user;

        private const int FullPermissions = 524287;
        private const int NoManageUsersPermissions = 13312;
        private const int NoAssignAdminRolesPermissions = 15360;
        private const int CreatedUserId = 100;
        private const string ExistedUserLogin = "ExistedUser";

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
            _privilegesRepository = new Mock<ISqlPrivilegesRepository>();
            _controller = new UsersController(_authRepoMock.Object, _usersRepoMock.Object, _settingsRepoMock.Object,
                _emailHelperMock.Object, _applicationSettingsRepository.Object, _logMock.Object, _httpClientProviderMock.Object, _privilegesRepository.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            _controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            _controller.Request.RequestUri = new Uri("http://localhost");

            _user = new CreationUserDto()
            {
                Login = "UserLogin",
                FirstName = "FirstNameValue",
                LastName = "LastNameValue",
                DisplayName = "DisplayNameValue",
                Email = "email@test.com",
                Source = UserGroupSource.Database,
                AllowFallback = false,
                Enabled = true,
                ExpirePassword = true,
                NewPassword  = "MTIzNFJFV1EhQCMk",
                Title = "TitleValue",
                Department = "Departmentvalue",
                GroupMembership = new int[] { 1 },
                Guest = false
            };
            _usersRepoMock
                .Setup(repo => repo.AddUserAsync(It.Is<User>(u => u.Login != ExistedUserLogin)))
                .ReturnsAsync(CreatedUserId);

            var badRequestException = new BadRequestException(ErrorMessages.LoginNameUnique);
            _usersRepoMock
                .Setup(repo => repo.AddUserAsync(It.Is<User>(u => u.Login == ExistedUserLogin)))
                .ThrowsAsync(badRequestException);
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
            var content = new byte[] { 0x20, 0x20, 0x20, 0x20 };
            var userIcon = new UserIcon { UserId = userId, Content = content };
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
                await _controller.PostReset(SystemEncryptions.EncodeTo64UTF8("admin"), new ResetPostContent { NewPass = newPass, OldPass = oldPass });
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
            IEnumerable<ApplicationSetting> applicationSettings = new List<ApplicationSetting> { new ApplicationSetting() { Key = "PasswordResetTokenExpirationInHours", Value = "40" } };
            _applicationSettingsRepository
                .Setup(repo => repo.GetSettings())
                .ReturnsAsync(applicationSettings);


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
            _applicationSettingsRepository
                .Setup(repo => repo.GetValue(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(24);


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
            Assert.AreEqual(ErrorCodes.PasswordResetUserNotFound, ((ConflictException)exception).ErrorCode);
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
                .ReturnsAsync(new AuthenticationUser { IsEnabled = false });
            _applicationSettingsRepository
                .Setup(repo => repo.GetValue(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(24);


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
        public async Task PostPasswordReset_SamePassword_ReturnsBadRequest()
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
                .ReturnsAsync(new AuthenticationUser
                {
                    IsEnabled = true,
                    UserSalt = new Guid("1021420F-12D9-4D9F-9B47-F07BD7DE8D2F"),
                    Password = "Dmg+JJ/DtmxEHNi5cpk9+IIYZi4FttQO5YHuddfcuvQ="
                });
            _applicationSettingsRepository
                .Setup(repo => repo.GetValue(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(24);


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
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
            Assert.AreEqual(ErrorCodes.SamePassword, ((BadRequestException)exception).ErrorCode);
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
                .ReturnsAsync(new AuthenticationUser { Id = uid, IsEnabled = true });
            _authRepoMock
                .Setup(a => a.ResetPassword(It.IsAny<AuthenticationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            var httpClientProvider = new TestHttpClientProvider(request => request.RequestUri.AbsolutePath.EndsWithOrdinal($"sessions/{uid}") ?
                     new HttpResponseMessage(HttpStatusCode.OK) : null);
            _applicationSettingsRepository
                .Setup(repo => repo.GetValue(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(24);

            _controller = new UsersController(_authRepoMock.Object, _usersRepoMock.Object, _settingsRepoMock.Object,
                _emailHelperMock.Object, _applicationSettingsRepository.Object, _logMock.Object, httpClientProvider, _privilegesRepository.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            // Act
            IHttpActionResult result = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
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
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
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
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
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
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
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
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_PasswordRecoveryDisabled()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            _applicationSettingsRepository
                .Setup(repo => repo.GetValue("IsPasswordRecoveryEnabled", It.IsAny<bool>()))
                .ReturnsAsync(false);

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_PasswordRecoveryEntryMissingFromDatabase()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            _applicationSettingsRepository
                .Setup(repo => repo.GetValue("IsPasswordRecoveryEnabled", It.IsAny<bool>()))
                .ReturnsAsync(false);

            // Act
            IHttpActionResult result = null;
            result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
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
                .Setup(helper => helper.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _authRepoMock
                .Setup(repo => repo.IsChangePasswordCooldownInEffect(It.IsAny<AuthenticationUser>()))
                .ReturnsAsync(false);

            _applicationSettingsRepository
                .Setup(repo => repo.GetValue("IsPasswordRecoveryEnabled", It.IsAny<bool>()))
                .ReturnsAsync(true);

        }

        #endregion PasswordRecovery

        #region GetAllUsers

        [TestMethod]
        public async Task GetAllUsers_AllParamsAreCorrect_RepositoryReturnUsers()
        {
            //arrange
            var sort = string.Empty;
            var filter = string.Empty;

            var users = new List<UserDto>() { new UserDto() { UserId = 1 } };
            var settings = new TableSettings() { PageSize = 3, Page = 1 };
            QueryResult returnResult = new QueryResult()
            {
                Data = new Data(),
                Pagination = new Pagination()
                {
                    Page = settings.Page,
                    PageSize = settings.PageSize
                }
            };
            returnResult.Data.Users = users.ToArray();

            returnResult.Data.Users = users.ToArray();

            _usersRepoMock.Setup(repo => repo.GetUsers(It.Is<TableSettings>(t => t.PageSize > 0 && t.Page > 0)))
                .Returns(returnResult);
            _privilegesRepository.Setup(t => t.IsUserHasPermissions(new[] { 1024 }, It.IsAny<int>())).ReturnsAsync(true);

            //act
            var result = await _controller.GetAllUsers(settings.Page, settings.PageSize, filter, sort) as OkNegotiatedContentResult<QueryResult>;

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Content, typeof(QueryResult));
        }

        [TestMethod]
        public async Task GetAllUsers_ParamsAreNotCorrect_BadRequestResult()
        {
            //arrange

            //act
            var result = await _controller.GetAllUsers(0, 0, string.Empty, string.Empty);

            //assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task GetAllUsers_UserDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            //arrange
            Exception exception = null;
            _privilegesRepository.Setup(t => t.IsUserHasPermissions(new[] { 1024 }, It.IsAny<int>())).ReturnsAsync(false);

            //act
            try
            {
                var result = await _controller.GetAllUsers(1, 2, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsInstanceOfType(exception, typeof(HttpResponseException));
            Assert.AreEqual(HttpStatusCode.Forbidden, ((HttpResponseException)exception).Response.StatusCode);
        }
        #endregion

        #region GetUser

        [TestMethod]
        public async Task GetUser_AllParamsAreCorrectAndPermissionsOk_RepositoryReturnUser()
        {
            //arrange
            var user = new User() { UserId = 5 };
            _usersRepoMock.Setup(repo => repo.GetUser(It.Is<int>(i => i > 0))).ReturnsAsync(user);
            _privilegesRepository.Setup(t => t.IsUserHasPermissions(new[] { 1024 }, It.IsAny<int>())).ReturnsAsync(true);

            //act
            var result = await _controller.GetUser(5) as OkNegotiatedContentResult<User>;

            //assert
            Assert.AreEqual(user, result.Content);
        }

        [TestMethod]
        public async Task GetUser_UserWithInvalidPermissions_ForbiddenResult()
        {
            //arrange
            Exception exception = null;
            _privilegesRepository.Setup(t => t.IsUserHasPermissions(new[] { 1024 }, It.IsAny<int>())).ReturnsAsync(false);

            //act
            try
            {
                var result = await _controller.GetUser(0);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsInstanceOfType(exception, typeof(HttpResponseException));
            Assert.AreEqual(HttpStatusCode.Forbidden, ((HttpResponseException)exception).Response.StatusCode);
        }

        [TestMethod]
        public async Task GetUser_ThereIsNoSuchUser_NotFoundResult()
        {
            //arrange
            var user = new User();
            _usersRepoMock.Setup(repo => repo.GetUser(It.Is<int>(i => i > 0))).ReturnsAsync(user);
            _privilegesRepository.Setup(t => t.IsUserHasPermissions(new[] { 1024 }, It.IsAny<int>())).ReturnsAsync(true);

            //act
            var result = await _controller.GetUser(1) as NotFoundResult;

            //assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));

        }
        #endregion

        #region Post user
        [TestMethod]
        public async Task PostUser_SuccessfulCreationOfUser_ReturnCreatedUserIdResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
                .ReturnsAsync(FullPermissions);

            // Act
            var result = await _controller.PostUser(_user);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.AreEqual(CreatedUserId, await result.Content.ReadAsAsync<int>());
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task PostUser_NoManageUsersPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
                .ReturnsAsync(NoManageUsersPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task PostUser_NoAssignAdminRolesPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _user.InstanceAdminRoleId = 1;
            _privilegesRepository
                .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
                .ReturnsAsync(NoAssignAdminRolesPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_UserLoginEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = string.Empty;
            _privilegesRepository
                .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
                .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_UserLoginOutOfRangeLengthString_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = "123";
            _privilegesRepository
               .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
               .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_UserLoginAlreadyExist_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = ExistedUserLogin;
            _privilegesRepository
               .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
               .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_DisplayNameEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _user.DisplayName = string.Empty;
            _privilegesRepository
              .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
              .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_DisplayNameOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.DisplayName = "1";
            _privilegesRepository
              .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
              .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_FirstNameEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _user.FirstName = string.Empty;
            _privilegesRepository
              .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
              .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_FirstNameOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.FirstName = "1";
            _privilegesRepository
              .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
              .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_LastNameEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _user.LastName = string.Empty;
            _privilegesRepository
              .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
              .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_LastNameOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.LastName = "1";
            _privilegesRepository
              .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
              .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_EmailOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.Email = "1@1";
            _privilegesRepository
             .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
             .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_TitleOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.Title = "1";
            _privilegesRepository
             .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
             .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_DepartmentOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            for (var i = 0; i < 258; i++)
            {
                _user.Department += i;
            }
            _privilegesRepository
              .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
              .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_PasswordEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _user.NewPassword = string.Empty;
            _privilegesRepository
               .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
               .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PostUser_PasswordContainsOnlyAlphanumericCharacters_ReturnBadRequestResult()
        {
            // Arrange
            _user.NewPassword = "MTIzNDU2Nzg=";
            _privilegesRepository
               .Setup(repo => repo.GetUserPermissionsAsync(It.IsAny<int>()))
               .ReturnsAsync(FullPermissions);

            // Act
            await _controller.PostUser(_user);

            // Assert
            // Exception
        }
        #endregion
    }
}
