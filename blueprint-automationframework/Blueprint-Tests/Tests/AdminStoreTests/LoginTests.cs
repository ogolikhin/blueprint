using System.Collections.Generic;
using System.Net;
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
    public class LoginTests : TestBase
    {
        private IUser _user = null;
        private IServiceErrorMessage _expectedServiceMessage2000 = ServiceErrorMessageFactory.CreateServiceErrorMessage(2000,
            "Invalid username or password");

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
        public void Login_ValidUser_Verify200OK()
        {
            Helper.AdminStore.AddSession(_user.Username, _user.Password);
        }

        [TestCase]
        public void Login_ValidUser_VerifySecondLogin409Error()
        {
            Helper.AdminStore.AddSession(_user.Username, _user.Password);
            Assert.Throws<Http409ConflictException>(() =>
           {
               Helper.AdminStore.AddSession(_user.Username, _user.Password);
           });
        }

        [TestCase]
        public void Login_ValidUser_VerifyForceLogin200OK()
        {
            Helper.AdminStore.AddSession(_user.Username, _user.Password);
            Helper.AdminStore.AddSession(_user.Username, _user.Password, force: true);
        }

        [TestCase]
        public void Login_ValidUserBadPassword_Verify401Error()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, "bad-password", expectedServiceErrorMessage: _expectedServiceMessage2000);
            });
        }

        [TestCase]
        public void Login_LockedUser_Verify401Error()
        {
            _user.DeleteUser(deleteFromDatabase: true);
            _user = UserFactory.CreateUserOnly();
            _user.Enabled = false;
            _user.CreateUser();
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password, expectedServiceErrorMessage: expectedServiceMessage2001(_user));
            });
        }

        [TestCase]
        public void GetLogedinUser_200OK()
        {
            ISession session = Helper.AdminStore.AddSession(_user.Username, _user.Password);
            IUser loggedinUser = Helper.AdminStore.GetLoginUser(session.SessionId);
            Assert.IsTrue(loggedinUser.Equals(_user), "User's details doesn't correspond to expectations");
        }

        [TestCase]
        public void Login_DeletedUser_Verify401Error()
        {
            _user.DeleteUser();
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password, expectedServiceErrorMessage: _expectedServiceMessage2000);
            });
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
        public void Login_5TimesWithBadPassword_VerifyAccountGetsLocked()
        {
            string invalidPassword = "badpassword";

            for (int i = 0; i < 5; i++)
            {
                Assert.Throws<Http401UnauthorizedException>(() =>
                {
                    Helper.AdminStore.AddSession(_user.Username, invalidPassword, expectedServiceErrorMessage: _expectedServiceMessage2000);
                }, "We expected to get a 401 Unauthorized when logging in with a bad password!");
            }

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password, expectedServiceErrorMessage: expectedServiceMessage2001(_user));
            }, "We expected to get a 401 Unauthorized when logging in with a good password after the user account is locked!");
        }

        [TestCase]
        public void Delete_ValidSession_Verify200OK()
        {
            ISession session = Helper.AdminStore.AddSession(_user.Username, _user.Password);
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.DeleteSession(session);
            });
        }

        [TestCase]
        public void Delete_ValidDeletedSession_Verify401Error()
        {
            ISession session = Helper.AdminStore.AddSession(_user.Username, _user.Password);
            Helper.AdminStore.DeleteSession(session);
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.DeleteSession(session);
            });
        }
    }
}
