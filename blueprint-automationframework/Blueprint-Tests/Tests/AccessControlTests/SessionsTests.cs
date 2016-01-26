using NUnit.Framework;
using CustomAttributes;
using Model.Factories;
using Common;
using Model;
using Utilities;
using Utilities.Factories;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public class SessionsTests
    {
        private IAccessControl _accessControl = AccessControlFactory.GetAccessControlFromTestConfig();
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            Logger.WriteTrace("TearDown() is deleting all sessions created by the tests...");

            // Delete all sessions created by the tests.
            foreach (var session in _accessControl.Sessions.ToArray())
            {
                _accessControl.DeleteSession(session);
            }
            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        /// <summary>
        /// Adds (POST's) the specified session to the AccessControl service.
        /// </summary>
        /// <param name="session">The session to add.</param>
        /// <returns>The session that was added including the session token.</returns>
        private ISession AddSessionToAccessControl(ISession session)
        {
            // POST the new session.
            ISession createdSession = _accessControl.AddSession(session);

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
            ISession session = SessionFactory.CreateSession(_user);

            // POST the new session.
            return AddSessionToAccessControl(session);
        }

        [Test]
        public void PostNewSession_Verify200OK()
        {
            // POST the new session.  There should be no errors.
            CreateAndAddSessionToAccessControl();
        }

        [TestCase(null, null)]
        [TestCase(null, 1234)]
        [TestCase("do_some_action", null)]
        [TestCase("do_some_action", 1234)]
        public void PutSessionWithSessionToken_Verify200OK(string operation, int? artifactId)
        {
            ISession createdSession = CreateAndAddSessionToAccessControl();

            ISession returnedSession = _accessControl.AuthorizeOperation(createdSession, operation, artifactId);

            Assert.That(returnedSession.Equals(createdSession), "The POSTed session doesn't match the PUT session!");
        }

        [Test]
        public void PutSessionWithoutSessionToken_Verify400BadRequest()
        {
            ISession session = SessionFactory.CreateSession(_user);
            int artifactId = RandomGenerator.RandomNumber();

            Assert.Throws<Http400BadRequestException>(() =>
            {
                _accessControl.AuthorizeOperation(session, "do_some_action", artifactId);
            }, "Calling PUT without a session token should return 400 Bad Request!");
        }

        [Test]
        public void GetSessionForUserIdWithActiveSession_VerifyPostAndGetSessionsMatch()
        {
            ISession addedSession = CreateAndAddSessionToAccessControl();

            ISession returnedSession = _accessControl.GetSession(addedSession.UserId);

            Assert.That(addedSession.Equals(returnedSession), "'GET {0}' returned a different session than the one we added!", addedSession.UserId);
        }

        [Test]
        public void GetSessionsForUserIdWithNoActiveSessions_Verify404NotFound()
        {
            // Add 1 session to AccessControl.
            CreateAndAddSessionToAccessControl();

            // Now create another session, but don't add it to AccessControl.
            ISession session = SessionFactory.CreateRandomSession();

            // Try to get the session without the session token, which should give a 404 error.
            Assert.Throws<Http404NotFoundException>(() => { _accessControl.GetSession(session.UserId); });
        }

        [Test]
        public void DeleteSessionWithSessionToken_Verify200OK()
        {
            // Setup: Create a session to be deleted.
            ISession createdSession = CreateAndAddSessionToAccessControl();

            // Delete the session.  We should get no errors.
            _accessControl.DeleteSession(createdSession);
        }

        [Test]
        public void DeleteSessionsWithoutSessionToken_Verify400BadRequest()
        {
            // Call the DELETE RestAPI without a session token which should return a 400 error.
            Assert.Throws<Http400BadRequestException>(() => { _accessControl.DeleteSession(null); });
        }

        [Test]
        public void GetActiveLicensesInfo_200OK()
        {
            ///TODO: add expected results
            Assert.DoesNotThrow(() =>
            {
                _accessControl.GetLicensesInfo(LicenseState.active);
            });
        }

        [Test]
        public void GetLockedLicensesInfo_200OK()
        {
            ISession session = CreateAndAddSessionToAccessControl();
            ///TODO: add expected results
            Assert.DoesNotThrow(() =>
            {
                _accessControl.GetLicensesInfo(LicenseState.locked, session: session);
            });
        }

        [Test]
        public void GetLicenseTransactionsInfo_200OK()
        {
            // Setup: Create a session for test.
            ISession session = CreateAndAddSessionToAccessControl();
            int numberOfDays = 1;
            //in our test environment we have only transactions related to consumerType = 1 - regular client
            int consumerType = 1;
            ///TODO: add expected results
            Assert.DoesNotThrow(() =>
            {
                _accessControl.GetLicenseTransactions(numberOfDays, consumerType, session);
            });
        }
    }
}
