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
        private ISession _session = null;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
            _session = _adminStore.AddSession(_user.Username, _user.Password);
        }

        [TearDown]
        public void TearDown()
        {
            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    // AdminStore removes and adds a new session in some cases, so we should expect a 401 error in some cases.
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

        [Test]//currently method returns empty dictionary
        public void GetSettings_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                _adminStore.GetSettings(_session);
            });
        }

        [Test]//currently method is under development
        public void GetConfigJS_OK()///TODO: add check for returned content
        {
            Assert.DoesNotThrow(() =>
            {
                _adminStore.GetConfigJs(_session);
            });
        }
    }
}
