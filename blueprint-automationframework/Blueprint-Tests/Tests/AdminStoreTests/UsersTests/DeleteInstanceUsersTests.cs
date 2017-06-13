using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;
using System.Linq;
using Model.Common.Enums;
using Model.Factories;
using Model.NovaModel.AdminStoreModel;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]
    public class DeleteInstanceUserTests : TestBase
    {
        private const string USER_PATH = RestPaths.Svc.AdminStore.Users.USERS_id_;

        private IUser _adminUser;

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
            Helper.DeleteInstanceUsers(_adminUser);

            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Positive tests

        [TestCase]
        [Description("Create and add an instance user. Using the id of the created user, delete the user. " +
                     "Verify the user has been deleted.")]
        [TestRail(303462)]
        public void DeleteInstanceUser_ValidUser_UserDeleted()
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

            // Execute:
            DeleteResult result = null;

            Assert.DoesNotThrow(() =>
            {
                result = Helper.AdminStore.DeleteUser(_adminUser, createdUser.Id.Value);
            }, "'DELETE {0}' should return 200 OK for a valid session token!", USER_PATH);

            // Verify:
            Assert.AreEqual(1, result.TotalDeleted, "There should be 1 user deleted!");
            AssertUserNotFound(createdUser);
        }

        [TestCase]
        [Description("Create and add 3 instance users.  Delete 2 of the users.  " +
                     "Verify the 2 specified users have been deleted, but the 3rd user still exists.")]
        [TestRail(305040)]
        public void DeleteInstanceUsers_MultipleValidUsers_UsersAreDeleted()
        {
            // Setup:
            var userToKeep = Helper.CreateAndAddInstanceUser(_adminUser);
            var usersToDelete = new List<InstanceUser>();
            usersToDelete.Add(Helper.CreateAndAddInstanceUser(_adminUser));
            usersToDelete.Add(Helper.CreateAndAddInstanceUser(_adminUser));

            // Execute:
            DeleteResult result = null;

            Assert.DoesNotThrow(() =>
            {
                result = Helper.AdminStore.DeleteUsers(_adminUser, usersToDelete.Select(u => u.Id.Value).ToList());
            }, "'DELETE {0}' should return 200 OK for a valid session token!", USER_PATH);

            // Verify:
            foreach (var user in usersToDelete)
            {
                AssertUserNotFound(user);
            }

            Assert.AreEqual(usersToDelete.Count, result.TotalDeleted, "There should be {0} users deleted!", usersToDelete.Count);
            AssertUserExists(userToKeep);
        }

        [TestCase]
        [Description("Create and add an instance user. Using the id of the created user, delete the user, then delete the same user again. " +
                     "Verify the 2nd delete call returns 200 OK with TotalDeleted = 0.")]
        [TestRail(303467)]
        public void DeleteInstanceUser_DeletedUser_TotalDeletedZero()
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);
            Helper.AdminStore.DeleteUser(_adminUser, createdUser.Id.Value);
            
            // Execute:
            DeleteResult result = null;

            Assert.DoesNotThrow(() =>
            {
                result = Helper.AdminStore.DeleteUser(_adminUser, createdUser.Id.Value);
            }, "'DELETE {0}' should return 200 OK for a valid session token!", USER_PATH);

            // Verify:
            Assert.AreEqual(0, result.TotalDeleted, "There should be 0 users deleted!");
            AssertUserNotFound(createdUser);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [Description("Try to delete a user with an invalid Id.  Verify the 2nd delete call returns 200 OK with TotalDeleted = 0.")]
        [TestRail(303466)]
        public void DeleteInstanceUser_UserDoesNotExist_TotalDeletedZero(int userId)
        {
            // Execute:
            DeleteResult result = null;

            Assert.DoesNotThrow(() =>
            {
                result = Helper.AdminStore.DeleteUser(_adminUser, userId);
            }, "'DELETE {0}' should return 404 Not Found!", USER_PATH);

            // Verify:
            Assert.AreEqual(0, result.TotalDeleted, "There should be 0 users deleted!");
        }

        [TestCase]
        [Description("Create and add 2 instance users & delete the first one.  Delete both users.  " +
                     "Verify the second user was deleted and the call returns 200 OK with TotalDeleted = 1.")]
        [TestRail(305042)]
        public void DeleteInstanceUsers_ActiveAndDeletedUsers_TotalDeletedOne()
        {
            // Setup:
            var usersToDelete = new List<InstanceUser>();
            usersToDelete.Add(Helper.CreateAndAddInstanceUser(_adminUser));
            usersToDelete.Add(Helper.CreateAndAddInstanceUser(_adminUser));

            Helper.AdminStore.DeleteUser(_adminUser, usersToDelete[0].Id.Value);

            // Execute:
            DeleteResult result = null;

            Assert.DoesNotThrow(() =>
            {
                result = Helper.AdminStore.DeleteUsers(_adminUser, usersToDelete.Select(u => u.Id.Value).ToList());
            }, "'DELETE {0}' should return 200 OK for a valid session token!", USER_PATH);

            // Verify:
            foreach (var user in usersToDelete)
            {
                AssertUserNotFound(user);
            }

            Assert.AreEqual(1, result.TotalDeleted, "There should be 1 user deleted!");
        }

        [TestCase]
        [Description("Create and add an instance admin user.  Have the admin user try to delete themself.  " +
                     "Verify the user is not deleted.")]
        [TestRail(303509)]
        public void DeleteInstanceUser_DeleteCurrentUser_VerifyUserNotDeleted()
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser,
                licenseLevel: LicenseLevel.Author,
                instanceAdminRole: InstanceAdminRole.DefaultInstanceAdministrator,
                adminPrivileges: InstanceAdminPrivileges.ManageUsers,
                expirePassword: false);

            var user = UserFactory.ConvertInstanceUserToIUser(createdUser);

            Helper.AdminStore.AddSession(user);

            // Execute:
            DeleteResult result = null;

            Assert.DoesNotThrow(() =>
            {
                result = Helper.AdminStore.DeleteUser(user, createdUser.Id.Value);
            }, "'DELETE {0}' should return 200 OK for a valid session token!", USER_PATH);

            // Verify:
            Assert.AreEqual(0, result.TotalDeleted, "There should be no users deleted!");
            AssertUserExists(createdUser);
        }

        #endregion Positive tests

        #region Negative tests

        [TestCase(null, InstanceAdminErrorMessages.TokenMissingOrMalformed)]
        [TestCase("", InstanceAdminErrorMessages.TokenInvalid)]
        [TestCase(CommonConstants.InvalidToken, InstanceAdminErrorMessages.TokenInvalid)]
        [Description("Create and add an instance user.  Try to delete the user with a missing or invalid session token.  " +
                     "Verify 401 Unauthorized is returned.")]
        [TestRail(303463)]
        public void DeleteInstanceUser_MissingOrInvalidSessionToken_401Unauthorized(string token, string expectedError)
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);
            var userWithInvalidToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                badToken: token);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.DeleteUser(userWithInvalidToken, createdUser.Id.Value);
            }, "'DELETE {0}' should return 401 Unauthorized with a missing or invalid session token!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedError);
            AssertUserExists(createdUser);
        }

        [TestCase(InstanceAdminPrivileges.ManageUsersOnly)]
        [TestCase(InstanceAdminPrivileges.ViewUsers)]
        [Description("Create and add an instance user.  Try to delete the user with a user that doesn't have permission to view or manage users.  " +
                     "Verify 403 Forbidden is returned.")]
        [TestRail(303465)]
        public void DeleteInstanceUser_NoPermissionsToDeleteUser_403Forbidden(InstanceAdminPrivileges privilegeToRemove)
        {
            using (var adminStoreHelper = new AdminStoreHelper())
            {
                // Setup:
                var allPrivilegesExceptManageUsers = (InstanceAdminPrivileges) int.MaxValue & ~privilegeToRemove;
                var adminRole = adminStoreHelper.AddInstanceAdminRoleToDatabase(allPrivilegesExceptManageUsers);

                var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);
                var userWithNoPermissionsToManageUsers = Helper.CreateUserAndAuthenticate(
                    TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

                // Execute:
                var ex = Assert.Throws<Http403ForbiddenException>(() =>
                {
                    Helper.AdminStore.DeleteUser(userWithNoPermissionsToManageUsers, createdUser.Id.Value);
                }, "'DELETE {0}' should return 403 Forbidden when the user doesn't have permission to delete users!", USER_PATH);

                // Verify:
                TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.Forbidden, InstanceAdminErrorMessages.UserDoesNotHavePermissions);
                AssertUserExists(createdUser);
            }
        }

        #endregion Negative tests

        #region Private methods

        /// <summary>
        /// Asserts that the specified user exists and whether or not the user is enabled.
        /// </summary>
        /// <param name="user">The user whose existence is being checked.</param>
        /// <param name="isEnabled">(optional) Pass false if you expect the user to be disabled.</param>
        private void AssertUserExists(InstanceUser user, bool isEnabled = true)
        {
            InstanceUser returnedUser = null;

            Assert.DoesNotThrow(() =>
            {
                returnedUser = Helper.AdminStore.GetUserById(_adminUser, user.Id);
            }, "'GET {0}' should return 404 Not Found for a valid session token!", USER_PATH);

            Assert.AreEqual(isEnabled, returnedUser.Enabled, "The user should be {0}!", isEnabled ? "enabled" : "disabled");
        }

        /// <summary>
        /// Asserts that the specified user is deleted or doesn't exist.
        /// </summary>
        /// <param name="user">The user whose existence is being checked.</param>
        private void AssertUserNotFound(InstanceUser user)
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.GetUserById(_adminUser, user.Id);
            }, "'GET {0}' should return 404 Not Found for a valid session token!", USER_PATH);
        }

        #endregion Private methods
    }
}
