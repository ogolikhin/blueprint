using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AdminStore.Models;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        private UsersController _controller;

        [TestInitialize]
        public void Initialize()
        {
            var session = new Session { UserId = 1 };
            _usersRepoMock = new Mock<ISqlUserRepository>();
            _logMock = new Mock<IServiceLogRepository>();
            _controller = new UsersController(_usersRepoMock.Object, _logMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            _controller.Request.Properties["Session"] = session;
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
    }
}
