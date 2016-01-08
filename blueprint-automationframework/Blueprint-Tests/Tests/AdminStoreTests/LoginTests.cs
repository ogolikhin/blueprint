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
                    List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.NotFound };
                    _adminStore.DeleteSession(session, expectedStatusCodes);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        [Test]
        public void Login_ValidUser_Verify200OK()
        {
            _adminStore.AddSession(_user.Username, _user.Password);
        }

        [Test]
        public void Login_ValidUser_VerifySecondLogin409Error()
        {
            _adminStore.AddSession(_user.Username, _user.Password);
            Assert.Throws<Http409ConflictException>(() =>
           {
               _adminStore.AddSession(_user.Username, _user.Password);
           });
        }

        [Test]
        public void Login_ValidUser_VerifyForceLogin200OK()
        {
            _adminStore.AddSession(_user.Username, _user.Password);
            _adminStore.AddSession(_user.Username, _user.Password, force: true);
        }

        [Test]
        public void Login_ValidUserBadPassword_Verify401Error()
        {
            IServiceErrorMessage expectedServiceErrorMessage = ServiceErrorMessageFactory.CreateServiceErrorMessage(2000, "Invalid username or password");

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.AddSession(_user.Username, "bad-password", expectedServiceErrorMessage: expectedServiceErrorMessage);
            });
        }

        [Test]
        public void Login_LockedUser_Verify401Error()
        {
            _user.DeleteUser(deleteFromDatabase: true);
            _user = UserFactory.CreateUserOnly();
            _user.Enabled = false;
            _user.CreateUser();
            IServiceErrorMessage expectedServiceErrorMessage = ServiceErrorMessageFactory.CreateServiceErrorMessage(2001, 
                "User account is locked out for the login: " + _user.Username);
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.AddSession(_user.Username, _user.Password, expectedServiceErrorMessage: expectedServiceErrorMessage);
            });
        }
    }
}
