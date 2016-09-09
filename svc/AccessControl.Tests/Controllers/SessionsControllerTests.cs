using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AccessControl.Helpers;
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
        private Mock<ITimeoutManager<Guid>> _cacheMock;
        private SessionsController _controller;

        [TestInitialize]
        public void Initialize()
        {
            _sessionsRepoMock = new Mock<ISessionsRepository>();
            _cacheMock = new Mock<ITimeoutManager<Guid>>();
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
            Assert.IsInstanceOfType(controller._sessions, typeof(TimeoutManager<Guid>));
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
        public async Task GetSession_SessionNotExpired_ReturnsSession()
        {
            // Arrange
            int uid = 999;
            var session = new Session { EndTime = DateTime.UtcNow.AddDays(1) };
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
            var session = new Session { SessionId = newSessionId, EndTime = DateTime.UtcNow.AddDays(1) };
            _sessionsRepoMock.Setup(r => r.BeginSession(It.IsAny<int>(), userName, licenseLevel, false, It.IsAny<Action<Guid>>()))
                .Returns((int i, string n, int l, bool s, Action<Guid> a) => { a(oldSessionId); return Task.FromResult(session); });

            // Act
            await _controller.PostSession(uid, userName, licenseLevel);

            // Assert
            _cacheMock.Verify(m => m.Remove(oldSessionId));
            _cacheMock.Verify(m => m.Insert(newSessionId, session.EndTime, It.Is<Action>(a => VerifyCallback(a, session))));
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

            // Act
            var result = await _controller.SelectSessions("0", "-1");

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task SelectSession_RepositoryThrowsException_BadRequest()
        {
            // Arrange
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

        #endregion SelectSession

        #region DeleteSession

        [TestMethod]
        public async Task DeleteSession_SessionDoesNotExist_NotFound()
        {
            // Arrange
            var guid = new Guid();
            _controller.Request.Headers.Add("Session-Token", Session.Convert(guid));
            _sessionsRepoMock.Setup(repo => repo.EndSession(guid, null)).ReturnsAsync(null);

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task DeleteSession_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            var guid = Guid.NewGuid();
            _controller.Request.Headers.Add("Session-Token", Session.Convert(guid));
            _sessionsRepoMock.Setup(repo => repo.EndSession(guid, null)).Throws<Exception>();

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task DeleteSession_MissingToken_BadRequest()
        {
            // Arrange

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task DeleteSession_SessionExists_OkResult()
        {
            // Arrange
            var guid = Guid.NewGuid();
            _controller.Request.Headers.Add("Session-Token", Session.Convert(guid));
            var session = new Session { SessionId = guid };
            _sessionsRepoMock.Setup(r => r.EndSession(guid, null)).ReturnsAsync(session);

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            _cacheMock.Verify(c => c.Remove(guid));
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task DeleteSession_MalformedToken_BadRequest()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", "bad token");

            // Act
            var result = await _controller.DeleteSession();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
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
            var session = new Session { EndTime = DateTime.UtcNow.AddDays(1) };
            _sessionsRepoMock.Setup(r => r.GetSession(guid)).ReturnsAsync(session);

            // Act
            var resultSession = await _controller.PutSession("", 1);

            // Assert
            Assert.IsInstanceOfType(resultSession, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutSession_SessionNotExpired_UpdatesCacheAndReturnsSession()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var token = Session.Convert(guid);
            _controller.Request.Headers.Add("Session-Token", token);
            var session = new Session { SessionId = guid, EndTime = DateTime.UtcNow.AddDays(1) };
            _sessionsRepoMock.Setup(c => c.ExtendSession(guid)).ReturnsAsync(session);

            // Act
            var result = await _controller.PutSession("", 1) as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Response.IsSuccessStatusCode);
            Assert.AreEqual(session, await result.Response.Content.ReadAsAsync<Session>());
            Assert.AreEqual(token, result.Response.Headers.GetValues("Session-Token").Single());
            _cacheMock.Verify(c => c.Insert(guid, session.EndTime, It.Is<Action>(a => VerifyCallback(a, session))));
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

        #region LoadAsync

        [TestMethod]
        public async Task LoadAsync_RepositoryReturnsSessions_CachesSessions()
        {
            // Arrange
            var session = new Session { SessionId = Guid.NewGuid(), EndTime = DateTime.UtcNow.AddDays(1) };
            var sessions = new List<Session> { session };
            _sessionsRepoMock.Setup(repo => repo.SelectSessions(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(sessions);

            // Act
            await _controller.LoadAsync();

            // Assert
            _cacheMock.Verify(m => m.Insert(session.SessionId, session.EndTime, It.Is<Action>(p => VerifyCallback(p, session))));
        }

        [TestMethod]
        public async Task LoadAsync_RepositoryThrowsException_LogsError()
        {
            // Arrange
            _sessionsRepoMock.Setup(repo => repo.SelectSessions(It.IsAny<int>(), It.IsAny<int>())).Throws<Exception>();

            // Act
            await _controller.LoadAsync();

            // Assert
            _logMock.Verify(l => l.LogError(WebApiConfig.LogSourceSessions, It.Is<Exception>(e => e.Message == "Error loading sessions from database."),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        #endregion LoadAsync

        private bool VerifyCallback(Action callback, Session session)
        {
            callback();
            _sessionsRepoMock.Verify(r => r.EndSession(session.SessionId, session.EndTime));
            return true;
        }
    }
}
