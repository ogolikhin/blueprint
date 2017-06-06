using CustomAttributes;
using Helper;
using Model;
using Model.Common.Enums;
using Model.NovaModel.AdminStoreModel;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]

    public class GetInstanceUserTests : TestBase
    {
        private const string USER_PATH_ID = RestPaths.Svc.AdminStore.Users.USERS_id_;

        private IUser _adminUser = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase]
        [Description("Create and add a default instance user. Get the added user using a user that only has permissions to " +
                     "view users. Verify the same user that was created is returned.")]
        [TestRail(303341)]
        public void GetInstanceUser_PermissionsToGetUser_ReturnsCorrectUser()
        {
            using (var adminStoreHelper = new AdminStoreHelper())
            {
                // Setup:
                var adminRole = adminStoreHelper.AddInstanceAdminRoleToDatabase(InstanceAdminPrivileges.ViewUsers);
                var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

                var userWithPermissionsToGetUsers = Helper.CreateUserAndAuthenticate(
                    TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

                InstanceUser returnedUser = null;

                // Execute:
                Assert.DoesNotThrow(() =>
                {
                    returnedUser = Helper.AdminStore.GetUserById(userWithPermissionsToGetUsers, createdUser.Id);
                }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

                //Verify:
                AdminStoreHelper.AssertAreEqual(createdUser, returnedUser);
            }
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase(null, InstanceAdminErrorMessages.TokenMissingOrMalformed)]
        [TestCase("", InstanceAdminErrorMessages.TokenInvalid)]
        [TestCase(CommonConstants.InvalidToken, InstanceAdminErrorMessages.TokenInvalid)]
        [Description("Create and add an instance user. Try to get the user using an invalid token header. " +
                     "Verify that 401 Unauthorized is returned.")]
        [TestRail(303451)]
        public void GetInstanceUser_InvalidTokenHeader_401Unauthorized(string tokenString, string errorMessage)
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

            var userWithInvalidTokenHeader = Helper.CreateUserWithInvalidToken(
                TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.DefaultInstanceAdministrator,
                badToken: tokenString);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetUserById(userWithInvalidTokenHeader, createdUser.Id);
            }, "'GET {0}' should return 401 Unauthorized with invalid token header!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, errorMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase]
        [Description("Create and add an instance user.  Try to get the user with another user that has all permissions except " +
                     "permission to view users. Verify that 403 Forbidden is returned.")]
        [TestRail(303452)]
        public void GetInstanceUser_NoPermissionsToGetUsers_403Forbidden()
        {
            using (var adminStoreHelper = new AdminStoreHelper())
            {
                // Setup:
                var allPrivilegesExceptViewUsers = (InstanceAdminPrivileges) int.MaxValue & ~InstanceAdminPrivileges.ViewUsers;
                var adminRole = adminStoreHelper.AddInstanceAdminRoleToDatabase(allPrivilegesExceptViewUsers);
                var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

                var userWithNoPermissionsToGetUsers = Helper.CreateUserAndAuthenticate(
                    TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

                // Execute:
                var ex = Assert.Throws<Http403ForbiddenException>(() =>
                {
                    Helper.AdminStore.GetUserById(userWithNoPermissionsToGetUsers, createdUser.Id);
                }, "'GET {0}' should return 403 Forbidden when the user updating the user has no permissions to get users!", USER_PATH_ID);

                // Verify:
                TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.Forbidden, InstanceAdminErrorMessages.UserDoesNotHavePermissions);
            }
        }

        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [Description("Try to get a non-existing user. Verify that 404 Not Found is returned.")]
        [TestRail(303453)]
        public void GetInstanceUser_UserDoesntExist_404NotFound(int userId)
        {
            // Setup, Execute & Verify:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.GetUserById(_adminUser, userId);
            }, "'GET {0}' should return 404 Not Found for a non-existent user ID!", USER_PATH_ID);

            // Verify:
            if (userId > 0)
            {
                TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, InstanceAdminErrorMessages.UserNotExist);
            }
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create and add an instance user. Delete the user.  Try to get the deleted user. " +
                     "Verify that 404 Not Found is returned.")]
        [TestRail(303456)]
        public void GetInstanceUser_UserDeleted_404NotFound()
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

            Helper.AdminStore.DeleteUser(_adminUser, createdUser.Id.Value);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.GetUserById(_adminUser, createdUser.Id);
            }, "'GET {0}' should return 404 Not Found for deleted user!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, InstanceAdminErrorMessages.UserNotExist);
        }

        #endregion 404 Not Found Tests

    }
}
