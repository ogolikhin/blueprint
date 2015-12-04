using System;
using System.Net;

using NUnit.Framework;
using System.Collections.Generic;
using Model.Facades;
using Model.Factories;

namespace AccessControlTests
{ 
    [TestFixture]
    public static class SessionsTests
    {
        private const string _serverUrl = "http://localhost:9801/";//TODO: replace with TestConfiguration.GetInstance();
        private const string _serviceRoute = "svc/accesscontrol/";
        private const string _sessionRoute = "sessions/";
        private const string _sessionUrl = _serverUrl + _serviceRoute + _sessionRoute;

        [TearDown]
        public static void TearDown()
        {
            //TODO: here we can put query 'delete FROM [AdminStorage].[dbo].[Sessions]'
        }

        [Test]
        [Explicit("we don't have deployed access control service")]
        public static void PostNewSession_OK()
        {
            int userId = 12534;//in current implementation of AccessControl we can put any Int
            string userName = RandomGenerator.RandomAlphaNumeric(7);
            Dictionary<string, string> headers = GetSessionToken(userId, userName, 3);

            var session = WebRequestFacade.GetWebResponseFacade(_sessionUrl + userId.ToString(), "GET", headers);

            Assert.AreEqual(session.StatusCode, HttpStatusCode.OK, "'GET {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.OK, session.StatusCode);
            Assert.AreEqual(session.GetBlueprintObject<Session>().UserId, userId,
                "UserID from returned session object must be equal to UserID used during session creation");
        }

        [Test]
        [Explicit("we don't have deployed access control service")]
        public static void PutSession_OK()
        {
            int userId = 1124;//in current implementation of AccessControl we can put any Int
            string userName = RandomGenerator.RandomAlphaNumeric(7);
            int artifactId = 5687;
            Dictionary<string, string> headers = GetSessionToken(userId, userName, 3);
            string action = "do_some_artifact_action/";//in current implementation it is an optional parameter to identify operation

            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl + action + artifactId, "PUT", headers);
            var session = response.GetBlueprintObject<Session>();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "'PUT {0}' should return {1}, but failed with {2}",
                _sessionUrl + action + userId.ToString(), HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(userId, session.UserId);
            Assert.AreEqual(userName, session.UserName);
        }

        [Test]
        [Explicit("we don't have deployed access control service")]
        public static void PutSession_BadRequest()
        {
            int userId = 1124;//in current implementation of AccessControl we can put any Int
            string userName = RandomGenerator.RandomAlphaNumeric(7);
            var requestToken = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(userId, userName, 3), "POST");

            Assert.AreEqual(requestToken.StatusCode, HttpStatusCode.OK, "'POST {0}' should return {1}, but failed with {2}",
                _sessionUrl + userId.ToString(), HttpStatusCode.OK, requestToken.StatusCode);

            string action = "do_some_action/";//in current implementation it is an optional parameter to identify operation
            var session = WebRequestFacade.GetWebResponseFacade(_sessionUrl + action + userId.ToString(), "PUT");

            Assert.AreEqual(HttpStatusCode.BadRequest, session.StatusCode, "'PUT {0}' should return {1}, but failed with {2}",
                _sessionUrl + action + userId.ToString(), HttpStatusCode.BadRequest, session.StatusCode);
        }

        [Test]
        [Explicit("we don't have deployed access control service")]
        public static void GetSessionsWithoutSessionToken_OK()
        {
            int userId = 12434;//in current implementation of AccessControl we can put any Int
            string userName = RandomGenerator.RandomAlphaNumeric(7);
            GetSessionToken(userId, userName, 3);

            var response = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(userId, userName, 3));
            var session = response.GetBlueprintObject<Session>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "'GET {0}' should return {1}, but failed with {2}",
                _sessionUrl + userId.ToString(), HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(userId, session.UserId);
            Assert.AreEqual(userName, session.UserName);
        }

        [Test]
        [Explicit("we don't have deployed access control service")]
        public static void DeleteSessions_OK()
        {
            int userId = 2311;//in current implementation of AccessControl we can put any Int
            string userName = RandomGenerator.RandomAlphaNumeric(7);
            Dictionary<string, string> headers = GetSessionToken(userId, userName, 3);

            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl, "DELETE", headers);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "'DELETE {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        [Explicit("we don't have deployed access control service")]
        public static void DeleteSessions_BadRequest()
        {
            int userId = 2311;//in current implementation of AccessControl we can put any Int
            string userName = RandomGenerator.RandomAlphaNumeric(7);
            var requestToken = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(userId, userName, 3), "POST");

            Assert.AreEqual(requestToken.StatusCode, HttpStatusCode.OK, "'POST {0}' should return {1}, but failed with {2}",
                _sessionUrl + userId.ToString(), HttpStatusCode.OK, requestToken.StatusCode);

            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl, "DELETE");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "'DELETE {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Returns header with Session-Token.
        /// </summary>
        /// <param name="userId">Id to use for Session-Token creation</param>
        /// <param name="userName">username</param>
        /// <param name="licenseLevel">license - now any int</param>
        /// <param name="isSso">boolean value</param>
        /// <returns>Header with valid Session-Token</returns>
        private static Dictionary<string, string> GetSessionToken(int userId, string userName, int licenseLevel, bool isSso=true)
        {
            var requestToken = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(userId, userName, licenseLevel), "POST");

            Assert.AreEqual(requestToken.StatusCode, HttpStatusCode.OK, "'POST {0}' should return {1}, but failed with {2}",
                _sessionUrl + userId.ToString(), HttpStatusCode.OK, requestToken.StatusCode);

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {"Session-Token", requestToken.GetHeaderValue("Session-Token")}
            };

            return headers;
        }

        /// <summary>
        /// Creates url for PostSession request.
        /// </summary>
        /// <param name="userId">Id to use for Session-Token creation</param>
        /// <param name="userName">username</param>
        /// <param name="licenseLevel">license - now any int</param>
        /// <param name="isSso">boolean value</param>
        /// <returns>url for Session object</returns>
        private static string CreateSessionUrl(int userId, string userName, int licenseLevel, bool isSso = true)
        {
            return _sessionUrl + userId.ToString() + "?userName=" + userName + "&licenseLevel=" +
                   licenseLevel.ToString() + "&isSso=" + isSso.ToString();
        }
    }
}
