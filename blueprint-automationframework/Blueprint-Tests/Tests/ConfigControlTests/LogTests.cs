using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;

namespace ConfigControlTests
{
    [TestFixture]
    [Explicit(IgnoreReasons.UnderDevelopment)]
    public class LogTests
    {
        private IAdminStore _adminStore;
        private IConfigControl _configControl;
        private IUser _user;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _configControl = ConfigControlFactory.GetConfigControlFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    _adminStore.DeleteSession(session);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        #endregion Setup and Cleanup

        [Test]
        public void GetLog_SendToken_ExpectSuccess()
        {
            IFile file = _configControl.GetLog(_user);

            Assert.NotNull(file, "ConfigControl returned a null file!");
        }

        [Test]
        public void GetLog_SendCookie_ExpectSuccess()
        {
            IFile file = _configControl.GetLog(_user, sendAuthorizationAsCookie: true);

            Assert.NotNull(file, "ConfigControl returned a null file!");
        }

        [Test]
        public void GetLog_NoToken_ExpectSuccess()
        {
            IFile file = _configControl.GetLog();

            Assert.NotNull(file, "ConfigControl returned a null file!");
        }
    }
}
