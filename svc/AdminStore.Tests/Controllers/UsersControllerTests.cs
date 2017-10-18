using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace AdminStore.Controllers
{
    [TestClass]
    public class UsersControllerTests
    {
        private Mock<IUserRepository> _usersRepoMock;
        private Mock<IServiceLogRepository> _logMock;
        private Mock<IAuthenticationRepository> _authRepoMock;
        private Mock<ISqlSettingsRepository> _settingsRepoMock;
        private Mock<IEmailHelper> _emailHelperMock;
        private Mock<IApplicationSettingsRepository> _applicationSettingsRepository;
        private Mock<IHttpClientProvider> _httpClientProviderMock;
        private Mock<IPrivilegesRepository> _privilegesRepository;
        private UsersController _controller;
        private UserDto _user;

        private const InstanceAdminPrivileges FullPermissions = InstanceAdminPrivileges.AssignAdminRoles;
        private const InstanceAdminPrivileges NoManageUsersPermissions = InstanceAdminPrivileges.ViewUsers;
        private const InstanceAdminPrivileges NoAssignAdminRolesPermissions = InstanceAdminPrivileges.ManageUsers;
        private const int SessionUserId = 1;
        private const int UserId = 100;
        private const string ExistedUserLogin = "ExistedUser";
        private QueryResult<GroupDto> _userGoupsQueryDataResult;
        private Pagination _userGroupsTabularPagination;
        private Sorting _userGroupsSorting;
        private OperationScope _operationScope;

        [TestInitialize]
        public void Initialize()
        {
            var session = new Session { UserId = SessionUserId };
            _usersRepoMock = new Mock<IUserRepository>();
            _logMock = new Mock<IServiceLogRepository>();
            _authRepoMock = new Mock<IAuthenticationRepository>();
            _settingsRepoMock = new Mock<ISqlSettingsRepository>();
            _emailHelperMock = new Mock<IEmailHelper>();
            _httpClientProviderMock = new Mock<IHttpClientProvider>();
            _applicationSettingsRepository = new Mock<IApplicationSettingsRepository>();
            _privilegesRepository = new Mock<IPrivilegesRepository>();

            _controller = new UsersController(
                _authRepoMock.Object, _usersRepoMock.Object, _settingsRepoMock.Object,
                _emailHelperMock.Object, _applicationSettingsRepository.Object, _logMock.Object,
                _httpClientProviderMock.Object, _privilegesRepository.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            _controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            _controller.Request.RequestUri = new Uri("http://localhost");

            _user = new UserDto
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
                Password = "MTIzNFJFV1EhQCMk",
                Title = "TitleValue",
                Department = "Departmentvalue",
                GroupMembership = new[] { 1 },
                Guest = false
            };

            _usersRepoMock
                .Setup(repo => repo.AddUserAsync(It.Is<User>(u => u.Login != ExistedUserLogin)))
                .ReturnsAsync(UserId);

            var badRequestException = new BadRequestException(ErrorMessages.LoginNameUnique);
            _usersRepoMock
                .Setup(repo => repo.AddUserAsync(It.Is<User>(u => u.Login == ExistedUserLogin)))
                .ThrowsAsync(badRequestException);

            _userGroupsTabularPagination = new Pagination { Limit = 1, Offset = 0 };
            _userGroupsSorting = new Sorting { Order = SortOrder.Asc, Sort = "Name" };
            _userGoupsQueryDataResult = new QueryResult<GroupDto> { Total = 1, Items = new List<GroupDto>() };
            _operationScope = new OperationScope { Ids = new[] { 3, 4 } };
        }

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
            const int userId = 1;
            _usersRepoMock
                .Setup(repo => repo.GetUserIconByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync((UserIcon)null);

            // Act
            try
            {
                await _controller.GetUserIcon(userId);
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
                .ReturnsAsync((LoginUser)null);

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

        [TestMethod]
        public async Task GetLoginUser_CheckAdminRoleRepositoryReturnsTrue_ReturnsUser()
        {
            // Arrange
            var loginUser = new LoginUser { IsProjectAdmin = false };

            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(loginUser);
            _usersRepoMock
                .Setup(repo => repo.CheckUserHasProjectAdminRoleAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.GetLoginUser() as OkNegotiatedContentResult<LoginUser>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(loginUser, result.Content);
            Assert.AreEqual(loginUser.IsProjectAdmin, result.Content.IsProjectAdmin);
            Assert.IsTrue(result.Content.IsProjectAdmin);
        }

        [TestMethod]
        public async Task GetLoginUser_CheckAdminRoleRepositoryWasNotHandled_ReturnsUser()
        {
            // Arrange
            var loginUser = new LoginUser { InstanceAdminRoleId = 1 };

            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(loginUser);
            _usersRepoMock
                .Setup(repo => repo.CheckUserHasProjectAdminRoleAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.GetLoginUser() as OkNegotiatedContentResult<LoginUser>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(loginUser, result.Content);
            Assert.AreEqual(loginUser.IsProjectAdmin, result.Content.IsProjectAdmin);
            Assert.IsTrue(result.Content.IsProjectAdmin);
            _usersRepoMock.Verify(x => x.CheckUserHasProjectAdminRoleAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task GetLoginUser_CheckAdminRoleRepositoryThrowsException_InternalServerErrorResult()
        {
            // Arrange
            var loginUser = new LoginUser { IsProjectAdmin = false };

            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(loginUser);
            _usersRepoMock
                .Setup(repo => repo.CheckUserHasProjectAdminRoleAsync(It.IsAny<int>()))
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
        public async Task PostReset_InvalidLogin_ReturnsBadRequest()
        {
            // Arrange
            Exception exception = null;
            var newPass = SystemEncryptions.EncodeTo64UTF8("123EWQ!@#");
            var oldPass = SystemEncryptions.EncodeTo64UTF8("changeme");

            // Act
            try
            {
                await _controller.PostReset("ZAP%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s\n", 
                    new ResetPostContent { NewPass = newPass, OldPass = oldPass });
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
        }

        [TestMethod]
        public async Task PostReset_InvalidOldPass_ReturnsBadRequest()
        {
            // Arrange
            Exception exception = null;
            var newPass = SystemEncryptions.EncodeTo64UTF8("123EWQ!@#");

            // Act
            try
            {
                await _controller.PostReset(SystemEncryptions.EncodeTo64UTF8("admin"), 
                    new ResetPostContent { NewPass = newPass, OldPass = "ZAP%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s\n" });
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
        }

        [TestMethod]
        public async Task PostReset_InvalidNewPass_ReturnsBadRequest()
        {
            // Arrange
            Exception exception = null;
            var oldPass = SystemEncryptions.EncodeTo64UTF8("changeme");

            // Act
            try
            {
                await _controller.PostReset(SystemEncryptions.EncodeTo64UTF8("admin"), 
                    new ResetPostContent { NewPass = "ZAP%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s%n%s\n", OldPass = oldPass });
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
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

            // This token simulates a token generated by the controller when no token is provided
            var emptyInputToken = new Guid("00000000-0000-0000-0000-000000000000");

            // Act
            IHttpActionResult result = null;
            BadRequestException exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = emptyInputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.PasswordResetEmptyToken, exception.ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_TokenListEmpty_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<PasswordRecoveryToken>());

            // Act
            IHttpActionResult result = null;
            ConflictException exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (ConflictException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.PasswordResetTokenNotFound, exception.ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_TokenNotMostRecent_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf") },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-20),
                    Login = "testUser", RecoveryToken = inputToken },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4") }
            };
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(tokenList);


            // Act
            IHttpActionResult result = null;
            ConflictException exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (ConflictException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.PasswordResetTokenNotLatest, exception.ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_ExpiredToken_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-200),
                    Login = "testUser", RecoveryToken = inputToken },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf") },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4") }
            };
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(tokenList);
            IEnumerable<ApplicationSetting> applicationSettings = new List<ApplicationSetting> { new ApplicationSetting { Key = "PasswordResetTokenExpirationInHours", Value = "40" } };
            _applicationSettingsRepository
                .Setup(repo => repo.GetSettingsAsync(false))
                .ReturnsAsync(applicationSettings);


            // Act
            IHttpActionResult result = null;
            ConflictException exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (ConflictException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.PasswordResetTokenExpired, exception.ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_NullUser_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-2),
                    Login = "testUser", RecoveryToken = inputToken },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf") },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4") }
            };
            _usersRepoMock
                .Setup(repo => repo.GetPasswordRecoveryTokensAsync(It.IsAny<Guid>()))
                .ReturnsAsync(tokenList);
            _usersRepoMock
                .Setup(repo => repo.GetUserByLoginAsync(It.IsAny<string>()))
                .ReturnsAsync((AuthenticationUser)null);
            _applicationSettingsRepository
                .Setup(repo => repo.GetValue(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(24);


            // Act
            IHttpActionResult result = null;
            ConflictException exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (ConflictException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.PasswordResetUserNotFound, exception.ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_DisabledUser_ReturnsConflict()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-2),
                    Login = "testUser", RecoveryToken = inputToken },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf") },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4") }
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
            ConflictException exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (ConflictException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.PasswordResetUserDisabled, exception.ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_SamePassword_ReturnsBadRequest()
        {
            // Arrange
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-2),
                    Login = "testUser", RecoveryToken = inputToken },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf") },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4") }
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
            BadRequestException exception = null;
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            try
            {
                result = await _controller.PostPasswordResetAsync(resetContent);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.SamePassword, exception.ErrorCode);
        }

        [TestMethod]
        public async Task PostPasswordReset_RepositoriesReturnsSuccessfully()
        {
            // Arrange
            var uid = 3;
            var inputToken = new Guid("e6b99f56-f2ff-49e8-85e1-4349a56271b9");
            var tokenList = new List<PasswordRecoveryToken>
            {
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-2),
                    Login = "testUser", RecoveryToken = inputToken },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-3),
                    Login = "testUser", RecoveryToken = new Guid("b76c7bf9-3a70-409b-b017-92dc056524cf") },
                new PasswordRecoveryToken { CreationTime = DateTime.Now.AddHours(-40),
                    Login = "testUser", RecoveryToken = new Guid("fb131adc-2be4-43a9-9d49-0c94313a23a4") }
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
            var resetContent = new ResetPasswordContent { Password = "MTIzNFJFV1EhQCMk", Token = inputToken };
            var result = await _controller.PostPasswordResetAsync(resetContent);

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
            var result = await _controller.PostRequestPasswordResetAsync("login");

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
            var result = await _controller.PostRequestPasswordResetAsync("login");

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
            var result = await _controller.PostRequestPasswordResetAsync("login");

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
            var result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
        }

        [TestMethod]
        public async Task PostRequestPasswordReset_InstanceEmailIsNotSetUp()
        {
            // Arrange
            SetupMocksForRequestPasswordReset();

            var instanceSettings = new InstanceSettings { EmailSettingsDeserialized = null };

            _settingsRepoMock
                .Setup(repo => repo.GetInstanceSettingsAsync())
                .ReturnsAsync(instanceSettings);

            // Act
            var result = await _controller.PostRequestPasswordResetAsync("login");

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
            var result = await _controller.PostRequestPasswordResetAsync("login");

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
            var result = await _controller.PostRequestPasswordResetAsync("login");

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
            var result = await _controller.PostRequestPasswordResetAsync("login");

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        private void SetupMocksForRequestPasswordReset()
        {
            var emailConfigSettings = new Mock<IEmailConfigInstanceSettings>();
            emailConfigSettings
                .SetupGet(s => s.HostName)
                .Returns("http://myhostname");

            var instanceSettings = new InstanceSettings { EmailSettingsDeserialized = emailConfigSettings.Object };

            _settingsRepoMock
                .Setup(repo => repo.GetInstanceSettingsAsync())
                .ReturnsAsync(instanceSettings);

            _usersRepoMock
                .Setup(repo => repo.GetUserByLoginAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationUser { Email = "a@b.com" });

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

        #region GetUsers

        [TestMethod]
        public async Task GetUsers_NoRequiredPermissions_ForbiddenResult()
        {
            // arrange
            AuthorizationException exception = null;
            var pagination = new Pagination { Offset = 0, Limit = 20 };
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);

            // act
            try
            {
                await _controller.GetUsers(pagination);
            }
            catch (AuthorizationException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public async Task GetUsers_AllRequirementsSatisfied_ReturnsUsers()
        {
            // arrange
            var pagination = new Pagination { Offset = 0, Limit = 20 };
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _usersRepoMock
                .Setup(r => r.GetUsersAsync(pagination, It.IsAny<Sorting>(), It.IsAny<string>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(QueryResult<UserDto>.Empty);

            // act
            var result = await _controller.GetUsers(pagination) as OkNegotiatedContentResult<QueryResult<UserDto>>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Content.Total);
            Assert.AreEqual(0, result.Content.Items.Count());
        }

        #endregion

        #region GetUser

        [TestMethod]
        public async Task GetUser_AllParamsAreCorrectAndPermissionsOk_RepositoryReturnUser()
        {
            // arrange
            const int userId = 5;
            var user = new UserDto { Id = userId };
            _usersRepoMock.Setup(repo => repo.GetUserDtoAsync(It.Is<int>(i => i == userId))).ReturnsAsync(user);
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            // act
            var result = await _controller.GetUser(userId) as OkNegotiatedContentResult<UserDto>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user, result.Content);
        }

        [TestMethod]
        public async Task GetUser_UserWithInvalidPermissions_ForbiddenResult()
        {
            // arrange
            Exception exception = null;
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);

            // act
            try
            {
                await _controller.GetUser(0);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        [TestMethod]
        public async Task GetUser_ThereIsNoSuchUser_NotFoundResult()
        {
            // arrange
            const int userId = 1;
            ResourceNotFoundException exception = null;
            _usersRepoMock
                .Setup(repo => repo.GetUserDtoAsync(It.Is<int>(i => i == userId)))
                .ReturnsAsync((UserDto)null);
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            // act
            try
            {
                await _controller.GetUser(userId);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.UserNotExist, exception.Message);
        }

        #endregion

        #region Create User

        [TestMethod]
        public async Task CreateUser_SuccessfulCreationOfUser_ReturnCreatedUserIdResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings());

            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.AreEqual(UserId, await result.Content.ReadAsAsync<int>());
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task CreateUser_NoManageUsersPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(NoManageUsersPermissions);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task CreateUser_NoAssignAdminRolesPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _user.InstanceAdminRoleId = 1;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(NoAssignAdminRolesPermissions);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_UserLoginEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = string.Empty;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task CreateUser_UserLoginContainsInvalidCharacters_ReturnsBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _user.Login = "abcырyz";
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);

            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, ErrorMessages.LoginInvalid);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task CreateUser_UserLoginContainsValidCharacters_ReturnsOk()
        {
            // Arrange
            _user.Login = "test-user!test_@user";
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(HttpResponseMessage));
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_UserLoginOutOfRangeLengthString_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = "123";
            _privilegesRepository
               .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_UserLoginAlreadyExist_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = ExistedUserLogin;
            _privilegesRepository
               .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings());

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task CreateUser_UserWithExpiredUserKeyLogin_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = ServiceConstants.ExpiredUserKey;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings());
            BadRequestException exception = null;
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, ErrorMessages.LoginInvalid);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task CreateUser_UserWithUserLogoutLogin_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = ServiceConstants.UserLogout;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            BadRequestException exception = null;

            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, ErrorMessages.LoginInvalid);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task CreateUser_UserWithInvalidUserKeyLogin_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = ServiceConstants.InvalidUserKey;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            BadRequestException exception = null;

            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, ErrorMessages.LoginInvalid);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_DisplayNameEmpty_ReturnBadRequestResult()
        {
            // Arrange
            _user.DisplayName = string.Empty;
            _privilegesRepository
              .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
              .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_DisplayNameOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.DisplayName = new string('1', 256);
            _privilegesRepository
              .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
              .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_FirstNameOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.FirstName = new string('1', 256);
            _privilegesRepository
              .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
              .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_LastNameOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.LastName = new string('1', 256);
            _privilegesRepository
              .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
              .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_EmailOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.Email = "1@1";
            _privilegesRepository
             .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
             .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_TitleOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            _user.Title = new string('1', 256);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _privilegesRepository
             .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
             .ReturnsAsync(FullPermissions);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_DepartmentOutOfRangeStringLength_ReturnBadRequestResult()
        {
            // Arrange
            for (var i = 0; i < 258; i++)
            {
                _user.Department += i;
            }
            _privilegesRepository
              .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
              .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task CreateUser_MaxUsersLimitPerInstanceWasReached_ReturnErrorResult()
        {
            // arrange
            _user.Login = "test-user!test_@user";
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);

            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(false);


            // act
            await _controller.CreateUser(_user);

            // assert
            // Exception
        }   

        #region Password

        [TestMethod]
        public async Task CreateUser_PasswordIsNull_AllowFallbackIsNull_FederatedAuthenticationIsDisabled_ReturnBadRequestResult()
        {
            // Arrange
            _user.Password = null;
            _user.AllowFallback = null;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = false });
            BadRequestException exception = null;

            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsProvided_AllowFallbackIsNull_FederatedAuthenticationIsDisabled_ReturnCreatedResult()
        {
            // Arrange
            _user.AllowFallback = null;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = false });

            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.IsNotNull(result.Content);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsNull_AllowFallbackIsFalse_FederatedAuthenticationIsDisabled_ReturnBadRequestResult()
        {
            // Arrange
            _user.Password = null;
            _user.AllowFallback = false;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = false });
            BadRequestException exception = null;

            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsProvided_AllowFallbackIsFalse_FederatedAuthenticationIsDisabled_ReturnCreatedResult()
        {
            // Arrange
            _user.AllowFallback = false;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = false });

            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.IsNotNull(result.Content);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsNull_AllowFallbackIsTrue_FederatedAuthenticationIsDisabled_ReturnBadRequestResult()
        {
            // Arrange
            _user.Password = null;
            _user.AllowFallback = true;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = false });
            BadRequestException exception = null;

            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsProvided_AllowFallbackIsTrue_FederatedAuthenticationIsDisabled_ReturnCreatedResult()
        {
            // Arrange
            _user.AllowFallback = true;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = false });

            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.IsNotNull(result.Content);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsNull_AllowFallbackIsNull_FederatedAuthenticationIsEnabled_ReturnCreatedResult()
        {
            // Arrange
            _user.Password = null;
            _user.AllowFallback = null;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = true });

            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.IsNotNull(result.Content);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsProvided_AllowFallbackIsNull_FederatedAuthenticationIsEnabled_ReturnCreatedResult()
        {
            // Arrange
            _user.AllowFallback = null;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = true });

            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.IsNotNull(result.Content);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsNull_AllowFallbackIsFalse_FederatedAuthenticationIsEnabled_ReturnCreatedResult()
        {
            // Arrange
            _user.Password = null;
            _user.AllowFallback = false;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = true });

            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.IsNotNull(result.Content);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsProvided_AllowFallbackIsFalse_FederatedAuthenticationIsEnabled_ReturnCreatedResult()
        {
            // Arrange
            _user.AllowFallback = false;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = true });

            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.IsNotNull(result.Content);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsNull_AllowFallbackIsTrue_FederatedAuthenticationIsEnabled_ReturnBadRequestResult()
        {
            // Arrange
            _user.Password = null;
            _user.AllowFallback = true;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = true });
            BadRequestException exception = null;

            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task CreateUser_PasswordIsProvided_AllowFallbackIsTrue_FederatedAuthenticationIsEnabled_ReturnCreatedResult()
        {
            // Arrange
            _user.AllowFallback = true;
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { IsFederatedAuthenticationEnabled = true });

            // Act
            var result = await _controller.CreateUser(_user);

            // Assert
            Assert.IsNotNull(result.Content);
        }

        [TestMethod]
        public async Task CreateUser_PasswordSameAsLogin_ReturnBadRequestResult()
        {
            // Arrange
            _user.Login = "RobertJordan_1!";
            _user.Password = SystemEncryptions.EncodeTo64UTF8(_user.Login);
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings());
            BadRequestException exception = null;

            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.PasswordSameAsLogin);
        }

        [TestMethod]
        public async Task CreateUser_PasswordSameAsDisplayName_ReturnBadRequestResult()
        {
            // Arrange
            _user.DisplayName = "RobertJordan_1!";
            _user.Password = SystemEncryptions.EncodeTo64UTF8(_user.DisplayName);
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings());
            BadRequestException exception = null;

            // Act
            try
            {
                await _controller.CreateUser(_user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.PasswordSameAsDisplayName);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_PasswordContainsOnlyAlphanumericCharacters_ReturnBadRequestResult()
        {
            // Arrange
            _user.Password = "MTIzNDU2Nzg=";
            _privilegesRepository
               .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings());

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_PasswordNotBase64String_ReturnBadRequestResult()
        {
            // Arrange
            _user.Password = "11111";
            _privilegesRepository
               .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);
            _settingsRepoMock.Setup(r => r.GetUserManagementSettingsAsync())
                .ReturnsAsync(new UserManagementSettings());

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        #endregion Password

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_EmailWithoutAtSymbol_ReturnBadRequestResult()
        {
            // Arrange
            _user.Email = "testemail.com";
            _privilegesRepository
               .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task CreateUser_EmailWithMultipleAtSymbols_ReturnBadRequestResult()
        {
            // Arrange
            _user.Email = "sp@rk@email.com";
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            _usersRepoMock.Setup(c => c.CheckIfAdminCanCreateUsers()).ReturnsAsync(true);

            // Act
            await _controller.CreateUser(_user);

            // Assert
            // Exception
        }

        #endregion Create User

        #region Update user

        [TestMethod]
        public async Task UpdateUser_AllRequirementsSatisfied_ReturnOkResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            var existingUser = new User { Id = UserId, InstanceAdminRoleId = null };
            _usersRepoMock
                .Setup(r => r.GetUserAsync(UserId))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _controller.UpdateUser(UserId, _user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateUser_UserNotFound_ReturnNotFoundErrorResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            var existingUser = new User { Id = UserId, InstanceAdminRoleId = null };
            _usersRepoMock
                .Setup(r => r.GetUserAsync(UserId))
                .ReturnsAsync(existingUser);

            var resourceNotFoundExeption = new ResourceNotFoundException(ErrorMessages.UserNotExist);
            _usersRepoMock.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>())).Throws(resourceNotFoundExeption);

            // Act
            await _controller.UpdateUser(UserId, _user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UpdateUser_UserHasDifferentVersion_ReturnConflicErrorResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            var existingUser = new User { Id = UserId, InstanceAdminRoleId = null };
            _usersRepoMock
                .Setup(r => r.GetUserAsync(UserId))
                .ReturnsAsync(existingUser);

            var conflictExeption = new ConflictException(ErrorMessages.UserVersionsNotEqual);
            _usersRepoMock.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>())).Throws(conflictExeption);

            // Act
            await _controller.UpdateUser(UserId, _user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateUser_UserIdNotPassed_ReturnBadRequestErrorResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);

            // Act
            await _controller.UpdateUser(0, _user);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateUser_UserModelIsEmpty_ReturnBadRequestErrorResult()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);

            // Act
            await _controller.UpdateUser(UserId, null);

            // Assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UpdateUser_ChangeInInstanceRolePrivilege_WithoutAssignInstanceRolePrivilege_ThrowsAuthenticationException()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            var existingUser = new User { Id = UserId, InstanceAdminRoleId = null };
            _usersRepoMock
                .Setup(r => r.GetUserAsync(UserId))
                .ReturnsAsync(existingUser);

            _user.InstanceAdminRoleId = 1;

            // Act
            await _controller.UpdateUser(UserId, _user);
        }

        [TestMethod]
        public async Task UpdateUser_ChangeInInstanceRolePrivilege_WithAssignInstanceRolePrivilege_ReturnsOk()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AssignAdminRoles);
            var existingUser = new User { Id = UserId, InstanceAdminRoleId = null };
            _usersRepoMock
                .Setup(r => r.GetUserAsync(UserId))
                .ReturnsAsync(existingUser);

            _user.InstanceAdminRoleId = 1;

            // Act
            var result = await _controller.UpdateUser(UserId, _user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task UpdateUser_ChangeInNonInstanceRolePrivilege_WithoutAssignInstanceRolePrivilege_ReturnsOk()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            var existingUser = new User { Id = UserId, InstanceAdminRoleId = 1 };
            _usersRepoMock
                .Setup(r => r.GetUserAsync(UserId))
                .ReturnsAsync(existingUser);

            _user.DisplayName = "New DisplayName";
            _user.InstanceAdminRoleId = 1;

            // Act
            var result = await _controller.UpdateUser(UserId, _user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task UpdateUser_UserLoginContainsInvalidCharacters_ReturnsBadRequestResult()
        {
            // Arrange
            BadRequestException exception = null;
            _user.Login = "abcырyz";
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            var existingUser = new User { Id = UserId, Login = "test_user" };
            _usersRepoMock
                .Setup(r => r.GetUserAsync(UserId))
                .ReturnsAsync(existingUser);

            // Act
            try
            {
                await _controller.UpdateUser(UserId, _user);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, ErrorMessages.LoginInvalid);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task UpdateUser_UserLoginContainsValidCharacters_ReturnsOk()
        {
            // Arrange
            _user.Login = "test-user!test_@user";
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(FullPermissions);
            var existingUser = new User { Id = UserId, Login = "test_user" };
            _usersRepoMock
                .Setup(r => r.GetUserAsync(UserId))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _controller.UpdateUser(UserId, _user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        #endregion

        #region GetUserGroups

        [TestMethod]
        public async Task GetUserGroups_UserDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            AuthorizationException exception = null;
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);
            _usersRepoMock
                .Setup(repo => repo.GetUserGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(_userGoupsQueryDataResult);

            // act
            try
            {
                await _controller.GetUserGroups(UserId, _userGroupsTabularPagination, _userGroupsSorting, string.Empty);
            }
            catch (AuthorizationException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public async Task GetUserGroups_UserNotFound_ResourceNotFoundResult()
        {
            // arrange
            ResourceNotFoundException exception = null;
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _usersRepoMock.Setup(repo => repo.GetUserGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>()))
                .ThrowsAsync(new ResourceNotFoundException(ErrorMessages.UserNotExist, ErrorCodes.ResourceNotFound));

            // act
            try
            {
                await _controller.GetUserGroups(UserId, _userGroupsTabularPagination, _userGroupsSorting, string.Empty);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.UserNotExist, exception.Message);
            Assert.AreEqual(ErrorCodes.ResourceNotFound, exception.ErrorCode);
        }

        [TestMethod]
        public async Task GetUserGroups_AllRequirementsSatisfied_ReturnsUserGroups()
        {
            // arrange
            _privilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);
            _usersRepoMock
                .Setup(repo => repo.GetUserGroupsAsync(It.IsAny<int>(), It.IsAny<TabularData>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(_userGoupsQueryDataResult);

            // act
            var result = await _controller.GetUserGroups(UserId, _userGroupsTabularPagination, _userGroupsSorting, string.Empty) as OkNegotiatedContentResult<QueryResult<GroupDto>>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Content, typeof(QueryResult<GroupDto>));
        }

        #endregion

        #region Deletete users

        [TestMethod]
        public async Task DeleteUsers_OperationScopIsNull_BadRequestResult()
        {
            // arrange
            _privilegesRepository
                .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);

            // act
            var result = await _controller.DeleteUsers(null, string.Empty);

            // assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }
        #endregion

        #region InstanceAdminChangePassword

        [TestMethod]
        public async Task InstanceAdminChangePassword_BodyIsNull_BadRequestResult()
        {
            // arrange
            UpdateUserPassword updatePasswor = null;
            IHttpActionResult result = null;
            BadRequestException exception = null;

            // act
            try
            {
                result = await _controller.InstanceAdminChangePassword(updatePasswor);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.InvalidChangeInstanceAdminPasswordParameters, exception.Message);
        }

        [TestMethod]
        public async Task InstanceAdminChangePassword_PasswordIsInvalid_BadRequestException()
        {
            // arrange
            var pass = "asdf1";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(pass);
            var encodedPassword = Convert.ToBase64String(plainTextBytes);
            var updatePasswor = new UpdateUserPassword { Password = encodedPassword };
            var user = new User { Id = 3 };
            IHttpActionResult result = null;
            BadRequestException exception = null;

            _privilegesRepository
               .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
               .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            _usersRepoMock.Setup(repo => repo.GetUserAsync(It.IsAny<int>())).ReturnsAsync(user);

            // act
            try
            {
                result = await _controller.InstanceAdminChangePassword(updatePasswor);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public async Task InstanceAdminChangePassword_UserNotFound_ResourceNotFoundException()
        {
            // arrange
            var updatePasswor = new UpdateUserPassword { Password = "adf1T~asdfasdf" };
            IHttpActionResult result = null;
            ResourceNotFoundException exception = null;

            _privilegesRepository
               .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
               .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);

            _usersRepoMock.Setup(repo => repo.GetUserAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            // act
            try
            {
                result = await _controller.InstanceAdminChangePassword(updatePasswor);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNull(result);
            Assert.AreEqual(ErrorCodes.ResourceNotFound, exception.ErrorCode);
        }

        [TestMethod]
        public async Task InstanceAdminChangePassword_PasswordIsInvalid_OkResult()
        {
            // arrange
            var pass = "adf1T~asdfasdf";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(pass);
            var encodedPassword = Convert.ToBase64String(plainTextBytes);
            var updatePasswor = new UpdateUserPassword { Password = encodedPassword };
            var user = new User { Id = 3 };
            IHttpActionResult result = null;

            _privilegesRepository
               .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
               .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            _usersRepoMock.Setup(repo => repo.GetUserAsync(It.IsAny<int>())).ReturnsAsync(user);

            // act

            result = await _controller.InstanceAdminChangePassword(updatePasswor) as OkResult;


            // assert
            Assert.IsNotNull(result);
        }


        #endregion

        #region Deletete user from groups

        [TestMethod]
        public async Task DeleteUserFromGroups_AllRequirementsSatisfied_SucсessResult()
        {
            // arrange
            _privilegesRepository
                .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            _usersRepoMock.Setup(r => r.DeleteUserFromGroupsAsync(It.IsAny<int>(), It.IsAny<OperationScope>())).ReturnsAsync(1);

            // act
            var result =
                await _controller.DeleteUserFromGroups(UserId, _operationScope) as
                    OkNegotiatedContentResult<DeleteResult>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Content, typeof(DeleteResult));
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task DeleteUserFromGroups_OperationScopeModelIsNull_ReturnBadRequestErrorResult()
        {
            // arrange
            _privilegesRepository
                .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            _usersRepoMock.Setup(r => r.DeleteUserFromGroupsAsync(It.IsAny<int>(), It.IsAny<OperationScope>())).ReturnsAsync(1);

            // act
            await _controller.DeleteUserFromGroups(UserId, null);

            // assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task DeleteUserFromGroups_InvalidUserPermissions_ReturnAuthenticationException()
        {
            // arrange
            _privilegesRepository
                .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
                .ReturnsAsync(InstanceAdminPrivileges.None);
            _usersRepoMock.Setup(r => r.DeleteUserFromGroupsAsync(It.IsAny<int>(), It.IsAny<OperationScope>())).ReturnsAsync(1);

            // act
            await _controller.DeleteUserFromGroups(UserId, _operationScope);

            // assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task DeleteUserFromGroups_UserNotFound_ReturnResourceNotFoundException()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);

            var resourceNotFoundExeption = new ResourceNotFoundException(ErrorMessages.UserNotExist);
            _usersRepoMock.Setup(r => r.DeleteUserFromGroupsAsync(It.IsAny<int>(), It.IsAny<OperationScope>()))
                .Throws(resourceNotFoundExeption);

            // Act
            await _controller.DeleteUserFromGroups(UserId, _operationScope);

            // Assert
            // Exception
        }
        #endregion

        #region Add user to groups

        [TestMethod]
        public async Task AddUserToGroups_AllRequirementsSatisfied_SucсessResult()
        {
            // arrange
            _privilegesRepository
                .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            _usersRepoMock.Setup(r => r.AddUserToGroupsAsync(It.IsAny<int>(), It.IsAny<OperationScope>(), It.IsAny<string>())).ReturnsAsync(1);

            // act
            var result =
                await _controller.AddUserToGroups(UserId, _operationScope) as
                    OkNegotiatedContentResult<CreateResult>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Content, typeof(CreateResult));
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddUserToGroups_OperationScopeModelIsNull_ReturnBadRequestErrorResult()
        {
            // arrange
            _privilegesRepository
                .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);
            _usersRepoMock.Setup(r => r.AddUserToGroupsAsync(It.IsAny<int>(), It.IsAny<OperationScope>(), It.IsAny<string>())).ReturnsAsync(1);

            // act
            var result = await _controller.AddUserToGroups(UserId, null);

            // assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task AddUserToGroups_InvalidUserPermissions_ReturnAuthenticationException()
        {
            // arrange
            _privilegesRepository
                .Setup(repo => repo.GetInstanceAdminPrivilegesAsync(It.IsAny<int>()))
                .ReturnsAsync(InstanceAdminPrivileges.None);
            _usersRepoMock.Setup(r => r.AddUserToGroupsAsync(It.IsAny<int>(), It.IsAny<OperationScope>(), It.IsAny<string>())).ReturnsAsync(1);

            // act
            await _controller.AddUserToGroups(UserId, _operationScope);

            // assert
            // Exception
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task AddUserToGroups_UserNotFound_ReturnResourceNotFoundException()
        {
            // Arrange
            _privilegesRepository
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageUsers);

            var resourceNotFoundExeption = new ResourceNotFoundException(ErrorMessages.UserNotExist);
            _usersRepoMock.Setup(r => r.AddUserToGroupsAsync(It.IsAny<int>(), It.IsAny<OperationScope>(), It.IsAny<string>())).Throws(resourceNotFoundExeption);

            // Act
            await _controller.AddUserToGroups(UserId, _operationScope);

            // Assert
            // Exception
        }
        #endregion
    }
}
