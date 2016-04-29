using System.Collections.Generic;
using System.Net;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class LoginTests
    {
        private IAdminStore _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
        private IUser _user = null;
        private IServiceErrorMessage _expectedServiceMessage2000 = ServiceErrorMessageFactory.CreateServiceErrorMessage(2000,
            "Invalid username or password");

        static private IServiceErrorMessage expectedServiceMessage2001(IUser user)
        {
            return ServiceErrorMessageFactory.CreateServiceErrorMessage(2001,
                "User account is locked out for the login: " + user.Username);
        }

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    // AdminStore removes and adds a new session in some cases, so we should expect a 404 error in some cases.
                    List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.Unauthorized };
                    _adminStore.DeleteSession(session, expectedStatusCodes);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        [TestCase]
        public void Login_ValidUser_Verify200OK()
        {
            _adminStore.AddSession(_user.Username, _user.Password);
        }

        [TestCase]
        public void Login_ValidUser_VerifySecondLogin409Error()
        {
            _adminStore.AddSession(_user.Username, _user.Password);
            Assert.Throws<Http409ConflictException>(() =>
           {
               _adminStore.AddSession(_user.Username, _user.Password);
           });
        }

        [TestCase]
        public void Login_ValidUser_VerifyForceLogin200OK()
        {
            _adminStore.AddSession(_user.Username, _user.Password);
            _adminStore.AddSession(_user.Username, _user.Password, force: true);
        }

        [TestCase]
        public void Login_ValidUserBadPassword_Verify401Error()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.AddSession(_user.Username, "bad-password", expectedServiceErrorMessage: _expectedServiceMessage2000);
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
                _adminStore.AddSession(_user.Username, _user.Password, expectedServiceErrorMessage: expectedServiceMessage2001(_user));
            });
        }

        [TestCase]
        public void GetLogedinUser_200OK()
        {
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            IUser loggedinUser = _adminStore.GetLoginUser(session.SessionId);
            Assert.IsTrue(loggedinUser.Equals(_user), "User's details doesn't correspond to expectations");
        }

        [TestCase]
        public void Login_DeletedUser_Verify401Error()
        {
            _user.DeleteUser();
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.AddSession(_user.Username, _user.Password, expectedServiceErrorMessage: _expectedServiceMessage2000);
            });
        }

        [TestCase]
        [TestRail(102892)]
        [Description("Tries to get a session for an SSO user while SAML is disabled.  This test is specifically to get code coverage of a catch block in SessionsController.PostSessionSingleSignOn().")]
        public void Login_SsoWithSamlDisabled_Verify401Error()
        {
            // NOTE: for this test, the SAML request doesn't matter, as long as it's not null or empty.
            string samlRequest = "<samlp:AuthnRequest />";

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.AddSSOSession(_user.Username, samlRequest);
            }, "We should get a 401 Unauthorized error if SAML is not enabled in Instance Administration!");

            // Verify:
            const string expectedMessage = "Federated Authentication mechanism must be enabled";
            Assert.That(ex.RestResponse.Content.Contains(expectedMessage), "Couldn't find '{0}' in the REST response: '{1}'.",
                expectedMessage, ex.RestResponse.Content);
        }

        [TestCase]
        public void Login_5TimesWithBadPassword_VerifyAccountGetsLocked()
        {
            string invalidPassword = "badpassword";
            for (int i = 0; i < 5; i++)
            {
                Assert.Throws<Http401UnauthorizedException>(() =>
                {
                _adminStore.AddSession(_user.Username, invalidPassword, expectedServiceErrorMessage: _expectedServiceMessage2000);
                });
            }
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.AddSession(_user.Username, _user.Password, expectedServiceErrorMessage: expectedServiceMessage2001(_user));
            });
        }

        [TestCase]
        public void Delete_ValidSession_Verify200OK()
        {
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            Assert.DoesNotThrow(() =>
            {
                _adminStore.DeleteSession(session);
            });
        }

        [TestCase]
        public void Delete_ValidDeletedSession_Verify401Error()
        {
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _adminStore.DeleteSession(session);
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.DeleteSession(session);
            });
        }
    }
}
