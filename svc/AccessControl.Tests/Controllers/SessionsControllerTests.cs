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

        #region Constructor

        [TestMethod]
        public void Constructor_Always_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new SessionsController();

            // Assert
            Assert.IsInstanceOfType(controller._cache, typeof(MemoryCache));
            Assert.IsInstanceOfType(controller._repo, typeof(SqlSessionsRepository));
            Assert.IsInstanceOfType(controller._log, typeof(ServiceLogRepository));
        }

        #endregion Constructor

        #region GetSession

        [TestMethod]
        public async Task GetSession_SessionDoesNotExist_NotFound()
        {
            // Arrange
            int uid = 100;
            _sessionsRepoMock.Setup(repo => repo.GetUserSession(uid)).ReturnsAsync(null);

            // Act
            var result = await _controller.GetSession(uid);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetSession_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            int uid = 999;
            _sessionsRepoMock.Setup(repo => repo.GetUserSession(It.IsAny<int>())).Throws<Exception>();

            // Act
            var result = await _controller.GetSession(uid);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task GetSession_SessionExists_ReturnsSession()
        {
            // Arrange
            int uid = 999;
            var guid = Guid.NewGuid();
            var session = new Session { SessionId = guid };
            _sessionsRepoMock.Setup(r => r.GetUserSession(uid)).ReturnsAsync(session);

            // Act
            var result = await _controller.GetSession(uid) as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Response.IsSuccessStatusCode);
            Assert.AreEqual(session, await result.Response.Content.ReadAsAsync<Session>());
        }

        [TestMethod]
        public async Task GetSession_SessionExpired_NotFound()
        {
            // Arrange
            int uid = 999;
            var session = new Session { EndTime = DateTime.UtcNow };
            _sessionsRepoMock.Setup(r => r.GetUserSession(uid)).ReturnsAsync(session);

            // Act
            var resultSession = await _controller.GetSession(uid);

            // Assert
            Assert.IsInstanceOfType(resultSession, typeof(NotFoundResult));
        }

        #endregion GetSession

        #region PostSession

        [TestMethod]
        public async Task PostSession_RepositoryReturnsNewSession_OkWithSessionToken()
        {
            // Arrange
            int uid = 999;
            string userName = "user";
            int licenseLevel = 3;
            var newSessionId = Guid.NewGuid();
            var session = new Session { SessionId = newSessionId };
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), userName, licenseLevel, true, It.IsAny<Action<Guid>>())).ReturnsAsync(session);

            // Act
            var result = await _controller.PostSession(uid, userName, licenseLevel, true) as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            var response = result.Response;
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(Session.Convert(newSessionId), response.Headers.GetValues("Session-Token").Single());
        }

        [TestMethod]
        public async Task PostSession_RepositoryReturnsNoSession_NotFound()
        {
            // Arrange
            int uid = 999;
            string userName = "user";
            int licenseLevel = 3;
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), userName, licenseLevel, false, It.IsAny<Action<Guid>>())).ReturnsAsync(null);

            // Act
            var result = await _controller.PostSession(uid, userName, licenseLevel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PostSession_RepositoryReturnsOldAndNewSession_UpdatesCache()
        {
            // Arrange
            int uid = 999;
            string userName = "user";
            int licenseLevel = 3;
            var newSessionId = Guid.NewGuid();
            var oldSessionId = Guid.NewGuid();
            var session = new Session { SessionId = newSessionId };
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), userName, licenseLevel, false, It.IsAny<Action<Guid>>()))
                .Returns((int i, string n, int l, bool s, Action<Guid> a) => { a(oldSessionId); return Task.FromResult(session); });

            // Act
            await _controller.PostSession(uid, userName, licenseLevel);

            // Assert
            var token = Session.Convert(newSessionId);
            _cacheMock.Verify(m => m.Remove(Session.Convert(oldSessionId), null));
            _cacheMock.Verify(m => m.Set(token, session, It.Is<CacheItemPolicy>(p => VerifyPolicy(p, token)), null));
        }

        [TestMethod]
        public async Task PostSession_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            int uid = 999;
            string userName = "user";
            int licenseLevel = 3;
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), userName, licenseLevel, false, It.IsAny<Action<Guid>>())).Throws<Exception>();

            // Act
            var result = await _controller.PostSession(uid, userName, licenseLevel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
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
        public async Task PutSession_SessionDoesNotExist_NotFound()
        {
            // Arrange
            var guid = Guid.NewGuid();
            _controller.Request.Headers.Add("Session-Token", Session.Convert(guid));
            _sessionsRepoMock.Setup(r => r.GetSession(guid)).ReturnsAsync(null);

            // Act
            var resultSession = await _controller.PutSession("", 1);

            // Assert
            Assert.IsInstanceOfType(resultSession, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutSession_SessionExpired_NotFound()
        {
            // Arrange
            var guid = Guid.NewGuid();
            _controller.Request.Headers.Add("Session-Token", Session.Convert(guid));
            var session = new Session { EndTime = DateTime.MaxValue };
            _sessionsRepoMock.Setup(r => r.GetSession(guid)).ReturnsAsync(session);

            // Act
            var resultSession = await _controller.PutSession("", 1);

            // Assert
            Assert.IsInstanceOfType(resultSession, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutSession_SessionExists_UpdatesCacheAndReturnsSession()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var token = Session.Convert(guid);
            _controller.Request.Headers.Add("Session-Token", token);
            var session = new Session { SessionId = guid };
            _sessionsRepoMock.Setup(c => c.ExtendSession(guid)).ReturnsAsync(session);

            // Act
            var result = await _controller.PutSession("", 1) as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Response.IsSuccessStatusCode);
            Assert.AreEqual(session, await result.Response.Content.ReadAsAsync<Session>());
            Assert.AreEqual(token, result.Response.Headers.GetValues("Session-Token").Single());
            _cacheMock.Verify(c => c.Set(token, session, It.Is<CacheItemPolicy>(p => VerifyPolicy(p, token)), null));
        }

        [TestMethod]
        public async Task PutSession_MissingToken_BadRequest()
        {
            // Arrange

            // Act
            var result = await _controller.PutSession("", 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task PutSession_MalformedToken_BadRequest()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", "bad token");

            // Act
            var result = await _controller.PutSession("", 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task PutSession_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            var guid = Guid.NewGuid();
            _controller.Request.Headers.Add("Session-Token", Session.Convert(guid));
            _sessionsRepoMock.Setup(r => r.ExtendSession(guid)).Throws<Exception>();

            // Act
            var result = await _controller.PutSession("", 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion PutSession

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
