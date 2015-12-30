using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AccessControl.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AccessControl.Controllers
{
    [TestClass]
    public class SessionsControllerTests
    {
        private Mock<IServiceLogRepository> _logMock;
        private Mock<ISessionsRepository> _sessionsRepoMock;
        private Mock<ObjectCache> _cacheMock;
        private SessionsController _controller;

        [TestInitialize]
        public void Initialize()
        {
            _sessionsRepoMock = new Mock<ISessionsRepository>();
            _cacheMock = new Mock<ObjectCache>();
            _logMock = new Mock<IServiceLogRepository>();

            _controller = new SessionsController(_cacheMock.Object, _sessionsRepoMock.Object, _logMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        #region GetSession

        [TestMethod]
        public async Task GetSession_SessionNotFound()
        {
            // Arrange
            _sessionsRepoMock
                .Setup(repo => repo.GetUserSession(It.IsAny<int>()))
                .ReturnsAsync(null);

            // Act
            var result = await _controller.GetSession(100);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetSession_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            int uid = 999;
            _sessionsRepoMock
                .Setup(repo => repo.GetUserSession(It.IsAny<int>()))
                .Throws(new Exception());

            // Act
            var result = await _controller.GetSession(uid);

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as InternalServerErrorResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task GetSession_SessionNotFoundThrowsException()
        {
            // Arrange
            _sessionsRepoMock
                .Setup(repo => repo.GetUserSession(It.IsAny<int>()))
                .Throws(new KeyNotFoundException());

            // Act
            var result = await _controller.GetSession(0);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetSession_ReturnsCorrectSession()
        {
            // Arrange
            int uid = 999;
            var sessionGuid = Guid.NewGuid();
            var session = new Session { SessionId = sessionGuid };
            _sessionsRepoMock.Setup(r => r.GetUserSession(uid)).ReturnsAsync(session);

            // Act
            var resultSession = await _controller.GetSession(uid);

            // Assert
            Assert.IsNotNull(resultSession);
            var responseResult = resultSession as ResponseMessageResult;
            Assert.IsNotNull(responseResult);
            var response = responseResult.Response;
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task GetSession_SessionExpired()
        {
            // Arrange
            int uid = 999;
            var session = new Session { EndTime = DateTime.UtcNow };
            _sessionsRepoMock.Setup(r => r.GetUserSession(uid)).ReturnsAsync(session);

            // Act
            var resultSession = await _controller.GetSession(uid);

            // Assert
            Assert.IsNotNull(resultSession);
            var responseResult = resultSession as NotFoundResult;
            Assert.IsNotNull(responseResult);
        }

        #endregion GetSesstion

        #region PostSession

        [TestMethod]
        public async Task PostSession_PostCorrectSession()
        {
            // Arrange
            int uid = 999;
            var newGuid = Guid.NewGuid();
            Guid?[] guids = { newGuid, Guid.NewGuid() };
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), "user", 3, true)).ReturnsAsync(guids);

            // Act
            var resultSession = await _controller.PostSession(uid, "user", 3, true);

            // Assert
            Assert.IsNotNull(resultSession);
            var responseResult = resultSession as ResponseMessageResult;
            Assert.IsNotNull(responseResult);
            var response = responseResult.Response;
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(response.Headers.Contains("Cache-Control"));
            Assert.IsTrue(response.Headers.Contains("Pragma"));
            Assert.IsTrue(response.Headers.Contains("Session-Token"));

            var sessionTokenValues = response.Headers.GetValues("Session-Token");
            Assert.IsTrue(sessionTokenValues.Count() == 1);
            Assert.IsTrue(sessionTokenValues.First() == newGuid.ToString("N"));
        }

        [TestMethod]
        public async Task PostSession_BeginSessionError()
        {
            // Arrange
            int uid = 999;
            Guid?[] guids = {  };
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), "user", 3, It.IsAny<bool>())).ReturnsAsync(guids);

            // Act
            var resultSession = await _controller.PostSession(uid, "user", 3);

            // Assert
            Assert.IsNotNull(resultSession);
            var responseResult = resultSession as InternalServerErrorResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task PostSession_FirstGuidNotFound()
        {
            // Arrange
            int uid = 999;
            Guid?[] guids = { null };
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), "user", 3, It.IsAny<bool>())).ReturnsAsync(guids);

            // Act
            var resultSession = await _controller.PostSession(uid, "user", 3);

            // Assert
            Assert.IsNotNull(resultSession);
            var responseResult = resultSession as NotFoundResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task PostSession_PostCorrectSessionCacheTest()
        {
            // Arrange
            int uid = 999;
            var firstGuid = Guid.NewGuid();
            var secondGuid = Guid.NewGuid();
            Guid?[] guids = { firstGuid, secondGuid };
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), "user", 3, It.IsAny<bool>())).ReturnsAsync(guids);

            // Act
            await _controller.PostSession(uid, "user", 3);

            // Assert
            var token = Session.Convert(firstGuid);
            _cacheMock.Verify(m => m.Remove(Session.Convert(secondGuid), null));
            _cacheMock.Verify(c => c.Add(token, null, It.Is<CacheItemPolicy>(p => VerifyPolicy(p, token)), null));
        }

        [TestMethod]
        public async Task PostSession_NoProperKeyInCache()
        {
            // Arrange
            int uid = 999;
            var firstGuid = Guid.NewGuid();
            var secondGuid = Guid.NewGuid();
            Guid?[] guids = { firstGuid, secondGuid };
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), "user", 3, It.IsAny<bool>())).ReturnsAsync(guids);
            _cacheMock.Setup(c => c.Remove(It.IsAny<string>(), null)).Throws(new KeyNotFoundException());


            // Act
            var resultSession = await _controller.PostSession(uid, "user", 3);

            // Assert
            Assert.IsNotNull(resultSession);
            var responseResult = resultSession as NotFoundResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task PostSession_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            int uid = 999;
            var firstGuid = Guid.NewGuid();
            var secondGuid = Guid.NewGuid();
            Guid?[] guids = { firstGuid, secondGuid };
            var session = new Session();
            _sessionsRepoMock.Setup(r => r.GetSession(firstGuid)).ReturnsAsync(session);
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), "user", 3, It.IsAny<bool>())).ReturnsAsync(guids);
            _cacheMock.Setup(c => c.Remove(It.IsAny<string>(), null)).Throws(new Exception());

            // Act
            var result = await _controller.PostSession(uid, "user", 3);

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as InternalServerErrorResult;
            Assert.IsNotNull(responseResult);
        }

        #endregion PostSession

        #region SelectSession

        [TestMethod]
        public async Task SelectSession_RepositoryThrowsException_KeyNotFound()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", Session.Convert(Guid.NewGuid()));
            _sessionsRepoMock
                .Setup(repo => repo.SelectSessions(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new KeyNotFoundException());
                //.ReturnsAsync((IEnumerable<Session>)new KeyNotFoundException());

            // Act
            var result = await _controller.SelectSessions();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task SelectSession_CallingWithInvalidArgument_BadRequest()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", Session.Convert(Guid.NewGuid()));

            // Act
            var result = await _controller.SelectSessions("0", "-1");

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task SelectSession_RepositoryThrowsException_BadRequest()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", Session.Convert(Guid.NewGuid()));
            _sessionsRepoMock
                .Setup(repo => repo.SelectSessions(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new ArgumentNullException());
                //.ReturnsAsync((IEnumerable<Session>)null);

            // Act
            var result = await _controller.SelectSessions();

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task SelectSession_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", Session.Convert(Guid.NewGuid()));
            _sessionsRepoMock
                .Setup(repo => repo.SelectSessions(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception());

            // Act
            var result = await _controller.SelectSessions();

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as InternalServerErrorResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task SelectSession_RepositoryReturnsResult_Result()
        {
            // Arrange
            var sessions = new List<Session>() { new Session() };
            _controller.Request.Headers.Add("Session-Token", Session.Convert(Guid.NewGuid()));
            _sessionsRepoMock
                .Setup(repo => repo.SelectSessions(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(sessions);

            // Act
            var result = await _controller.SelectSessions();

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as OkNegotiatedContentResult<IEnumerable<Session>>;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task SelectSession_TokenNotSet_BadRequest()
        {
            // Arrange

            // Act
            var result = await _controller.SelectSessions();

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task SelectSession_TokenIsNull_BadRequest()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", "null");

            // Act
            var resultSessions = await _controller.SelectSessions();

            // Assert
            Assert.IsNotNull(resultSessions);
            var responseResult = resultSessions as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        #endregion SelectSession

        #region DeleteSession

        [TestMethod]
        public async Task DeleteSession_TokenIsNotInRepository_KeyNotFound()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", Session.Convert(Guid.NewGuid()));

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task DeleteSession_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            _controller.Request.Headers.Add("Session-Token", Session.Convert(newGuid));
            _sessionsRepoMock
                .Setup(repo => repo.EndSession(newGuid, false))
                .Throws(new Exception());

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as InternalServerErrorResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task DeleteSession_RepositoryThrowsException_ArgumentNull()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            _controller.Request.Headers.Add("Session-Token", Session.Convert(newGuid));
            _cacheMock.Setup(c => c.Remove(It.IsAny<string>(), null)).Returns(new object());
            _sessionsRepoMock
                .Setup(repo => repo.EndSession(newGuid, false))
                .Throws(new ArgumentNullException());

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task DeleteSession_TokenHasNotBeenSet_BadRequest()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            _sessionsRepoMock.Setup(r => r.EndSession(newGuid, false)).Returns(Task.FromResult(new object()));

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task DeleteSession_RepositoryReturnsResult_OkResult()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            _sessionsRepoMock.Setup(r => r.EndSession(newGuid, false)).Returns(Task.FromResult(new object()));
            _cacheMock.Setup(c => c.Remove(It.IsAny<string>(), null)).Returns(new object());
            _controller.Request.Headers.Add("Session-Token", newGuid.ToString("N"));

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as OkResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task DeleteSession_TokenIsNull_BadRequest()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", "null");

            // Act
            var resultSessions = await _controller.DeleteSession();

            // Assert
            Assert.IsNotNull(resultSessions);
            var responseResult = resultSessions as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        #endregion DeleteSession

        #region PutSession

        [TestMethod]
        public async Task PutSession_KeyNotFound()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", Session.Convert(Guid.NewGuid()));

            // Act
            var resultSession = await _controller.PutSession("", 1);

            // Assert
            Assert.IsInstanceOfType(resultSession, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutSession_CorrectResult()
        {
            // Arrange
            int uid = 999;
            var newGuid = Guid.NewGuid();
            var token = Session.Convert(newGuid);

            _controller.Request.Headers.Add("Session-Token", token);

            _cacheMock.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((cacheKey, x) => cacheKey == token ? new Session {UserId = uid} : null);

            // Act
            var resultSession = await _controller.PutSession("", 1);

            // Assert
            Assert.IsNotNull(resultSession);
            var response = resultSession as ResponseMessageResult;
            Assert.IsNotNull(response);
            var sessionTokenValues = response.Response.Headers.GetValues("Session-Token");
            Assert.IsTrue(sessionTokenValues.Count() == 1);
            Assert.IsTrue(sessionTokenValues.First() == token);
            var cacheControlValues = response.Response.Headers.GetValues("Cache-Control");
            Assert.IsTrue(cacheControlValues.Count() == 1);
            Assert.IsTrue(cacheControlValues.First() == "no-store, must-revalidate, no-cache");
            var pragmaValues = response.Response.Headers.GetValues("Pragma");
            Assert.IsTrue(pragmaValues.Count() == 1);
            Assert.IsTrue(pragmaValues.First() == "no-cache");

        }

        [TestMethod]
        public async Task PutSession_NoCache()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var token = Session.Convert(newGuid);
            var session = new Session();
            _sessionsRepoMock.Setup(r => r.GetSession(newGuid)).ReturnsAsync(session);
            _controller.Request.Headers.Add("Session-Token", token);

            // Act
            var resultSession = await _controller.PutSession("", 1);

            // Assert
            Assert.IsNotNull(resultSession);
            var response = resultSession as ResponseMessageResult;
            Assert.IsNotNull(response);
            var sessionTokenValues = response.Response.Headers.GetValues("Session-Token");
            Assert.IsTrue(sessionTokenValues.Count() == 1);
            Assert.IsTrue(sessionTokenValues.First() == token);
            var cacheControlValues = response.Response.Headers.GetValues("Cache-Control");
            Assert.IsTrue(cacheControlValues.Count() == 1);
            Assert.IsTrue(cacheControlValues.First() == "no-store, must-revalidate, no-cache");
            var pragmaValues = response.Response.Headers.GetValues("Pragma");
            Assert.IsTrue(pragmaValues.Count() == 1);
            Assert.IsTrue(pragmaValues.First() == "no-cache");

            _cacheMock.Verify(c => c.Add(token, session, It.Is<CacheItemPolicy>(p => VerifyPolicy(p, token)), null));
        }

        [TestMethod]
        public async Task PutSession_NoToken()
        {
            // Arrange

            // Act
            var resultSession = await _controller.PutSession("", 1);

            // Assert
            var responseResult = resultSession as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task PutSession_ArgumentNull()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var token = Session.Convert(newGuid);
            _controller.Request.Headers.Add("Session-Token", token);

            _sessionsRepoMock
                .Setup(r => r.GetSession(newGuid))
                .Throws(new ArgumentNullException());

            // Act
            var result = await _controller.PutSession("", 1);

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task PutSession_FormatExp()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var token = Session.Convert(newGuid);
            _controller.Request.Headers.Add("Session-Token", token);

            _sessionsRepoMock
                .Setup(r => r.GetSession(newGuid))
                .Throws(new FormatException());

            // Act
            var result = await _controller.PutSession("", 1);

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as BadRequestResult;
            Assert.IsNotNull(responseResult);
        }

        [TestMethod]
        public async Task PutSession_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var token = Session.Convert(newGuid);
            _controller.Request.Headers.Add("Session-Token", token);

            _sessionsRepoMock
                .Setup(r => r.GetSession(newGuid))
                .Throws(new Exception());

            // Act
            var result = await _controller.PutSession("", 1);

            // Assert
            Assert.IsNotNull(result);
            var responseResult = result as InternalServerErrorResult;
            Assert.IsNotNull(responseResult);
        }

        #endregion PutSession

        #region Load

        [TestMethod]
        public void Load_RepositoryReturnsSessions_ReadyIsSet()
        {
            // Arrange
            var sessions = new List<Session>() { new Session() };
            _sessionsRepoMock
                .Setup(repo => repo.SelectSessions(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(sessions);

            // Act
            SessionsController.Load(_cacheMock.Object);

            // Assert
            Assert.IsTrue(StatusController.Ready.Wait(200));
        }

        [TestMethod]
        public void Load_RepositoryThrowsException_ReadyIsNotSet()
        {
            // Arrange
            StatusController.Ready.Reset();
            _sessionsRepoMock
                .Setup(repo => repo.SelectSessions(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception());

            // Act
            SessionsController.Load(_cacheMock.Object);

            // Assert
            Assert.IsFalse(StatusController.Ready.Wait(200));
        }

        #endregion Load

        private bool VerifyPolicy(CacheItemPolicy policy, string token)
        {
            policy.RemovedCallback(new CacheEntryRemovedArguments(_cacheMock.Object, CacheEntryRemovedReason.Evicted, new CacheItem(token)));
            //_logMock.Verify(l => l.WriteEntry(WebApiConfig.ServiceLogSource, "Not enough memory", LogEntryType.Error));
            policy.RemovedCallback(new CacheEntryRemovedArguments(_cacheMock.Object, CacheEntryRemovedReason.Expired, new CacheItem(token)));
            _sessionsRepoMock.Verify(r => r.EndSession(Session.Convert(token), true));
            return true;
        }
    }
}
