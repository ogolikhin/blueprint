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
using ServiceLibrary.Models;

namespace AdminStore.Controllers
{
    [TestClass]
    public class SessionsControllerTests
    {
        private Mock<IAuthenticationRepository> _authenticationRepositoryMock;
        private Mock<IServiceLogRepository> _logMock;

        private const string SamlResponse = "samlResponse";
        private const string Login = "admin";
        private const string Password = "changeme";
        private const string EncryptedUsername = "Zm9v";
        private const string EncryptedPassword = "YmFy";
        private readonly AuthenticationUser _loginUser = new AuthenticationUser() { Id = 1, Login = Login };

        private HttpClientProvider _httpClientProvider;

        [TestInitialize]
        public void Initialize()
        {
            _authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            _logMock = new Mock<IServiceLogRepository>();
            _httpClientProvider = new HttpClientProvider();
        }

        #region PostSession

        [TestMethod]
        public async Task PostSession_ForceIsTrue_Success()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(Login, Password, false)).ReturnsAsync(_loginUser);

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

            var controller = new SessionsController(_authenticationRepositoryMock.Object, httpClientProvider, _logMock.Object);

            // Act
            var result = (ResponseMessageResult)await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(Login), SystemEncryptions.EncodeTo64UTF8(Password), true);

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
            _authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(Login, Password, false)).ReturnsAsync(_loginUser);

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

            var controller = new SessionsController(_authenticationRepositoryMock.Object, httpClientProvider, _logMock.Object);

            // Act
            var result = (ResponseMessageResult)await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(Login), SystemEncryptions.EncodeTo64UTF8(Password));

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
            _authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(Login, Password, false)).ReturnsAsync(_loginUser);

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));

            var controller = new SessionsController(_authenticationRepositoryMock.Object, httpClientProvider, _logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(Login), SystemEncryptions.EncodeTo64UTF8(Password));

            // Assert
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
        }

        [TestMethod]
        public async Task PostSession_ServerError_InternalServerErrorResult()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(Login, Password, false)).ReturnsAsync(_loginUser);

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.NotFound));

            var controller = new SessionsController(_authenticationRepositoryMock.Object, httpClientProvider, _logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(Login), SystemEncryptions.EncodeTo64UTF8(Password), true);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostSession_AuthenticationException_HttpResponseException()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(Login, Password, false))
                .Throws(new AuthenticationException("Invalid username or password"));

            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            // Act
            await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(Login), SystemEncryptions.EncodeTo64UTF8(Password), true);
        }

        [TestMethod]
        public async Task PostSession_ArgumentNullException_BadRequestResult()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(m => m.AuthenticateUserAsync(Login, Password, false))
                .Throws(new ArgumentNullException());

            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSession(SystemEncryptions.EncodeTo64UTF8(Login), SystemEncryptions.EncodeTo64UTF8(Password), true);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task PostSession_FormatException_BadRequestResult()
        {
            // Arrange
            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            // Act
            try
            {
                await controller.PostSession(Login, Password, true);
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
            _authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(SamlResponse)).ReturnsAsync(_loginUser);

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

            var controller = new SessionsController(_authenticationRepositoryMock.Object, httpClientProvider, _logMock.Object);

            // Act
            var result = (ResponseMessageResult)await controller.PostSessionSingleSignOn(SamlResponse);

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
            var httpRequestMessage = new HttpRequestMessage();

            _authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(SamlResponse)).Throws(new FederatedAuthenticationException(FederatedAuthenticationErrorCode.Unknown));

            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = httpRequestMessage
            };

            // Act
            try
            {
                await controller.PostSessionSingleSignOn(SamlResponse);
            }
            catch (HttpResponseException ex)
            {
                // Assert
                Assert.AreEqual(ex.Response.StatusCode, HttpStatusCode.Unauthorized);

                return;
            }

            Assert.Fail("A HttpResponseException was not thrown");
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_FormatException_BadRequestResult()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(SamlResponse)).Throws(new FormatException());

            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSessionSingleSignOn(SamlResponse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_FederatedAuthenticationException_BadRequestResult()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage();

            _authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(SamlResponse)).Throws(new FederatedAuthenticationException(FederatedAuthenticationErrorCode.WrongFormat));

            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = httpRequestMessage
            };

            // Act
            try
            {
                await controller.PostSessionSingleSignOn(SamlResponse);
            }
            catch (HttpResponseException ex)
            {
                // Assert
                Assert.AreEqual(ex.Response.StatusCode, HttpStatusCode.BadRequest);

                return;
            }

            Assert.Fail("A HttpResponseException was not thrown");
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_SessionFound_ConflictResult()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(SamlResponse)).ReturnsAsync(_loginUser);

            var token = Guid.NewGuid().ToString();

            var httpClientProvider = new TestHttpClientProvider(request =>
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Headers.Add("Session-Token", token);
                return httpResponseMessage;
            });

            var controller = new SessionsController(_authenticationRepositoryMock.Object, httpClientProvider, _logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSessionSingleSignOn(SamlResponse);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ConflictResult));
        }

        [TestMethod]
        public async Task PostSessionSingleSignOn_ServerError_InternalServerErrorResult()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(m => m.AuthenticateSamlUserAsync(SamlResponse)).ReturnsAsync(_loginUser);

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.NotFound));

            var controller = new SessionsController(_authenticationRepositoryMock.Object, httpClientProvider, _logMock.Object);

            // Act
            IHttpActionResult result = await controller.PostSessionSingleSignOn(SamlResponse);

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

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider, _logMock.Object)
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

            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.NotFound));

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider, _logMock.Object)
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

            var httpClientProvider = new TestHttpClientProvider(request => { throw new Exception(); });

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider, _logMock.Object)
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

            var controller = new SessionsController(new AuthenticationRepository(), httpClientProvider, _logMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            // Act
            IHttpActionResult result = await controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        #endregion

        #region VerifyCredentials

        [TestMethod]
        public async Task VerifyCredentials_Should_Decrypt_Username_From_Base64()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(repo => repo.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new AuthenticationUser()
            {
                Id = 2
            });

            // Act
            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = new HttpRequestMessage()
                {
                    Properties =
                    {
                        { ServiceConstants.SessionProperty, new Session() { UserId = 2 } }
                    }
                }
            };

            await controller.VerifyCredentials(EncryptedUsername, EncryptedPassword);

            // Assert
            _authenticationRepositoryMock.Verify(repo => repo.AuthenticateUserAsync("foo", It.IsAny<string>(), true));
        }

        [TestMethod]
        public async Task VerifyCredentials_Should_Decrypt_Password_From_Base64()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(repo => repo.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new AuthenticationUser()
            {
                Id = 2
            });

            // Act
            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = new HttpRequestMessage()
                {
                    Properties =
                    {
                        { ServiceConstants.SessionProperty, new Session() { UserId = 2 } }
                    }
                }
            };

            await controller.VerifyCredentials(EncryptedUsername, EncryptedPassword);

            // Assert
            _authenticationRepositoryMock.Verify(repo => repo.AuthenticateUserAsync(It.IsAny<string>(), "bar", true));
        }

        [TestMethod]
        public async Task VerifyCredentials_Should_Throw_Bad_Request_Exception_When_AuthenticateUserAsync_Throws_Authentication_Exception()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(repo => repo.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>(), true))
                                         .Throws(new AuthenticationException("baz", ErrorCodes.UserDisabled));

            // Act
            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = new HttpRequestMessage()
                {
                    Properties =
                    {
                        { ServiceConstants.SessionProperty, new Session() { UserId = 2 } }
                    }
                }
            };

            try
            {
                await controller.VerifyCredentials(EncryptedUsername, EncryptedPassword);
            }
            catch (BadRequestException ex)
            {
                // Assert
                Assert.AreEqual("baz", ex.Message);
                Assert.AreEqual(ErrorCodes.UserDisabled, ex.ErrorCode);

                return;
            }

            Assert.Fail("A Bad Request Exception was not thrown.");
        }

        [TestMethod]
        public async Task VerifyCredentials_Should_Throw_Authentication_Exception_When_Session_Does_Not_Exist()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(repo => repo.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>(), true))
                                         .Throws(new AuthenticationException("baz", ErrorCodes.UserDisabled));

            // Act
            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = new HttpRequestMessage()
                {
                    Properties = { }
                }
            };

            try
            {
                await controller.VerifyCredentials(EncryptedUsername, EncryptedPassword);
            }
            catch (AuthenticationException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

                return;
            }

            Assert.Fail("An Authentication Exception was not thrown.");
        }

        [TestMethod]
        public async Task VerifyCredentials_Should_Throw_Bad_Request_Exception_When_Session_UserId_Doesnt_Match_User()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(repo => repo.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new AuthenticationUser()
            {
                Id = 2
            });

            // Act
            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = new HttpRequestMessage()
                {
                    Properties =
                    {
                        { ServiceConstants.SessionProperty, new Session() { UserId = 3 } }
                    }
                }
            };

            try
            {
                await controller.VerifyCredentials(EncryptedUsername, EncryptedPassword);
            }
            catch (BadRequestException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.InvalidCredentials, ex.ErrorCode);

                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task VerifyCredentials_Should_Succeed_When_Session_UserId_Matches_User()
        {
            // Arrange
            _authenticationRepositoryMock.Setup(repo => repo.AuthenticateUserAsync("foo", "bar", true)).ReturnsAsync(new AuthenticationUser()
            {
                Id = 2
            });

            // Act
            var controller = new SessionsController(_authenticationRepositoryMock.Object, _httpClientProvider, _logMock.Object)
            {
                Request = new HttpRequestMessage()
                {
                    Properties =
                    {
                        { ServiceConstants.SessionProperty, new Session() { UserId = 2 } }
                    }
                }
            };

            await controller.VerifyCredentials(EncryptedUsername, EncryptedPassword);
        }

        #endregion
    }
}
