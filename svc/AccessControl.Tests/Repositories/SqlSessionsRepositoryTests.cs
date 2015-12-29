using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models;

namespace AccessControl.Repositories
{
    [TestClass]
    public class SqlSessionsRepositoryTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesConnectionToAdminStorage()
        {
            // Arrange

            // Act
            var repository = new SqlSessionsRepository();

            // Assert
            Assert.AreEqual(WebApiConfig.AdminStorage, repository._connectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region GetSession

        [TestMethod]
        public async Task GetSession_QueryReturnsSession_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            var guid = new Guid("12345678901234567890123456789012");
            Session[] result = { new Session {SessionId = guid } };
            cxn.SetupQueryAsync("GetSession", new Dictionary<string, object> { { "SessionId", guid } }, result);

            // Act
            Session session = await repository.GetSession(guid);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), session);
        }

        [TestMethod]
        public async Task GetSession_QueryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            var guid = new Guid("12345678901234567890123456789012");
            Session[] result = {};
            cxn.SetupQueryAsync("GetSession", new Dictionary<string, object> { { "SessionId", guid } }, result);

            // Act
            Session session = await repository.GetSession(guid);

            // Assert
            cxn.Verify();
            Assert.IsNull(session);
        }

        #endregion GetSession

        #region GetUserSession

        [TestMethod]
        public async Task GetUserSession_QueryReturnsSession_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            int uid = 1;
            Session[] result = { new Session { UserId = uid } };
            cxn.SetupQueryAsync("GetUserSession", new Dictionary<string, object> { { "UserId", uid } }, result);

            // Act
            Session session = await repository.GetUserSession(uid);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), session);
        }

        [TestMethod]
        public async Task GetUserSession_QueryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            int uid = 5;
            Session[] result = { };
            cxn.SetupQueryAsync("GetUserSession", new Dictionary<string, object> { { "UserId", uid } }, result);

            // Act
            Session session = await repository.GetUserSession(uid);

            // Assert
            cxn.Verify();
            Assert.IsNull(session);
        }

        #endregion GetUserSession

        #region SelectSessions

        [TestMethod]
        public async Task SelectSessions_QueryReturnsSessions_ReturnsAll()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            int ps = 100;
            int pn = 1;
            Session[] result =
            {
                new Session {SessionId = new Guid("12345678901234567890123456789012")},
                new Session {SessionId = new Guid("11111111111111111111111111111111")}
            };
            cxn.SetupQueryAsync("SelectSessions", new Dictionary<string, object> { { "ps", ps }, { "pn", pn } }, result);

            // Act
            IEnumerable<Session> sessions = await repository.SelectSessions(ps, pn);

            // Assert
            cxn.Verify();
            CollectionAssert.AreEquivalent(result, sessions.ToList());
        }

        [TestMethod]
        public async Task SelectSessions_QueryReturnsEmpty_ReturnsEmpty()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            int ps = 100;
            int pn = 1;
            Session[] result = {};
            cxn.SetupQueryAsync("SelectSessions", new Dictionary<string, object> { { "ps", ps }, { "pn", pn } }, result);

            // Act
            IEnumerable<Session> sessions = await repository.SelectSessions(ps, pn);

            // Assert
            cxn.Verify();
            Assert.IsFalse(sessions.Any());
        }

        #endregion SelectSessions

        #region BeginSession

        [TestMethod]
        public async Task BeginSession_QueryReturnsNewAndOldSessions_ReturnsBoth()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            int id = 123;
            string userName = "user";
            int licenseLevel = 3;
            Guid? newSession = new Guid("12345678901234567890123456789012");
            Guid? oldSession = new Guid("11111111111111111111111111111111");
            cxn.SetupExecuteAsync(
                "BeginSession",
                new Dictionary<string, object> { { "UserId", id }, { "NewSessionId", null }, { "OldSessionId", null }, { "UserName", userName }, { "LicenseLevel", licenseLevel }, { "IsSso", false } },
                1,
                new Dictionary<string, object> { { "NewSessionId", newSession }, { "OldSessionId", oldSession } });

            // Act
            Guid?[] sessions = await repository.BeginSession(id, userName, licenseLevel, false);

            // Assert
            cxn.Verify();
            CollectionAssert.AreEqual(new [] { newSession, oldSession }, sessions);
        }

        [TestMethod]
        public async Task BeginSession_QueryReturnsNewSessionOnly_ReturnsNewSessionAndNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            int id = 123;
            string userName = "user";
            int licenseLevel = 3;
            Guid? newSession = new Guid("12345678901234567890123456789012");
            cxn.SetupExecuteAsync(
                "BeginSession",
                new Dictionary<string, object> { { "UserId", id }, { "NewSessionId", null }, { "OldSessionId", null }, { "UserName", userName }, { "LicenseLevel", licenseLevel }, { "IsSso", true } },
                1,
                new Dictionary<string, object> { { "NewSessionId", newSession }, { "OldSessionId", null } });

            // Act
            Guid?[] sessions = await repository.BeginSession(id, userName, licenseLevel, true);

            // Assert
            cxn.Verify();
            CollectionAssert.AreEqual(new[] { newSession, null }, sessions);
        }

        #endregion BeginSession

        #region EndSession

        [TestMethod]
        public async Task EndSession_Logout_CallsProcedureWithCorrectParameters()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            var guid = new Guid("12345678901234567890123456789012");
            cxn.SetupExecuteAsync("EndSession", new Dictionary<string, object> { { "SessionId", guid }, { "Timeout", 0 } }, 1);

            // Act
            await repository.EndSession(guid, false);

            // Assert
            cxn.Verify();
        }

        [TestMethod]
        public async Task EndSession_Timeout_CallsProcedureWithCorrectParameters()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSessionsRepository(cxn.Object);
            var guid = new Guid("00000000000000000000000000000000");
            cxn.SetupExecuteAsync("EndSession", new Dictionary<string, object> { { "SessionId", guid }, { "Timeout", 0 } }, 1);

            // Act
            await repository.EndSession(guid, true);

            // Assert
            cxn.Verify();
        }

        #endregion EndSession
    }
}
