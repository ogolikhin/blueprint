using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public class SessionsTests : TestBase
    {
        private const string GET_SESSION_PATH = RestPaths.Svc.AccessControl.SESSIONS_id_;
        private IUser _adminUser = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            Logger.WriteTrace("TearDown() is deleting all sessions created by the tests...");
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase]
        [TestRail(96103)]
        [Description("Check that POST new session returns 200 OK")]
        public void PostNewSession_Verify200OK()
        {
            // POST the new session.  There should be no errors.
            CreateAndAddSessionToAccessControl();
        }

        [TestCase(null, null)]
        [TestCase(null, 1234)]
        [TestCase("do_some_action", null)]
        [TestCase("do_some_action", 1234)]
        [TestRail(96104)]
        [Description("Check that PUT for valid Session returns 200 OK")]
        public void PutSessionWithSessionToken_Verify200OK(string operation, int? artifactId)
        {
            var createdSession = CreateAndAddSessionToAccessControl();

            var returnedSession = Helper.AccessControl.AuthorizeOperation(createdSession, operation, artifactId);

            Assert.That(returnedSession.Equals(createdSession), "The POSTed session doesn't match the PUT session!");
        }

        [TestCase]
        [TestRail(96111)]
        [Description("Check that AccessControl returns active Session-Token for the user.")]
        public void GetSessionForUserIdWithActiveSession_VerifyPostAndGetSessionsMatch()
        {
            var addedSession = CreateAndAddSessionToAccessControl();

            var returnedSession = Helper.AccessControl.GetSession(addedSession.UserId);

            Assert.That(addedSession.Equals(returnedSession), "'GET {0}' returned a different session than the one we added!", addedSession.UserId);
        }

        [TestCase]
        [TestRail(96103)]
        [Description("Check that DELETE session returns 200 OK")]
        public void DeleteSessionWithSessionToken_Verify200OK()
        {
            // Setup: Create a session to be deleted.
            var createdSession = CreateAndAddSessionToAccessControl();

            // Delete the session.  We should get no errors.
            Helper.AccessControl.DeleteSession(createdSession);
        }

        [TestCase]
        [TestRail(96108)]
        [Description("Check that GET active sessiones returns active sessions.")]
        public void GetActiveSessions_VerifySessionsWereFound()
        {
            // Setup: Create a session for test.
            var session = CreateAndAddSessionToAccessControl();

            // TODO: add expected results
            var sessionsList = Helper.AccessControl.GetActiveSessions(session: session);
            Assert.That(sessionsList.Count > 0, "GetActiveSessions should find at least 1 active session, but found none.");
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase]
        [TestRail(96116)]
        [Description("Put session without SessionToken returns 400.")]
        public void PutSessionWithoutSessionToken_Verify400BadRequest()
        {
            var session = SessionFactory.CreateSession(_adminUser);
            int artifactId = RandomGenerator.RandomNumber();

            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AccessControl.AuthorizeOperation(session, "do_some_action", artifactId);
            }, "Calling PUT without a session token should return 400 Bad Request!");
        }

        [TestCase]
        [TestRail(96106)]
        [Description("Check that DELETE session without SessionToken returns 400 BadRequest")]
        public void DeleteSessionsWithoutSessionToken_Verify400BadRequest()
        {
            // Call the DELETE RestAPI without a session token which should return a 400 error.
            Assert.Throws<Http400BadRequestException>(() => { Helper.AccessControl.DeleteSession(null); });
        }

        [TestCase("*")]
        [TestCase("&")]
        [TestRail(246571)]
        [Description("GetSession using the invalid URL containing a special character. Verify that 400 bad request is returned.")]
        public void GetSession_SendInvalidUrl_400BadRequest(string invalidCharacter)
        {
            // Setup:
            int nonExistingUserId = int.MaxValue;
            string invalidPath = I18NHelper.FormatInvariant(GET_SESSION_PATH, invalidCharacter + nonExistingUserId);

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, _adminUser?.Token?.AccessControlToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestAndDeserializeObject<Session>(
                invalidPath,
                RestRequestMethod.GET,
                shouldControlJsonChanges: true
                ),
                "GET {0} call should return a 400 Bad Request exception when trying with invalid URL.", GET_SESSION_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant(
                "A potentially dangerous Request.Path value was detected from the client ({0}).", invalidCharacter);

            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedMessage);
        }

        #endregion 400 Bad Request Tests

        #region 404 Not Found Tests

        [TestCase]
        [TestRail(96112)]
        [Description("Get Session-Token for user without active session returns 404 - Not Found.")]
        public void GetSessionsForUserIdWithNoActiveSessions_Verify404NotFound()
        {
            // Add 1 session to AccessControl.
            CreateAndAddSessionToAccessControl();

            // Now create another session, but don't add it to AccessControl.
            var session = SessionFactory.CreateRandomSession();

            // Try to get the session without the session token, which should give a 404 error.
            Assert.Throws<Http404NotFoundException>(() => { Helper.AccessControl.GetSession(session.UserId); });
        }

        #endregion 404 Not Found Tests

        #region Private Functions

        /// <summary>
        /// Adds (POST's) the specified session to the AccessControl service.
        /// </summary>
        /// <param name="session">The session to add.</param>
        /// <returns>The session that was added including the session token.</returns>
        private ISession AddSessionToAccessControl(ISession session)
        {
            // POST the new session.
            var createdSession = Helper.AccessControl.AddSession(session);

            // Verify that the POST returned the expected session.
            Assert.True(session.Equals(createdSession),
                "POST returned a different session than expected!  Got '{0}', but expected '{1}'.",
                createdSession.SessionId, session.SessionId);

            return createdSession;
        }

        /// <summary>
        /// Creates a random session and adds (POST's) it to the AccessControl service.
        /// </summary>
        /// <returns>The session that was added including the session token.</returns>
        private ISession CreateAndAddSessionToAccessControl()
        {
            var session = SessionFactory.CreateSession(_adminUser);

            // POST the new session.
            return AddSessionToAccessControl(session);
        }

        #endregion Private Functions
    }
}
