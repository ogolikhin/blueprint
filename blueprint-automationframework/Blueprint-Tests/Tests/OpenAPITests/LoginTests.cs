using System;
using System.Net;

using NUnit.Framework;
using Model;
using System.Collections.Generic;
using System.Threading;
using CustomAttributes;
using Common;
using Model.Factories;
using TestConfig;
using Utilities;
using Utilities.Facades;


namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class LoginTests
    {
        private static TestConfiguration _testConfig = TestConfiguration.GetInstance();

        private IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        #region Private Functions

        /// <summary>
        /// Creates a new thread that logs in with the specified user, with the specified number of retries.
        /// </summary>
        /// <param name="user">The user to login with.</param>
        /// <param name="maxRetries">The maximum number of retries for connection timeouts.</param>
        /// <param name="exceptions">If this thread fails, the exception will be added to this list.</param>
        /// <param name="threads">A list of threads that will be killed if any thread failed.</param>
        /// <returns>The new thread.</returns>
        private Thread CreateThreadToLoginWithValidCredentials(IUser user, uint maxRetries, List<Exception> exceptions, List<Thread> threads)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    LoginWithValidCredentials(user, maxRetries);
                }
                catch (ThreadAbortException)
                {
                    Logger.WriteTrace("Thread [{0}] was aborted.", Thread.CurrentThread.ManagedThreadId);
                }
                catch (Exception e)
                {
                    lock (this)
                    {
                        exceptions.Add(e);

                        if (threads.Count > 0)
                        {
                            Logger.WriteError("*** Thread caught exception:  {0}", e.Message);

                            // If one thread fails, kill all other threads immediately (fail fast).
                            Logger.WriteInfo("One thread failed.  Killing all other threads...");
                            threads.ForEach(t => KillThreadIfNotCurrentThread(t));
                            threads.Clear();
                            Logger.WriteDebug("Finished killing all threads.");
                        }
                    }

                    throw;
                }
            });

            return thread;
        }

        /// <summary>
        /// Kills the specified thread, unless the specified thread is this thread.
        /// </summary>
        /// <param name="thread">The thread to kill.</param>
        private static void KillThreadIfNotCurrentThread(Thread thread)
        {
            if (Thread.CurrentThread != thread)
            {
                Logger.WriteTrace("Killing thread [{0}]...", Thread.CurrentThread.ManagedThreadId);
                thread.Abort();
            }
        }

        /// <summary>
        /// Tries to login using invalid credentials (i.e. bad password).
        /// </summary>
        /// <param name="username">The username to attempt to login with.</param>
        /// <exception cref="NUnit.Framework.AssertionException">If the login doesn't fail with a 401 Unauthorized exception.</exception>
        private void LoginWithInvalidCredentials(string username)
        {
            const string badPassword = "bad-password";
            string noToken = string.Empty;
            IUser invalidUser = UserFactory.CreateUserOnly(username, badPassword);

            Assert.Throws<Http401UnauthorizedException>(
                () => { _server.LoginUsingBasicAuthorization(invalidUser, noToken); },
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
            string noToken = string.Empty;

            // Login.
            Assert.DoesNotThrow(() => { response = _server.LoginUsingBasicAuthorization(user, noToken, maxRetries); });

            // Verify login was successful.
            Assert.IsNotNull(response, "Login for user {0} returned a null RestResponse!", user.Username);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Login for user '{0}' should get '200 OK', but got '{1}' instead!",
                user.Username, response.StatusCode);
        }

        #endregion Private Functions

        [Test]
        public void LoginWithInvalidPassword_401Error()
        {
            LoginWithInvalidCredentials(_user.Username);
        }

        [Test]
        public void LoginWithInvalidUser_401Error()
        {
            LoginWithInvalidCredentials("wrong-user");
        }

        [Test]
        public void LoginWithValidCredentials_OK()
        {
            LoginWithValidCredentials(_user);
        }

        [Test]
        public void Verify_InvalidLogonAttemptsNumber_IsResetOnSuccessfulLogin()
        {
            RestResponse response = null;

            // Creating the user with invalid password.
            IUser invalidUser = UserFactory.CreateUserOnly(_user.Username, "bad-password");
            
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
        [Category(Categories.ConcurrentTest)]
        public void LoginValidUsersConcurrently_OK(int numUsers, uint maxRetries)
        {
            List<IUser> users = new List<IUser>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();

            try
            {
                // Create the users & threads.
                for (int i = 0; i < numUsers; ++i)
                {
                    IUser user = UserFactory.CreateUserAndAddToDatabase();
                    users.Add(user);
                    threads.Add(CreateThreadToLoginWithValidCredentials(user, maxRetries, exceptions, threads));
                }

                // Now run the threads.
                threads.ForEach(t => t.Start());

                // Wait for threads to finish.
                threads.ForEach(t => t.Join());

                Assert.IsEmpty(exceptions, "At least {0} threads failed to login!", exceptions.Count);
            }
            finally
            {
                // Cleanup: delete all the users we created.
                users.ForEach(u => u.DeleteUser(deleteFromDatabase: true));
            }
        }
    }
}
