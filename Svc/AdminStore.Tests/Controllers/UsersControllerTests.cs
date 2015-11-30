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
using Newtonsoft.Json;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Controllers
{
    [TestClass]
    public class UsersControllerTests 
    {
        private Mock<ISqlUserRepository> _usersRepoMock;
        private UsersController _controller;

        [TestInitialize]
        public void Initialize()
        {
            var session = new Session { UserId = 1 };
            var httpClientProvider = new TestHttpClientProvider(request =>
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(session))
                };
                return httpResponseMessage;
            });
            _usersRepoMock = new Mock<ISqlUserRepository>();
            _controller = new UsersController(_usersRepoMock.Object, httpClientProvider)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            _controller.Request.Headers.Add("Session-Token", Session.Convert(Guid.NewGuid()));
        }

        [TestMethod]
        public void GetLoginUser_Success()
        {
            // Arrange
            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new LoginUser()));

            // Act
            var result = ((System.Web.Http.Results.OkNegotiatedContentResult<LoginUser>)(_controller.GetLoginUser().Result)).Content;

            // Assert
            Assert.IsInstanceOfType(result, typeof(LoginUser));
        }

        [TestMethod]
        public void GetLoginUser_RepositoryReturnsNull_Unauthorized()
        {
            // Arrange
            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<LoginUser>(null));

            // Act
            var result = _controller.GetLoginUser().Result;

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as UnauthorizedResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public void GetLoginUser_RepositoryThrowsArgumentNullException_BadRequest()
        {
            // Arrange
            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .Throws(new ArgumentNullException());

            // Act
            var result = _controller.GetLoginUser().Result;

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public void GetLoginUser_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            //just to add code coverage
            var usercontroller = new UsersController();
            _usersRepoMock
                .Setup(repo => repo.GetLoginUserByIdAsync(It.IsAny<int>()))
                .Throws(new Exception());

            // Act
            var result = _controller.GetLoginUser().Result;

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as InternalServerErrorResult;
            Assert.IsNotNull(responseResult);
        }
    }
}
