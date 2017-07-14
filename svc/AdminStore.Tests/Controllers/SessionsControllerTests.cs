using AdminStore.Models;
using AdminStore.Repositories;
using AdminStore.Saml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace AdminStore.Controllers
{
    [TestClass]
    public class SessionsControllerTests
    {
        #region PostSession

        [TestMethod]
        public async Task PostSession_ForceIsTrue_Success()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";
            var loginUser = new AuthenticationUser { Id = 1, Login = login };

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password)).ReturnsAsync(loginUser);

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

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider, logMock.Object);

            // Act
            var result = (ResponseMessageResult)await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(login), SystemEncryptions.EncodeTo64UTF8(password), true);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.AreEqual(token, result.Response.Headers.GetValues("Session-Token").FirstOrDefault());
            var expectedToken = await result.Response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedToken, token);
        }

        [TestMethod]
        public async Task PostSession_SessionNotFound_Success()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";
            var loginUser = new AuthenticationUser { Id = 1, Login = login };

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password)).ReturnsAsync(loginUser);

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

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider, logMock.Object);

            // Act
            var result = (ResponseMessageResult)await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(login), SystemEncryptions.EncodeTo64UTF8(password));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.AreEqual(token, result.Response.Headers.GetValues("Session-Token").FirstOrDefault());
            var expectedToken = await result.Response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedToken, token);
        }

        [TestMethod]
        public async Task PostSession_SessionFound_ConflictResult()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";
            var loginUser = new AuthenticationUser { Id = 1, Login = login };

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password)).ReturnsAsync(loginUser);

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider, logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(login), SystemEncryptions.EncodeTo64UTF8(password));

            // Assert
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
        }

        [TestMethod]
        public async Task PostSession_ServerError_InternalServerErrorResult()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";
            var loginUser = new AuthenticationUser { Id = 1, Login = login };

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>().Object;
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password)).ReturnsAsync(loginUser);

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.NotFound));

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider, logMock);

            // Act
            IHttpActionResult result = await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(login), SystemEncryptions.EncodeTo64UTF8(password), true);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostSession_AuthenticationException_HttpResponseException()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password))
                .Throws(new AuthenticationException("Invalid username or password"));

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider(), logMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            // Act
            await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(login), SystemEncryptions.EncodeTo64UTF8(password), true);
        }

        [TestMethod]
        public async Task PostSession_ArgumentNullException_BadRequestResult()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(login, password))
                .Throws(new ArgumentNullException());

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider(), logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(login), SystemEncryptions.EncodeTo64UTF8(password), true);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task PostSession_FormatException_BadRequestResult()
        {
            // Arrange
            const string login = "admin";
            const string password = "changeme";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider(), logMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            // Act
            try
            {
                await controller.PostSession(login, password, true);
            }
            catch (HttpResponseException ex)
            {
                Assert.IsTrue(ex.Response.StatusCode == HttpStatusCode.Unauthorized);
                return;
            }
            // Assert
            Assert.IsTrue(false);           
        }

        #endregion

        #region PostSessionSingleSignOn

        [TestMethod]
        public async Task PostSessionSingleSignOn_SessionNotFound_Success()
        {
            // Arrange
            const string login = "admin";
            var loginUser = new AuthenticationUser { Id = 1, Login = login };
            const string samlResponse = "samlResponse";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(samlResponse)).ReturnsAsync(loginUser);

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

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider, logMock.Object);

            // Act
            var result = (ResponseMessageResult)await controller.PostSessionSingleSignOn(samlResponse);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.AreEqual(token, result.Response.Headers.GetValues("Session-Token").FirstOrDefault());
            var expectedToken = await result.Response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedToken, token);
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_FederatedAuthenticationException_UnauthorizedResult()
        {
            // Arrange
            const string samlResponse = "samlResponse";
            var httpRequestMessage = new HttpRequestMessage();
            Exception exception = null;

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(samlResponse)).Throws(new FederatedAuthenticationException(FederatedAuthenticationErrorCode.Unknown));

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider(), logMock.Object)
            {
                Request = httpRequestMessage
            };

            // Act
            try
            {
                await controller.PostSessionSingleSignOn(samlResponse);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsInstanceOfType(exception, typeof(HttpResponseException));
            Assert.AreEqual(((HttpResponseException)exception).Response.StatusCode, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_FormatException_BadRequestResult()
        {
            // Arrange
            const string samlResponse = "samlResponse";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(samlResponse)).Throws(new FormatException());

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider(), logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSessionSingleSignOn(samlResponse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_FederatedAuthenticationException_BadRequestResult()
        {
            // Arrange
            const string samlResponse = "samlResponse";
            var httpRequestMessage = new HttpRequestMessage();
            Exception exception = null;

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(samlResponse)).Throws(new FederatedAuthenticationException(FederatedAuthenticationErrorCode.WrongFormat));

            var controller = new SessionsController(authenticationRepositoryMock.Object, new HttpClientProvider(), logMock.Object)
            {
                Request = httpRequestMessage
            };

            // Act
            try
            {
                await controller.PostSessionSingleSignOn(samlResponse);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsInstanceOfType(exception, typeof(HttpResponseException));
            Assert.AreEqual(((HttpResponseException)exception).Response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_SessionFound_ConflictResult()
        {
            // Arrange
            const string login = "admin";
            var loginUser = new AuthenticationUser { Id = 1, Login = login };
            const string samlResponse = "samlResponse";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(samlResponse)).ReturnsAsync(loginUser);

            var token = Guid.NewGuid().ToString();

            var httpClientProvider = new TestHttpClientProvider(request =>
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Headers.Add("Session-Token", token);
                return httpResponseMessage;
            });

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider, logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSessionSingleSignOn(samlResponse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_ServerError_InternalServerErrorResult()
        {
            // Arrange
            const string login = "admin";
            var loginUser = new AuthenticationUser { Id = 1, Login = login };
            const string samlResponse = "samlResponse";

            var authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(samlResponse)).ReturnsAsync(loginUser);

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.NotFound));

            var controller = new SessionsController(authenticationRepositoryMock.Object, httpClientProvider, logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSessionSingleSignOn(samlResponse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion

        #region DeleteSession

        [TestMethod]
        public async Task DeleteSession_SessionFound_OkResult()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("Session-Token", Guid.NewGuid().ToString());
            var logMock = new Mock<IServiceLogRepository>();

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider, logMock.Object)
            {
                Request = httpRequestMessage
            };

            // Act
            IHttpActionResult result = await controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task DeleteSession_SessionNotFound_ResponseMessageResult()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("Session-Token", Guid.NewGuid().ToString());
            var logMock = new Mock<IServiceLogRepository>();

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.NotFound));

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider, logMock.Object)
            {
                Request = httpRequestMessage
            };

            // Act
            IHttpActionResult result = await controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ResponseMessageResult));
        }

        [TestMethod]
        public async Task DeleteSession_Exception_InternalServerErrorResult()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("Session-Token", Guid.NewGuid().ToString());
            var logMock = new Mock<IServiceLogRepository>();

            var httpClientProvider = new TestHttpClientProvider(request => { throw new Exception(); });

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider, logMock.Object)
            {
                Request = httpRequestMessage
            };

            // Act
            IHttpActionResult result = await controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task DeleteSession_SessionTokenIsNull_BadRequest()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var logMock = new Mock<IServiceLogRepository>();

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider, logMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            // Act
            IHttpActionResult result = await controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        #endregion
    }
}
