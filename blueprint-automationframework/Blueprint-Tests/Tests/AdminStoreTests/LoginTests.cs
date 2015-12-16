using System.Collections.Generic;
using System.Net;
using CustomAttributes;
using Helper.Factories;
using Model;
using Model.Facades;
using NUnit.Framework;
using TestConfig;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class LoginTests
    {
        private const string _serviceRoute = "svc/adminstore/";
        private const string _sessionRoute = "sessions/";
        private static Dictionary<string, Service> _services = TestConfiguration.GetInstance().Services;
        private static string _sessionUrl = _services["AdminStore"].Address + _serviceRoute + _sessionRoute;
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

        [Explicit(CustomAttributes.IgnoreReasons.DeploymentNotReady)]
        [Test]
        public void Login_ValidUser_OK()
        {
            WebResponseFacade _response = Login(_user);
            Assert.AreEqual(HttpStatusCode.OK, _response.StatusCode, "Login failed with {0} code and {1}", _response.StatusCode,
                _response.ResponseString);
        }

        /// <summary>
        /// Return WebResponseFacade for login attempt.
        /// </summary>
        /// <param name="user">User for login.</param>
        /// <returns>The WebResponseFacade.</returns>
        private static WebResponseFacade Login(IUser user)
        {
            //string encodedUsername = HashingUtilities.GenerateSaltedHash(user.Username, string.Empty);
            string encodedUsername = EncodeTo64UTF8(user.Username);
            string encodedPassword = EncodeTo64UTF8(user.Password);
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
