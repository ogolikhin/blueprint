﻿using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.Common.Enums;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]
    public class InstanceAdminChangePasswordTests : TestBase
    {
        private const string USER_CHANGE_PASSWORD = RestPaths.Svc.AdminStore.Users.CHANGE_PASSWORD;

        private const uint MinPasswordLength = AdminStoreHelper.MinPasswordLength;
        private const uint MaxPasswordLength = AdminStoreHelper.MaxPasswordLength;

        private const string CANNOTUSELASTPASSWORDS = "CannotUseLastPasswords";

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
            Helper.DeleteInstanceUsers(_adminUser);

            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase(MinPasswordLength)]
        [TestCase(MaxPasswordLength)]
        [TestRail(308863)]
        [Description("Change a user's password to a valid new password.  The password should be changed successfully and " +
                     "the user should be able to login with the new password.")]
        public void InstanceAdminChangePassword_ChangeUserPassword_VerifyUserCanLoginWithNewPassword(uint length)
        {
            // Setup: Create and add user with valid password.
            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);

            string newPassword = AdminStoreHelper.GenerateValidPassword(length);

            // Execute: change the user's password.
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, newPassword);
            }, "'POST {0}' failed when we passed a valid username & password!", USER_CHANGE_PASSWORD);

            // Verify: make sure user can login with the new password.
            Helper.AssertUserCanLogin(instanceUser.Login, newPassword);
        }

        [TestCase]
        [Description("Create and add an instance user.  Change the password of the user with another user that has " +
                     "permission to manage users. The password should be changed successfully and the user should " +
                     "be able to login with the new password.")]
        [TestRail(308872)]
        public void InstanceAdminChangePassword_PermissionsToManageUsers_VerifyUserCanLoginWithNewPassword()
        {
            using (var adminStoreHelper = new AdminStoreHelper())
            {
                // Setup:
                var adminRole = adminStoreHelper.AddInstanceAdminRoleToDatabase(InstanceAdminPrivileges.ManageUsers);
                var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);

                var userWithPermissionsToManageUsers = Helper.CreateUserAndAuthenticate(
                    TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

                string newPassword = AdminStoreHelper.GenerateValidPassword();

                // Execute:
                Assert.DoesNotThrow(() =>
                {
                    Helper.AdminStore.InstanceAdminChangePassword(userWithPermissionsToManageUsers, instanceUser, newPassword);
                }, "'POST {0}' failed when we passed a valid username & password!", USER_CHANGE_PASSWORD);

                // Verify: make sure user can login with the new password.
                Helper.AssertUserCanLogin(instanceUser.Login, newPassword);
            }
        }

        [Category(Categories.CannotRunInParallel)]
        [TestCase]
        [TestRail(308885)]
        [Description("Change a user's password to a valid new password 2 times in a row.  The password should be changed " +
                     "successfully and the user should be able to login with the new password.")]
        public void InstanceAdminChangePassword_ChangePasswordTwiceWithin24Hrs_VerifyUserCanLoginWithNewPassword()
        {
            // Setup: Create and add user with valid password.
            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);

            string firstPassword = AdminStoreHelper.GenerateValidPassword();

            // First password change
            Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, firstPassword);

            string secondPassword = AdminStoreHelper.GenerateValidPassword();

            // Set CannotUseLastPasswords in instance table and store original value
            var originalCannotUseLastPasswords = TestHelper.GetValueFromInstancesTable(CANNOTUSELASTPASSWORDS);
            TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, "10");

            // Execute: Second password change.
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, secondPassword);
                }, "'POST {0}' failed when attempting to change password 2 times within 24 Hrs!", USER_CHANGE_PASSWORD);

                // Verify: make sure user can login with the new password.
                Helper.AssertUserCanLogin(instanceUser.Login, secondPassword);
            }
            finally
            {
                // Restore CannotUseLastPasswords back to original value.
                TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, originalCannotUseLastPasswords);
            }
        }

        [Category(Categories.CannotRunInParallel)]
        [TestCase]
        [TestRail(308886)]
        [Description("Change a user's password to using the existing user password.  The password should be changed " +
                     "successfully and the user should be able to login with the password.")]
        public void InstanceAdminChangePassword_ChangePasswordToExistingPassword_VerifyUserCanLoginWithNewPassword()
        {
            // Setup: Create and add user with valid password.
            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);

            // Set CannotUseLastPasswords in instance table and store original value
            var originalCannotUseLastPasswords = TestHelper.GetValueFromInstancesTable(CANNOTUSELASTPASSWORDS);
            TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, "10");

            try
            {
                // Execute:.
                Assert.DoesNotThrow(() =>
                {
                    Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, instanceUser.Password);
                }, "'POST {0}' failed when attempting to change passord to the existing password!", USER_CHANGE_PASSWORD);

                // Verify: make sure user can login with the original password.
                Helper.AssertUserCanLogin(instanceUser.Login, instanceUser.Password);
            }
            finally
            {
                // Restore CannotUseLastPasswords back to original value.
                TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, originalCannotUseLastPasswords);
            }
        }

        [Category(Categories.CannotRunInParallel)]
        [TestCase]
        [TestRail(308887)]
        [Description("Change a user's password to a previously used password.  The password should be changed " +
                     "successfully and the user should be able to login with the new password.")]
        public void InstanceAdminChangePassword_ChangePasswordToPreviousPassword_VerifyUserCanLoginWithNewPassword()
        {
            // Setup: Create and add user with valid password.
            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);
            string originalPassword = instanceUser.Password;

            // Password change to new passowrd
            string newPassword = AdminStoreHelper.GenerateValidPassword();
            Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, newPassword);

            // Set CannotUseLastPasswords in instance table and store original value
            var originalCannotUseLastPasswords = TestHelper.GetValueFromInstancesTable(CANNOTUSELASTPASSWORDS);
            TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, "10");

            try
            {
                // Execute: Change password back to original password.
                Assert.DoesNotThrow(() =>
                {
                    Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, originalPassword);
                }, "'POST {0}' failed when attempting to change password to a previously used password!", USER_CHANGE_PASSWORD);

                // Verify: make sure user can login with the original password.
                Helper.AssertUserCanLogin(instanceUser.Login, originalPassword);
            }
            finally
            {
                // Restore CannotUseLastPasswords back to original value.
                TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, originalCannotUseLastPasswords);
            }
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase(null)]
        [TestCase("")]
        [TestRail(308864)]
        [Description("Try to change the user's password to a null or blank password. Verify 400 Bad Request is returned and " +
                     "that the user is still able to login with their old password.")]
        public void InstanceAdminChangePassword_SendBlankNewPassword_400BadRequest(string newPassword)
        {
            // Setup: Create and add user with valid password.
            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);

            // Execute: try to change the user's password to an invalid password.
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, newPassword);
            }, "'POST {0}' should get a 400 Bad Request when passing a null or empty new password!", USER_CHANGE_PASSWORD);

            // Verify: make sure user can still login with the old password.
            Helper.AssertUserCanLogin(instanceUser.Login, instanceUser.Password);

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.BadRequest, InstanceAdminErrorMessages.PasswordMissing);
        }

        [TestCase("Blueprint1234", InstanceAdminErrorMessages.PasswordDoesNotHaveNonAlphanumeric)]
        [TestCase("blueprint1234!", InstanceAdminErrorMessages.PasswordDoesNotHaveUpperCase)]
        [TestCase("Blueprint!", InstanceAdminErrorMessages.PasswordDoesNotHaveNumber)]
        [TestRail(308865)]
        [Description("Try to change the user's password to a value without the required complexity. Verify that 400 Bad Request " +
                     "is returned and that the user is still be able to login with their old password.")]
        public void InstanceAdminChangePassword_SendNonComplexPassword_400BadRequest(string newPassword, string errorMessage)
        {
            // Setup: Create and add user with valid password.
            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);

            // Execute: try to change the user's password to an invalid password.
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, newPassword);
            }, "'POST {0}' should get a 400 Bad Request when the new password doesn't meet the complexity rules!", USER_CHANGE_PASSWORD);

            // Verify: make sure user can still login with their old password.
            Helper.AssertUserCanLogin(instanceUser.Login, instanceUser.Password);

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.BadRequest, errorMessage);
        }

        [TestCase(MinPasswordLength - 1)]
        [TestCase(MaxPasswordLength + 1)]
        [TestRail(308866)]
        [Description("Try to change the user's password to a value that doesn't meet length requirements. Verify that 400 Bad Request " +
                     "is returned and that the user is still able to login with their old password.")]
        public void InstanceAdminChangePassword_SendNewPasswordWithInvalidLength_400BadRequest(uint length)
        {
            // Setup: Create and add user with valid password.
            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);

            // Generate an invalid password.
            string newPassword = AdminStoreHelper.GenerateValidPassword(length, skipLengthRequirement: true);

            // Execute: try to change the user's password to an invalid password.
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, newPassword);
            }, "'POST {0}' should get a 400 Bad Request when passing new password that is too short or too long!", USER_CHANGE_PASSWORD);

            // Verify: make sure user can still login with their old password.
            Helper.AssertUserCanLogin(instanceUser.Login, instanceUser.Password);

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.BadRequest, InstanceAdminErrorMessages.PasswordInvalidLength);
        }

        [TestCase]
        [TestRail(308867)]
        [Description("Try to change the user's password to a string value identical to the username of the user. " +
                     "Verify that 400 Bad Request is returned and that the user can still login with their old password.")]
        public void InstanceAdminChangePassword_SendUserNameAsNewPassword_400BadRequest()
        {
            // Setup: Create and add user with valid password.
            // Need a login that will pass the password validation
            string login = AdminStoreHelper.GenerateValidPassword();

            // Need to override email address since it cannot contain some special characters
            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser, login: login, email: "user@nowhere.com");

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                {
                    Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, instanceUser.Login);
                }, "'POST {0}' should get a 400 Bad Request when passing its user name as a new password!", USER_CHANGE_PASSWORD);

            // Verify: Make sure the user can still login with their old password.
            Helper.AssertUserCanLogin(instanceUser.Login, instanceUser.Password);

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordSameAsLogin, InstanceAdminErrorMessages.PasswordSameAsLogin);
        }

        [TestCase]
        [TestRail(308868)]
        [Description("Try to change the user's password to a string value identical to the user's display name. " +
                     "Verify that 400 Bad Request is returned and that the user can still login with their old password.")]
        public void InstanceAdminChangePassword_SendDisplayNameAsNewPassword_400BadRequest()
        {
            // Setup: Create and add user with valid password.
            // Need a display name that will pass the password validation
            string displayName = AdminStoreHelper.GenerateValidPassword();

            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser, displayname: displayName);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, instanceUser.DisplayName); 
                
            }, "'POST {0}' should get a 400 Bad Request when passing its display name as a new password!", USER_CHANGE_PASSWORD);

            // Verify: Make sure the user can still login with their old password.
            Helper.AssertUserCanLogin(instanceUser.Login, instanceUser.Password);

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordSameAsDisplayName, InstanceAdminErrorMessages.PasswordSameAsDisplayName);
        }

        [Explicit(IgnoreReasons.ProductBug)]  // Trello: https://trello.com/c/XsGpLoAq
        [TestCase]
        [TestRail(308869)]
        [Description("Try to pass a non-Base64 encoded value as the new password to the back end API call. Verify that " +
                     "400 Bad Request is returned and that the user can still login with their old password.")]
        public void InstanceAdminChangePassword_SendNonBase64StringAsNewPassword_400BadRequest()
        {
            // Setup: Create and add user with valid password.
            var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);

            string newPassword = AdminStoreHelper.GenerateValidPassword();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.InstanceAdminChangePassword(_adminUser, instanceUser, newPassword, encodePassword: false);
                
            }, "'POST {0}' should get a 400 Bad Request when passing a non-Base64 value as a new password!", USER_CHANGE_PASSWORD);

            // Verify: Make sure the user can still login with their old password.
            Helper.AssertUserCanLogin(instanceUser.Login, instanceUser.Password);

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.BadRequest, InstanceAdminErrorMessages.PasswordIsNotBase64);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase(null, InstanceAdminErrorMessages.TokenMissingOrMalformed)]
        [TestCase("", InstanceAdminErrorMessages.TokenInvalid)]
        [TestCase(CommonConstants.InvalidToken, InstanceAdminErrorMessages.TokenInvalid)]
        [Description("Create and add an instance user. Try to change the user's password using an invalid token header. " +
                     "Verify that 401 Unauthorized is returned.")]
        [TestRail(308870)]
        public void InstanceAdminChangePassword_InvalidTokenHeader_401Unauthorized(string tokenString, string errorMessage)
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

            var userWithInvalidTokenHeader = Helper.CreateUserWithInvalidToken(
                TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.DefaultInstanceAdministrator,
                badToken: tokenString);

            string newPassword = AdminStoreHelper.GenerateValidPassword();

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.InstanceAdminChangePassword(userWithInvalidTokenHeader, createdUser, newPassword);
            }, "'POST {0}' should return 401 Unauthorized with invalid token header!", USER_CHANGE_PASSWORD);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, errorMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase(InstanceAdminPrivileges.ManageUsersOnly)]
        [TestCase(InstanceAdminPrivileges.ViewUsers)]
        [Description("Create and add an instance user. Try to change the user's password with another user that does not have " +
                     "permission to manage users.  Verify that 403 Forbidden is returned.")]
        [TestRail(308871)]
        public void InstanceAdminChangePassword_NoPermissionsToManageUsers_403Forbidden(InstanceAdminPrivileges privilegeToRemove)
        {
            using (var adminStoreHelper = new AdminStoreHelper())
            {
                // Setup:
                var allPrivilegesExceptManageUsers = (InstanceAdminPrivileges)int.MaxValue & ~privilegeToRemove;
                var adminRole = adminStoreHelper.AddInstanceAdminRoleToDatabase(allPrivilegesExceptManageUsers);
                var instanceUser = Helper.CreateAndAddInstanceUser(_adminUser);

                var userWithNoPermissionsToManageUsers = Helper.CreateUserAndAuthenticate(
                    TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

                string newPassword = AdminStoreHelper.GenerateValidPassword();

                // Execute:
                var ex = Assert.Throws<Http403ForbiddenException>(() =>
                {
                    Helper.AdminStore.InstanceAdminChangePassword(userWithNoPermissionsToManageUsers, instanceUser, newPassword);
                }, "'POST {0}' should return 403 Forbidden when the user has no permissions to manage users!",
                    USER_CHANGE_PASSWORD);

                // Verify:
                TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.Forbidden, InstanceAdminErrorMessages.UserDoesNotHavePermissions);
            }
        }

        #endregion 403 Forbidden Tests

        #region 404 Not Found

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [Description("Try to change the password of a user with an invalid Id.  Verify that 404 Not Found is returned.")]
        [TestRail(308883)]
        public void InstanceAdminChangePassword_InvalidUserId_404NotFound(int invalidId)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();
            createdUser.Id = invalidId;

            string newPassword = AdminStoreHelper.GenerateValidPassword();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.InstanceAdminChangePassword(_adminUser, createdUser, newPassword);
            }, "'POST {0}' should return 404 Not Found!", USER_CHANGE_PASSWORD);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, InstanceAdminErrorMessages.UserNotExist);
        }

        [TestCase]
        [Description("Create and add an instance user. Delete the user. Try to change the password of the deleted user. " +
                     "Verify that 404 Not Found is returned.")]
        [TestRail(308884)]
        public void InstanceAdminChangePassword_UserDeleted_404NotFound()
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

            Helper.AdminStore.DeleteUsers(_adminUser, new List<int> { createdUser.Id.Value });

            string newPassword = AdminStoreHelper.GenerateValidPassword();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.InstanceAdminChangePassword(_adminUser, createdUser, newPassword);
            }, "'POST {0}' should return 404 Not Found for deleted user!", USER_CHANGE_PASSWORD);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, InstanceAdminErrorMessages.UserNotExist);
        }

        #endregion 404 Not Found
    }
}