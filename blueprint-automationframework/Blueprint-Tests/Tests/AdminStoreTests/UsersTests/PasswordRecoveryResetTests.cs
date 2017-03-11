using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities.Facades;

namespace AdminStoreTests.UsersTests
{
    [Explicit(IgnoreReasons.UnderDevelopmentDev)]   // US5414
    [Category(Categories.AdminStore)]
    [TestFixture]
    public class PasswordRecoveryResetTests : TestBase
    {
        private const string REST_PATH = RestPaths.Svc.AdminStore.Users.PasswordRecovery.RESET;

        private IUser _adminUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Positive tests

        [TestCase(TestHelper.ProjectRole.AuthorFullAccess)]
        [TestCase(TestHelper.ProjectRole.None)]
        [Description("Create a user and then request a password reset for that user; then reset the user's password with the recovery token.  " +
                     "Verify 200 OK is returned and the user's password was reset.")]
        [TestRail(266995)]
        public void PasswordRecoveryReset_ValidTokenAndPassword_PasswordIsReset(TestHelper.ProjectRole role)
        {
            // Setup:
            var user = Helper.CreateUserWithProjectRolePermissions(role, _project);

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);

            RestResponse response = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                response = Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, user.Password);
            }, "'POST {0}' should return 200 OK when passed a valid token and password.", REST_PATH);

            // Verify:
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user),
                "Couldn't login with the newly reset password!");

            TestHelper.AssertResponseBodyIsEmpty(response);
        }

        #endregion Positive tests
    }
}
