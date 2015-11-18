﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AdminStore.Models;
using AdminStore.Repositories;
using AdminStore.Saml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;

namespace AdminStore.Controllers
{
    [TestClass]
    public class SessionsControllerTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new SessionsController();

            // Assert
            Assert.IsInstanceOfType(controller._authenticationRepository, typeof(AuthenticationRepository));
            Assert.IsInstanceOfType(controller._httpClientProvider, typeof(HttpClientProvider));
        }

        #endregion

        #region PostSession

        [TestMethod]
        public async Task PostSession_Force_True_Success()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";
            var loginUser = new LoginUser { Id = 1, Login = login };

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password)).Returns(Task.FromResult(loginUser));

            var token = Guid.NewGuid().ToString();

            var httpClientProvider = new TestHttpClientProvider(request =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Headers.Add("Session-Token", token);
                return httpResponseMessage;
            });

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider);

            // Act
            var result = (ResponseMessageResult)await controller.PostSession(login, password, true);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.AreEqual(token, result.Response.Headers.GetValues("Session-Token").FirstOrDefault());
            var expectedToken = await result.Response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedToken, token);
        }

        [TestMethod]
        public async Task PostSession_Success()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";
            var loginUser = new LoginUser { Id = 1, Login = login };

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password)).Returns(Task.FromResult(loginUser));

            var token = Guid.NewGuid().ToString();

            var httpClientProvider = new TestHttpClientProvider(request =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Headers.Add("Session-Token", token);
                return httpResponseMessage;
            });

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider);

            // Act
            var result = (ResponseMessageResult)await controller.PostSession(login, password);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.AreEqual(token, result.Response.Headers.GetValues("Session-Token").FirstOrDefault());
            var expectedToken = await result.Response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedToken, token);
        }

        [TestMethod]
        public async Task PostSession_SessionExists_ConflictResult()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";
            var loginUser = new LoginUser { Id = 1, Login = login };

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password)).Returns(Task.FromResult(loginUser));

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider);

            // Act
            var conflictResult = await controller.PostSession(login, password) as ConflictResult;

            // Assert
            Assert.IsNotNull(conflictResult);
        }

        [TestMethod]
        public async Task PostSession_ServerError_InternalServerErrorResult()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";
            var loginUser = new LoginUser { Id = 1, Login = login };

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password)).Returns(Task.FromResult(loginUser));

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.NotFound));

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider);

            // Act
            var internalServerErrorResult = await controller.PostSession(login, password, true) as InternalServerErrorResult;

            // Assert
            Assert.IsNotNull(internalServerErrorResult);
        }

        [TestMethod]
        public async Task PostSession_AuthenticationException_UnauthorizedResult()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password))
                .Throws(new AuthenticationException());

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider());

            // Act
            var unauthorizedResult = await controller.PostSession(login, password, true) as UnauthorizedResult;

            // Assert
            Assert.IsNotNull(unauthorizedResult);
        }

        [TestMethod]
        public async Task PostSession_ArgumentNullException_BadRequestResult()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password))
                .Throws(new ArgumentNullException());

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider());

            // Act
            var badRequestResult = await controller.PostSession(login, password, true) as BadRequestResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
        }

        [TestMethod]
        public async Task PostSession_FormatException_BadRequestResult()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password))
                .Throws(new FormatException());

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider());

            // Act
            var badRequestResult = await controller.PostSession(login, password, true) as BadRequestResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
        }

        #endregion

        #region PostSessionSingleSignOn

        [TestMethod]
        public async Task PostSessionSingleSignOn_Success()
        {
            // Arrange
            const string login = "admin";
            var loginUser = new LoginUser { Id = 1, Login = login };
            const string samlResponse = "samlResponse";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(samlResponse)).Returns(Task.FromResult(loginUser));

            var token = Guid.NewGuid().ToString();

            var httpClientProvider = new TestHttpClientProvider(request =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Headers.Add("Session-Token", token);
                return httpResponseMessage;
            });

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider);

            // Act
            var result = (ResponseMessageResult)await controller.PostSessionSingleSignOn(samlResponse);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.AreEqual(token, result.Response.Headers.GetValues("Session-Token").FirstOrDefault());
            var expectedToken = await result.Response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedToken, token);
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_FederatedAuthenticationException_NotFoundResult()
        {
            // Arrange
            const string samlResponse = "samlResponse";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(samlResponse)).Throws(new FederatedAuthenticationException(FederatedAuthenticationErrorCode.Unknown));

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider());

            // Act
            var notFoundResult = await controller.PostSessionSingleSignOn(samlResponse) as NotFoundResult;

            // Assert
            Assert.IsNotNull(notFoundResult);
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_InternalServerError()
        {
            // Arrange
            const string login = "admin";
            var loginUser = new LoginUser { Id = 1, Login = login };
            const string samlResponse = "samlResponse";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(samlResponse)).Returns(Task.FromResult(loginUser));

            var token = Guid.NewGuid().ToString();

            var httpClientProvider = new TestHttpClientProvider(request =>
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Headers.Add("Session-Token", token);
                return httpResponseMessage;
            });

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider);

            // Act
            var internalServerErrorResult = await controller.PostSessionSingleSignOn(samlResponse) as InternalServerErrorResult;

            // Assert
            Assert.IsNotNull(internalServerErrorResult);
        }

        #endregion

        #region DeleteSession

        [TestMethod]
        public async Task DeleteSession_OkResult()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("Session-Token", Guid.NewGuid().ToString());

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider)
            {
                Request = httpRequestMessage
            };

            // Act
            var okResult = await controller.DeleteSession() as OkResult;

            // Assert
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public async Task DeleteSession_InternalServerError()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.NotFound));

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider);

            // Act
            var internalServerErrorResult = await controller.DeleteSession() as InternalServerErrorResult;

            // Assert
            Assert.IsNotNull(internalServerErrorResult);
        }

        #endregion
    }
}
