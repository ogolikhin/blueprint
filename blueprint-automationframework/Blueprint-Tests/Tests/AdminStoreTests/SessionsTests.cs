using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class SessionsTests : TestBase
    {
        private IUser _user = null;
        private IServiceErrorMessage _expectedServiceMessage2000 = ServiceErrorMessageFactory.CreateServiceErrorMessage(2000,
            "Invalid username or password");

        // TODO: Replace with calls to TestHelper.ValidateServiceError().
        private static IServiceErrorMessage expectedServiceMessage2001(IUser user)
        {
            return ServiceErrorMessageFactory.CreateServiceErrorMessage(2001,
                "User account is locked out for the login: " + user.Username);
        }

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [Description("Run:  POST /sessions?login={encrypted username}  with a valid username/password.  Verify it returns 200 OK and the token is returned.")]
        [TestRail(146180)]
        public void Login_ValidUser_ReturnsValidToken()
        {
            ISession session = null;

            Assert.DoesNotThrow(() =>
            {
                session = Helper.AdminStore.AddSession(_user.Username, _user.Password);
            }, "AddSession() should return 200 OK for a valid username/password!");

            Assert.NotNull(session, "AddSession() returned a null session object with a valid username/password!");
            Assert.IsNotNullOrEmpty(session.SessionId, "The returned Session token should not be null or empty!");
        }

        [TestCase]
        [Description("Run:  POST /sessions?login={encrypted username}  twice with the same valid username/password.  Verify 2nd login gets a 409 error.")]
        [TestRail(146181)]
        public void Login_ValidUser_VerifySecondLogin409Error()
        {
            Helper.AdminStore.AddSession(_user.Username, _user.Password);

            Assert.Throws<Http409ConflictException>(() =>
           {
               Helper.AdminStore.AddSession(_user.Username, _user.Password);
           }, "Second login attempt should return a 409 Conflict error!");
        }

        [TestCase]
        [Description("Run:  POST /sessions?login={encrypted username}&force=true  with the same valid username/password of a user who already has a session token.  Verify 2nd login gets 200 OK.")]
        [TestRail(146182)]
        public void Login_Force2ndLogin_200OK()
        {
            Helper.AdminStore.AddSession(_user.Username, _user.Password);

            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password, force: true);
            }, "AddSession() should succeed if run a second time with the 'force=true' parameter!");
        }

        [TestCase("bad-password")]
        [Description("Run:  POST /sessions?login={encrypted username}  with a wrong password.  Verify it returns a 401 Unauthorized error.")]
        [TestRail(146183)]
        public void Login_ValidUserBadPassword_Verify401Error(string password)
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, password, expectedServiceErrorMessage: _expectedServiceMessage2000);
            }, "AddSession() should return a 401 Unauthorized error for a valid user but wrong password!");
        }

        [TestCase("")]
        [TestCase(null)]
        [Description("Run:  POST /sessions?login={encrypted username}  with a blank or missing password.  Verify it returns a 401 Unauthorized error.")]
        [TestRail(146292)]
        public void Login_ValidUserNullOrEmptyPassword_Verify401Error(string password)
        {
            var expectedServiceErrorMessage = ServiceErrorMessageFactory.CreateServiceErrorMessage(2003,
                "Username and password cannot be empty");

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, password, expectedServiceErrorMessage: expectedServiceErrorMessage);
            }, "AddSession() should return a 401 Unauthorized error for a valid user but blank or missing password!");
        }

        [TestCase]
        [Description("Run:  POST /sessions?login={encrypted username}  with a non-existing user.  Verify it returns a 401 Unauthorized error.")]
        [TestRail(146288)]
        public void Login_NonExistingUser_Verify401Error()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession("bad-username", "bad-password", expectedServiceErrorMessage: _expectedServiceMessage2000);
            }, "AddSession() should return a 401 Unauthorized error for a non-existing user!");
        }

        [TestCase]
        [Description("Run:  POST /sessions?login={encrypted username}  with locked user.  Verify it returns a 401 Unauthorized error.")]
        [TestRail(146184)]
        public void Login_LockedUser_Verify401Error()
        {
            // Setup: Create a locked user.
            var user = UserFactory.CreateUserOnly();
            user.Enabled = false;
            user.CreateUser();

            // Execute & verify 401 error.
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(user.Username, user.Password,
                    expectedServiceErrorMessage: expectedServiceMessage2001(user));
            }, "AddSession() should return a 401 Unauthorized error if the user is locked!");
        }

        [TestCase]
        [Description("Run:  POST /sessions?login={encrypted username}  with a deleted user.  Verify it returns a 401 Unauthorized error.")]
        [TestRail(146186)]
        public void Login_DeletedUser_Verify401Error()
        {
            _user.DeleteUser();

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password, expectedServiceErrorMessage: _expectedServiceMessage2000);
            }, "AddSession() should return 401 Unauthorized for a deleted user!");
        }

        [TestCase]
        [TestRail(102892)]
        [Description("Tries to get a session for an SSO user while SAML is disabled.  " +
            "This test is specifically to get code coverage of a catch block in SessionsController.PostSessionSingleSignOn().")]
        public void Login_SsoWithSamlDisabled_Verify401Error()
        {
            // NOTE: for this test, the SAML request doesn't matter, as long as it's not null or empty.
            string samlRequest = "<samlp:AuthnRequest />";

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSsoSession(_user.Username, samlRequest);
            }, "We should get a 401 Unauthorized error if SAML is not enabled in Instance Administration!");

            // Verify:
            const string expectedMessage = "Federated Authentication mechanism must be enabled";
            Assert.That(ex.RestResponse.Content.Contains(expectedMessage), "Couldn't find '{0}' in the REST response: '{1}'.",
                expectedMessage, ex.RestResponse.Content);
        }

        [TestCase]
        [TestRail(104415)]
        [Description("Gets the InvalidLogonAttemptsNumber up to 4, then login with a valid password and verify that the InvalidLogonAttemptsNumber is reset.")]
        public void Login_4TimesWithBadPassword_VerifyInvalidLogonAttemptsNumberIsResetOnSuccessfulLogin()
        {
            // Setup:
            string invalidPassword = "badpassword";

            // Login with a bad password 4 times to get us to the limit before the account is locked.
            for (int i = 0; i < 4; i++)
            {
                Assert.Throws<Http401UnauthorizedException>(() =>
                {
                    Helper.AdminStore.AddSession(_user.Username, invalidPassword, expectedServiceErrorMessage: _expectedServiceMessage2000);
                }, "We expected to get a 401 Unauthorized when logging in with a bad password!");
            }

            // Execute: Login with a valid password to reset the InvalidLogonAttemptsNumber.
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.AddSession(_user, expectedServiceErrorMessage: expectedServiceMessage2001(_user));
            }, "Login should succeed after 4 bad logins.");

            // Delete the token, otherwise we get a 409 error on the successful login at the end.
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.DeleteSession(_user);
            }, "Failed to delete session token for user '{0}'!", _user.Username);

            // Login one more time with a bad password which would lock the account if the successful login above didn't reset the InvalidLogonAttemptsNumber.
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, invalidPassword, expectedServiceErrorMessage: _expectedServiceMessage2000);
            }, "We expected to get a 401 Unauthorized when logging in with a bad password!");

            // Verify: Finally, login again with a valid password which should work if InvalidLogonAttemptsNumber was reset properly.
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.AddSession(_user, expectedServiceErrorMessage: expectedServiceMessage2001(_user));
            }, "Login should succeed.  It looks like the InvalidLogonAttemptsNumber didn't get reset properly!");
        }

        [TestCase]
        [TestRail(146187)]
        [Description("Login with a bad password 5 times and verify the account gets locked.")]
        public void Login_5TimesWithBadPassword_VerifyAccountGetsLocked()
        {
            string invalidPassword = "badpassword";

            // Execute:
            for (int i = 0; i < 5; i++)
            {
                Assert.Throws<Http401UnauthorizedException>(() =>
                {
                    Helper.AdminStore.AddSession(_user.Username, invalidPassword, expectedServiceErrorMessage: _expectedServiceMessage2000);
                }, "We expected to get a 401 Unauthorized when logging in with a bad password!");
            }

            // Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password, expectedServiceErrorMessage: expectedServiceMessage2001(_user));
            }, "We expected to get a 401 Unauthorized when logging in with a good password after the user account is locked!");
        }

        [TestCase]
        [TestRail(146188)]
        [Description("Delete a valid session.  Verify the session is deleted.")]
        public void Delete_ValidSession_VerifySessionIsDeleted()
        {
            // Setup:
            var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.DeleteSession(session);
            }, "DeleteSession() should return 200 OK when deleting a valid session token!");

            // Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetLoginUser(session.SessionId);
            }, "The session token is still valid after it was deleted!");
        }

        [TestCase]
        [TestRail(146189)]
        [Description("Delete a valid session twice.  Verify a 401 Unauthorized error is returned.")]
        public void Delete_ValidSessionTwice_Verify401Error()
        {
            var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);
            Helper.AdminStore.DeleteSession(session);

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.DeleteSession(session);
            }, "The second call to DeleteSession() should return a 401 Unauthorized error!");
        }

        [TestCase]
        [TestRail(146291)]
        [Description("Run:  DELETE /sessions  but don't pass any Session-Token header.  Verify a 401 Unauthorized error is returned.")]
        public void Delete_MissingTokenHeader_401Unauthorized()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.DeleteSession(session: null);
            }, "DeleteSession() should return 401 Unauthorized if no Session-Token header was passed!");
        }

        [TestCase]
        [TestRail(234354)]
        [Description("Add a session. Check if session is valid. Verify 200 OK is returned.")]
        public void Check_ValidSession_Verify200OK()
        {
            // Setup:
            var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.CheckSession(session);
            }, "The call to CheckSession() should succeed!");
        }

        [TestCase]
        [TestRail(234355)]
        [Description("Add a session. Delete session and check if session is valid. " +
                     "Verify 401 Unauthorized is returned.")]
        public void Check_SessionDeleted_Verify401Unauthorized()
        {
            // Setup:
            var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

            Helper.AdminStore.DeleteSession(session);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.CheckSession(session);
            }, "The call to CheckSession() should return 401 Unauthorized!");
        }

        [TestCase]
        [TestRail(234357)]
        [Description("Add a session. Check session with invalid token. Verify 401 Unauthorized is returned.")]
        public void Check_InvalidSessionToken_Verify401Unauthorized()
        {
            // Setup, Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.CheckSession(CommonConstants.InvalidToken);
            }, "The call to CheckSession() should return 401 Unauthorized!");
        }

        [TestCase]
        [TestRail(234358)]
        [Description("Add a session. Check session with missing token. Verify 401 Unauthorized is returned.")]
        public void Check_MissingSessionToken_Verify401Unauthorized()
        {
            // Setup:
            Helper.AdminStore.AddSession(_user.Username, _user.Password);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.CheckSession(token: null);
            }, "The call to CheckSession() should return 401 Unauthorized!");
        }
    }
}
