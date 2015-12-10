using System.Net;

using NUnit.Framework;
using System.Collections.Generic;
using CustomAttributes;
using Model.Facades;
using Model.Factories;
using TestConfig;
using Helper.Factories;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public class SessionsTests
    {
        private const string _serviceRoute = "/svc/accesscontrol/";
        private const string _sessionRoute = "sessions/";
        private static TestConfiguration _testConfig = TestConfiguration.GetInstance();
        private static string _sessionUrl = BlueprintServerFactory.GetBlueprintServerFromTestConfig().Address + _serviceRoute + _sessionRoute;

        [TearDown]
        public static void TearDown()
        {
            //TODO: here we can put query 'delete FROM [AdminStorage].[dbo].[Sessions]'
        }

        [Test]
        [Explicit(IgnoreReasons.ProductBug)]
        public static void PostNewSession_OK()
        {
            Session expectSession = Session.CreateSession();
            Dictionary<string, string> headers = GetSessionToken(expectSession);
            var session = WebRequestFacade.GetWebResponseFacade(_sessionUrl + expectSession.UserId, "GET", headers);
            Assert.AreEqual(session.StatusCode, HttpStatusCode.OK, "'GET {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.OK, session.StatusCode);
            Assert.True(expectSession.Equals(session.GetBlueprintObject<Session>()), "UserID from returned session object must be equal to UserID used during session creation");
        }

        [Test]
        [Explicit(IgnoreReasons.ProductBug)]
        public static void PutSession_OK()
        {
            int artifactId = RandomGenerator.RandomNumber();
            Session expectedSession = Session.CreateSession();
            Dictionary<string, string> headers = GetSessionToken(expectedSession);
            string action = "do_some_artifact_action/";//in current implementation it is an optional parameter to identify operation

            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl + action + artifactId, "PUT", headers);
            var session = response.GetBlueprintObject<Session>();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "'PUT {0}' should return {1}, but failed with {2}",
                _sessionUrl + action + expectedSession.UserId, HttpStatusCode.OK, response.StatusCode);
            Assert.True(expectedSession.Equals(session));
        }

        [Test]
        [Explicit(IgnoreReasons.ProductBug)]
        public static void PutSession_BadRequest()
        {
            Session expectedSession = Session.CreateSession();
            var requestToken = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(expectedSession.UserId,
                expectedSession.UserName, expectedSession.LicenseLevel), "POST");

            Assert.AreEqual(requestToken.StatusCode, HttpStatusCode.OK, "'POST {0}' should return {1}, but failed with {2}",
                _sessionUrl + expectedSession.UserId, HttpStatusCode.OK, requestToken.StatusCode);

            string action = "do_some_action/";//in current implementation it is an optional parameter to identify operation
            var session = WebRequestFacade.GetWebResponseFacade(_sessionUrl + action + RandomGenerator.RandomNumber(), "PUT");

            Assert.AreEqual(HttpStatusCode.BadRequest, session.StatusCode, "'PUT {0}' should return {1}, but failed with {2}",
                _sessionUrl + action, HttpStatusCode.BadRequest, session.StatusCode);
        }

        [Test]
        [Explicit(IgnoreReasons.ProductBug)]
        public static void GetSessionsWithoutSessionToken_NotFound()
        {
            Session expectedSession = Session.CreateSession();
            GetSessionToken(expectedSession);

            var response = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(expectedSession.UserId,
                expectedSession.UserName, 3));
            var session = response.GetBlueprintObject<Session>();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "'GET {0}' should return {1}, but failed with {2}",
                _sessionUrl + expectedSession.UserId, HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        [Explicit(IgnoreReasons.ProductBug)]
        public static void DeleteSessions_OK()
        {
            int userId = 2311;//in current implementation of AccessControl we can put any Int
            string userName = RandomGenerator.RandomAlphaNumeric(7);
            Session expectedSession = Session.CreateSession();
            Dictionary<string, string> headers = GetSessionToken(expectedSession);

            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl, "DELETE", headers);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "'DELETE {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        [Explicit(IgnoreReasons.ProductBug)]
        public static void DeleteSessions_BadRequest()
        {
            Session expectedSession = Session.CreateSession();
            var requestToken = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(expectedSession.UserId,
                expectedSession.UserName, 3), "POST");

            Assert.AreEqual(requestToken.StatusCode, HttpStatusCode.OK, "'POST {0}' should return {1}, but failed with {2}",
                _sessionUrl + expectedSession.UserId, HttpStatusCode.OK, requestToken.StatusCode);
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
        private static Dictionary<string, string> GetSessionToken(Session session)
        {
            var requestToken = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(session.UserId, session.UserName,
                session.LicenseLevel), "POST");

            Assert.AreEqual(requestToken.StatusCode, HttpStatusCode.OK, "'POST {0}' should return {1}, but failed with {2}",
                _sessionUrl + session.UserId, HttpStatusCode.OK, requestToken.StatusCode);

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
