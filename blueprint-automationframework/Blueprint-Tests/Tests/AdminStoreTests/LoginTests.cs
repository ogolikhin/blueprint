using System;
using System.Net;

using NUnit.Framework;
using Model;
using System.Collections.Generic;
using System.Threading;
using CustomAttributes;
using Helper.Factories;
using Logging;
using Model.Impl;
using TestConfig;
using Model.Facades;
using Model.Factories;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public static class LoginTests
    {
        private const string _serviceRoute = "svc/adminstore/";
        private const string _sessionRoute = "sessions/";
        private static Dictionary<string, Service> _services = TestConfiguration.GetInstance().Services;
        private static string _sessionUrl = _services["AdminStore"].Address + _serviceRoute + _sessionRoute;
        private static IUser _user = null;

        [SetUp]
        public static void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public static void TearDown()
        {
            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        [Explicit(CustomAttributes.IgnoreReasons.DeploymentNotReady)]
        [Test]
        public static void Login_ValidUser_OK()
        {
            WebResponseFacade _response = Login(_user.Username, _user.Password);
            Assert.AreEqual(HttpStatusCode.OK, _response.StatusCode, "Login failed with {0} code and {1}", _response.StatusCode,
                _response.ResponseString);
        }

        [Test]
        public static void Login_ValidUserBadPassword_OK()
        {
            WebResponseFacade _response = Login(_user.Username, RandomGenerator.RandomAlphaNumeric(8));
            IServiceErrorMessage expectedMessage = ServiceErrorMessageFactory.CreateServiceErrorMessage(2000, "Invalid username or password");
            var recievedMessage = _response.GetBlueprintObject<ServiceErrorMessage>();
            Assert.AreEqual(HttpStatusCode.Unauthorized, _response.StatusCode, "Login failed with {0}", _response.StatusCode);
            Assert.True(expectedMessage.Equals(recievedMessage), "Response message is different from expected");
        }

        /// <summary>
        /// Return WebResponseFacade for login attempt.
        /// </summary>
        /// <param name="user">User for login.</param>
        /// <returns>The WebResponseFacade.</returns>
        private static WebResponseFacade Login(string username, string password)
        {
            //string encodedUsername = HashingUtilities.GenerateSaltedHash(user.Username, string.Empty);
            string encodedUsername = EncodeTo64UTF8(username);
            string encodedPassword = EncodeTo64UTF8(password);
            Dictionary<string, string> headers = new Dictionary<string, string> { { "Content-Type", "Application/json" } };
            return WebRequestFacade.GetWebResponseFacade(_sessionUrl + "?login=" + encodedUsername, "POST", headers, encodedPassword);
        }

        /// <summary>
        /// Encode string to Base64.
        /// </summary>
        /// <param name="m_enc">String to encode.</param>
        // TODO: move to framework
        private static string EncodeTo64UTF8(string m_enc)
        {
            byte[] toEncodeAsBytes =
            System.Text.Encoding.UTF8.GetBytes(m_enc);
            string returnValue =
            System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
    }
}
