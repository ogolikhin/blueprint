using System.Net;

using NUnit.Framework;
using System.Collections.Generic;
using CustomAttributes;
using Model.Facades;
using Model.Factories;
using TestConfig;
using Helper.Factories;
using Logging;
using Model;
using Model.Impl;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public class SessionsTests
    {
        private const string _serviceRoute = "svc/accesscontrol/";
        private const string _sessionRoute = "sessions/";

        private static Dictionary<string, Service> _services = TestConfiguration.GetInstance().Services;
        private static string _sessionUrl = _services["AccessControl"].Address + _serviceRoute + _sessionRoute;

        private IAccessControl _accessControl = AccessControlFactory.GetAccessControlFromTestConfig();
        private ISession _randomSession = null;


        [SetUp]
        public void SetUp()
        {
            // Setup: Create a random session.
            _randomSession = SessionFactory.CreateRandomSession();
        }

        [TearDown]
        public void TearDown()
        {
            // Delete all sessions created by the tests.
            _accessControl.Sessions.ForEach(s => _accessControl.DeleteSession(s));
        }

        [Test]
        public void PostNewSession_OK()
        {
            // POST the new session.
            ISession createdSession = _accessControl.CreateSession(_randomSession);

            // Verify that the POST returned the expected session.
            Assert.True(_randomSession.Equals(createdSession),
                "POST returned a different session than expected!  Got '{0}', but expected '{1}'.",
                createdSession.SessionId, _randomSession.SessionId);

            // Verify that the session was saved by AccessControl.
            ISession session = _accessControl.GetSession(_randomSession.UserId);
            Assert.True(_randomSession.Equals(session),
                "GET returned a different session than expected!  Got '{0}', but expected '{1}'.",
                session.SessionId, _randomSession.SessionId);
        }

        [Test]
        public static void PutSession_OK()
        {
            int artifactId = RandomGenerator.RandomNumber();
            ISession expectedSession = SessionFactory.CreateRandomSession();
            Dictionary<string, string> headers = GetSessionToken(expectedSession);
            string action = "do_some_artifact_action/";//in current implementation it is an optional parameter to identify operation

            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl + action + artifactId, "PUT", headers);
            var session = response.GetBlueprintObject<Session>();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "'PUT {0}' should return {1}, but failed with {2}",
                _sessionUrl + action + expectedSession.UserId, HttpStatusCode.OK, response.StatusCode);
            Assert.True(expectedSession.Equals(session));
            DeleteSession(headers);
        }

        [Test]
        public static void PutSession_BadRequest()
        {
            ISession expectedSession = SessionFactory.CreateRandomSession();
            Dictionary<string, string> headers = GetSessionToken(expectedSession);
            
            string action = "do_some_action/";//in current implementation it is an optional parameter to identify operation
            var session = WebRequestFacade.GetWebResponseFacade(_sessionUrl + action + RandomGenerator.RandomNumber(), "PUT");

            Assert.AreEqual(HttpStatusCode.BadRequest, session.StatusCode, "'PUT {0}' should return {1}, but failed with {2}",
                _sessionUrl + action, HttpStatusCode.BadRequest, session.StatusCode);
            DeleteSession(headers);
        }

        [Test]
        public static void GetSessionsWithoutSessionToken_OK()
        {
            ISession expectedSession = SessionFactory.CreateRandomSession();
            Dictionary<string, string> headers = GetSessionToken(expectedSession);

            var response = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(expectedSession));
            var session = response.GetBlueprintObject<Session>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "'GET {0}' should return {1}, but failed with {2}",
                _sessionUrl + expectedSession.UserId, HttpStatusCode.NotFound, response.StatusCode);
            DeleteSession(headers);
            Assert.True(expectedSession.Equals(session));
        }

        [Test]
        public static void DeleteSessions_OK()
        {
            ISession expectedSession = SessionFactory.CreateRandomSession();
            Dictionary<string, string> headers = GetSessionToken(expectedSession);

            var response = DeleteSession(headers);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "'DELETE {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public static void DeleteSessions_BadRequest()
        {
            ISession expectedSession = SessionFactory.CreateRandomSession();
            Dictionary<string, string> headers = GetSessionToken(expectedSession);

            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl, "DELETE");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "'DELETE {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.BadRequest, response.StatusCode);
            DeleteSession(headers);
        }

        /// <summary>
        /// Returns header with Session-Token.
        /// </summary>
        /// <param name="userId">Id to use for Session-Token creation</param>
        /// <param name="userName">username</param>
        /// <param name="licenseLevel">license - now any int</param>
        /// <param name="isSso">boolean value</param>
        /// <returns>Header with valid Session-Token</returns>
        private static Dictionary<string, string> GetSessionToken(ISession session)
        {
            var requestToken = WebRequestFacade.GetWebResponseFacade(CreateSessionUrl(session) + "?userName=" +
                session.UserName + "&licenseLevel=" + session.LicenseLevel + "&isSso=" + session.IsSso, "POST");

            Assert.AreEqual(requestToken.StatusCode, HttpStatusCode.OK, "'POST {0}' should return {1}, but failed with {2}",
                _sessionUrl + session.UserId, HttpStatusCode.OK, requestToken.StatusCode);

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {"Session-Token", requestToken.GetHeaderValue("Session-Token")}
            };

            return headers;
        }

        private static WebResponseFacade DeleteSession(Dictionary <string, string> headers)
        {
            return WebRequestFacade.GetWebResponseFacade(_sessionUrl, "DELETE", headers);
        }

        /// <summary>
        /// Creates url for PostSession request.
        /// </summary>
        /// <param name="userId">Id to use for Session-Token creation</param>
        /// <param name="userName">username</param>
        /// <param name="licenseLevel">license - now any int</param>
        /// <param name="isSso">boolean value</param>
        /// <returns>url for Session object</returns>
        private static string CreateSessionUrl(ISession session)
        {
            return _sessionUrl + session.UserId;
        }
    }
}
