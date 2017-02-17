using System.Net;

using NUnit.Framework;
using Model;
using System.Collections.Generic;
using CustomAttributes;
using Common;
using Helper;
using Model.Factories;
using TestCommon;
using TestConfig;
using Utilities;
using Utilities.Facades;


namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class LoginTests : TestBase
    {
        private static TestConfiguration _testConfig = TestConfiguration.GetInstance();

        private IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        private IUser _user = null;

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

        #region Private Functions

        /// <summary>
        /// Tries to login using invalid credentials (i.e. bad password).
        /// </summary>
        /// <param name="username">The username to attempt to login with.</param>
        /// <exception cref="NUnit.Framework.AssertionException">If the login doesn't fail with a 401 Unauthorized exception.</exception>
        private void LoginWithInvalidCredentials(string username)
        {
            const string badPassword = "bad-password";
            var invalidUser = UserFactory.CreateUserOnly(username, badPassword);

            Assert.Throws<Http401UnauthorizedException>(
                () => { _server.LoginUsingBasicAuthorization(invalidUser); },
                I18NHelper.FormatInvariant("We were expecting an exception when logging into '{0}' with user '{1}' and password '{2}'.",
                    _server.Address, username, badPassword));
        }

        /// <summary>
        /// Tries to login with the specified user and expects login to succeed.
        /// </summary>
        /// <param name="user">The user to login with.</param>
        /// <param name="maxRetries">(optional) The number of times to retry the login in cases such as Socket Timeouts...</param>
        /// <exception cref="NUnit.Framework.AssertionException">If the login fails and all retries are exhausted.</exception>
        private void LoginWithValidCredentials(IUser user, uint maxRetries = 1)
        {
            RestResponse response = null;

            // Login.
            Assert.DoesNotThrow(() => { response = _server.LoginUsingBasicAuthorization(user, maxRetries: maxRetries); });

            // Verify login was successful.
            Assert.IsNotNull(response, "Login for user {0} returned a null RestResponse!", user.Username);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Login for user '{0}' should get '200 OK', but got '{1}' instead!",
                user.Username, response.StatusCode);
        }

        #endregion Private Functions

        [TestCase]
        [TestRail(227247)]
        [Description("Login with a valid user but invalid password.  Verify 401 Unauthorized is returned.")]
        public void LoginWithInvalidPassword_401Error()
        {
            LoginWithInvalidCredentials(_user.Username);
        }

        [TestCase]
        [TestRail(227248)]
        [Description("Login with an invalid user & password.  Verify 401 Unauthorized is returned.")]
        public void LoginWithInvalidUser_401Error()
        {
            LoginWithInvalidCredentials("wrong-user");
        }

        [TestCase]
        [TestRail(227243)]
        [Description("Login with valid credentials.  Verify 200 OK is returned.")]
        public void LoginWithValidCredentials_OK()
        {
            LoginWithValidCredentials(_user);
        }

        [TestCase]
        [TestRail(227245)]
        [Description("Login with a bad password 4 times, then login with a valid password, then login again with a bad password.  Verify that if you" +
                     "login again with a valid password that it returns 200 OK.")]
        public void Verify_InvalidLogonAttemptsNumber_IsResetOnSuccessfulLogin()
        {
            RestResponse response = null;

            // Creating the user with invalid password.
            var invalidUser = UserFactory.CreateUserOnly(_user.Username, "bad-password");
            
            // Invalid login attempt 4 times.
            for (int i = 0; i < 4; i++)
            {
                Assert.Throws<Http401UnauthorizedException>(() => { response = _server.LoginUsingBasicAuthorization(invalidUser); },
                    I18NHelper.FormatInvariant("We were expecting an exception when logging into '{0}' with user '{1}' and password '{2}'. <Iteration: {3}>",
                        _testConfig.BlueprintServerAddress, _testConfig.Username, _testConfig.Password, i+1));
            }
            
            // Valid login should reset InvalidLogonAttemptsNumber value to 0.
            Assert.DoesNotThrow(() => { LoginWithValidCredentials(_user); }, "Login with valid credentials should succeed!");

            // Invalid login to see if it gets locked.
            Assert.Throws<Http401UnauthorizedException>(() => { response = _server.LoginUsingBasicAuthorization(invalidUser); },
                I18NHelper.FormatInvariant("We were expecting an exception when logging into '{0}' with user '{1}' and password '{2}'.",
                    _testConfig.BlueprintServerAddress, _testConfig.Username, _testConfig.Password));

            // Valid login should reset InvalidLogonAttemptsNumber.
            Assert.DoesNotThrow(() => { LoginWithValidCredentials(_user); }, "Login with valid credentials should succeed!");
        }

        [TestCase(10, (uint)1)]
        [TestCase(100, (uint)2)]
        [TestCase(1000, (uint)5, Explicit = true, Reason = IgnoreReasons.OverloadsTheSystem)]
        [TestRail(227246)]
        [Category(Categories.ConcurrentTest)]
        [Description("Login with multiple users concurrently and verify that they all return 200 OK.")]
        public void LoginValidUsersConcurrently_OK(int numThreads, uint maxRetries)
        {
            // Setup:
            List<IUser> users = new List<IUser>();
            var threadHelper = new ConcurrentTestHelper(Helper);

            try
            {
                // Create the users & threads.
                for (int i = 0; i < numThreads; ++i)
                {
                    var user = UserFactory.CreateUserAndAddToDatabase();  // Don't use Helper here because the users are deleted in the finally block.
                    users.Add(user);

                    threadHelper.AddTestFunctionToThread(() =>
                    {
                        LoginWithValidCredentials(user, maxRetries);
                    });
                }

                // Execute & Verify:
                threadHelper.RunThreadsAndWaitToCompletion();
            }
            finally
            {
                // Cleanup: delete all the users we created.
                users.ForEach(u => u.DeleteUser());
            }
        }
    }
}
