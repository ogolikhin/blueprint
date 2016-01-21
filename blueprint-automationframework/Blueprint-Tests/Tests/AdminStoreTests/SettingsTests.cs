using System.Net;
using NUnit.Framework;
using CustomAttributes;
using Model;
using Model.Factories;
using System.Collections.Generic;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class SettingsTests
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
                    List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.Unauthorized };
                    _adminStore.DeleteSession(session, expectedStatusCodes);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: false);
                _user = null;
            }
        }

        [Test]
        public void GetSettings_OK()
        {
            List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            _adminStore.GetSettings(_adminStore.AddSession(_user.Username, _user.Password), expectedStatusCodes: expectedStatusCodes);
        }

        [Test]
        public void GetConfigJS_OK()///TODO: add check for returned content
        {
            List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            _adminStore.GetConfigJs(_adminStore.AddSession(_user.Username, _user.Password), expectedStatusCodes: expectedStatusCodes);
        }
    }
}
