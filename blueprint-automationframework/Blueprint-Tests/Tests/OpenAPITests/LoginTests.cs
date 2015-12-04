using System;
using System.Net;

using NUnit.Framework;
using Model;
using System.Collections.Generic;
using System.Threading;
using Helper.Factories;
using Logging;
using Model.Impl;
using TestConfig;


namespace OpenAPITests
{
    [TestFixture]
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

        /// <summary>
        /// Tries to login using invalid credentials (i.e. bad password).
        /// </summary>
        /// <param name="username">The username to attempt to login with.</param>
        /// <exception cref="NUnit.Framework.AssertionException">If the login doesn't fail with a 401 Unauthorized exception.</exception>
        private void LoginWithInvalidCredentials(string username)
        {
            IUser invalidUser = UserFactory.CreateUserOnly(username, "bad-password");
            HttpWebResponse response = null;

            try
            {
                WebException ex = Assert.Throws<Http401UnauthorizedException>(
                    () => { response = _server.LoginUsingBasicAuthorization(invalidUser); },
                    string.Format("We were expecting an exception when logging into '{0}' with user '{1}' and password '{2}'.",
                        _testConfig.BlueprintServerAddress, _testConfig.Username, _testConfig.Password));

                // Make sure that the exception also has the right error message.
                const string expectedMsg = "(401) Unauthorized";
                Assert.That(ex.Message.Contains(expectedMsg), "The exception should contain '{0}', but instead it has: '{1}'!", expectedMsg, ex.Message);
            }
            finally
            {
                if (response != null) { response.Dispose(); }
            }
        }

        /// <summary>
        /// Tries to login with the specified user and expects login to succeed.
        /// </summary>
        /// <param name="user">The user to login with.</param>
        /// <param name="maxRetries">(optional) The number of times to retry the login in cases such as Socket Timeouts...</param>
        /// <exception cref="NUnit.Framework.AssertionException">If the login fails and all retries are exhausted.</exception>
        private void LoginWithValidCredentials(IUser user, uint maxRetries = 1)
        {
            HttpWebResponse response = null;

            try
            {
                Logger.WriteDebug("Before logging in user {0}.", user.Username);
                Assert.DoesNotThrow(() => { response = _server.LoginUsingBasicAuthorization(user, maxRetries); });
                Logger.WriteDebug("After logging in user {0}.", user.Username);

                Assert.IsNotNull(response, "Login failed for user {0} and returned a null HttpWebResponse!", user.Username);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "We expected '200 OK', but got '{0}' instead!",
                    response.StatusCode);
            }
            finally
            {
                if (response != null) { response.Dispose(); }
            }
        }

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
        [Explicit("Ignore since current build has this defect")]
        public void Verify_InvalidLogonAttemptsNumber_IsResetOnSuccessfulLogin()
        {
            HttpWebResponse response = null;

            // Creating the user with invalid password.
            IUser invalidUser = UserFactory.CreateUserOnly(_user.Username, "bad-password");

            // Invalid login attempt 4 times.
            for (int i = 0; i < 4; i++)
            {
                Assert.Throws<Http401UnauthorizedException>(() => { response = _server.LoginUsingBasicAuthorization(invalidUser); },
                    string.Format("We were expecting an exception when logging into '{0}' with user '{1}' and password '{2}'. <Iteration: {3}>",
                        _testConfig.BlueprintServerAddress, _testConfig.Username, _testConfig.Password, i+1));
            }

            // Valid login should reset InvalidLogonAttemptsNumber value to 0.
            Assert.DoesNotThrow(() => { response = _server.LoginUsingBasicAuthorization(_user); });
            using (response)
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "We expected '200 OK', but got '{0}' instead!",
                    response.StatusCode);
            }

            // Invalid login to see if it gets locked.
            Assert.Throws<Http401UnauthorizedException>(() => { response = _server.LoginUsingBasicAuthorization(invalidUser); },
                string.Format("We were expecting an exception when logging into '{0}' with user '{1}' and password '{2}'.",
                    _testConfig.BlueprintServerAddress, _testConfig.Username, _testConfig.Password));

            // Valid login should reset InvalidLogonAttemptsNumber.
            Assert.DoesNotThrow(() => { response = _server.LoginUsingBasicAuthorization(_user); });
            using (response)
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "We expected '200 OK', but got '{0}' instead!",
                    response.StatusCode);
            }
        }

        [Test]
        public static void GetUserInfoFromDatabase_VerifyUserExists()
        {
            List<IUser> users = UserFactory.GetUsers();

            foreach (var user in users)
            {
                Logger.WriteDebug(user.ToString());
            }

            // We assume that every Blueprint installation has an 'admin' user by default.
            Assert.That(users.Exists(x => x.Username == "admin"), "Couldn't find 'admin' user in database!");
        }

        [Test]
        public void CreateUserInDatabase_VerifyUserExists()
        {
            List<IUser> users = UserFactory.GetUsers();

            Assert.That(users.Exists(x => x.Username == _user.Username), "Couldn't find user '{0}' in database after adding the user to the database!", _user.Username);
        }

        [Test]
        public void DeleteUser_VerifyUserIsDeleted()
        {
            List<IUser> users = UserFactory.GetUsers();
            string username = _user.Username;

            Assert.That(users.Exists(x => x.Username == username), "Couldn't find user '{0}' in database after adding the user to the database!", username);

            // Now delete the user.
            _user.DeleteUser();

            // Verify that the user was deleted.
            users = UserFactory.GetUsers();

            Assert.IsFalse(users.Exists(x => x.Username == username), "We found user '{0}' in database after deleting the user!", username);
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
                catch (ThreadAbortException e)
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

        [TestCase(10, (uint)1)]
        [TestCase(100, (uint)2)]
        [TestCase(1000, (uint)5, Explicit = true, Reason = "Test takes too long and kills the server.")]
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
