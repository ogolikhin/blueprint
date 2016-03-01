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

        /// <summary>
        /// Runs a common set of asserts on the given file.
        /// </summary>
        /// <param name="file">The file to check.</param>
        private static void AssertLogFile(IFile file)
        {
            Assert.NotNull(file, "ConfigControl returned a null file!");

            const string expectedFilename = "AdminStore.csv";
            const string expectedFileType = "text/csv";

            Assert.That(file.FileName == expectedFilename,
                "ConfigControl.GetLog returned a file named '{0}', but it should be '{1}'!", file.FileName, expectedFilename);

            Assert.That(file.FileType == expectedFileType,
                "ConfigControl.GetLog returned File Type '{0}', but it should be '{1}'!", file.FileType, expectedFileType);

            Assert.That(file.Content.ToString().Length > 0, "ConfigControl.GetLog returned an empty file!");
        }

        [Test, Description("Calls the GetLog method of ConfigControl and sends a valid token header.  Verify log file is returned.")]
        public void GetLog_SendToken_VerifyLogFile()
        {
            IFile file = _configControl.GetLog(_user);

            AssertLogFile(file);
        }

        [Test, Description("Calls the GetLog method of ConfigControl and sends a valid token in a cookie.  Verify log file is returned.")]
        public void GetLog_SendCookie_VerifyLogFile()
        {
            IFile file = _configControl.GetLog(_user, sendAuthorizationAsCookie: true);

            AssertLogFile(file);
        }

        [Test, Description("Calls the GetLog method of ConfigControl with no authentication.  Verify log file is returned.")]
        public void GetLog_NoToken_VerifyLogFile()
        {
            IFile file = _configControl.GetLog();

            AssertLogFile(file);
        }
    }
}
