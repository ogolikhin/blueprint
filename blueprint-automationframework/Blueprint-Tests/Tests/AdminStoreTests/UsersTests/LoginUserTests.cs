using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class LoginUserTests : TestBase
    {
        private const string REST_PATH = RestPaths.Svc.AdminStore.Users.LOGINUSER;

        private IUser _adminUser = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        // TODO: Update this to test different LicenseTypes & AdminRoleId & privileges.
        [TestCase]
        [Description("Run:  GET /users/loginuser   with valid token.  Verify it returns the user who owns the specified session.")]
        [TestRail(146185)]
        public void GetLogedinUser_ValidSession_ReturnsCorrectUser()
        {
            var session = Helper.AdminStore.AddSession(_adminUser.Username, _adminUser.Password);
            IUser loggedinUser = null;

            Assert.DoesNotThrow(() =>
            {
                loggedinUser = Helper.AdminStore.GetLoginUser(session.SessionId);
            }, "'GET {0}' should return 200 OK for a valid session token!", REST_PATH);

            Assert.IsTrue(loggedinUser.Equals(_adminUser), "The user info returned by GetLoginUser() doesn't match the user who owns the token!");
        }

        [TestCase]
        [Description("Run:  GET /users/loginuser   with an invalid token.  Verify it returns 401 Unauthorized.")]
        [TestRail(146289)]
        public void GetLogedinUser_InvalidSession_401Unauthorized()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetLoginUser(CommonConstants.InvalidToken);
            }, "'GET {0}' should return 401 Unauthorized for an invalid session token!", REST_PATH);
        }

        [TestCase]
        [Description("Run:  GET /users/loginuser   but don't pass any Session-Token header.  Verify it returns 401 Unauthorized.")]
        [TestRail(146290)]
        public void GetLogedinUser_MissingTokenHeader_401Unauthorized()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetLoginUser(null);
            }, "'GET {0}' should return 401 Unauthorized if the Session-Token header is missing!", REST_PATH);
        }
    }
}
