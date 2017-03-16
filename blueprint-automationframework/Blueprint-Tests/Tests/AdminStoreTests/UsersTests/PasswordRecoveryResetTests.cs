﻿using System;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;

namespace AdminStoreTests.UsersTests
{
    [Category(Categories.AdminStore)]
    [TestFixture]
    public class PasswordRecoveryResetTests : TestBase
    {
        private const string REST_PATH = RestPaths.Svc.AdminStore.Users.PasswordRecovery.RESET;

        private const string INVALID_PASSWORD_MESSAGE           = "Password reset failed, new password is invalid";
        private const string EMPTY_PASSWORD_MESSAGE             = "Password reset failed, new password cannot be empty";
        private const string NOT_LATEST_TOKEN_MESSAGE           = "Password reset failed, a more recent recovery token exists.";
        private const string PASSWORD_RESET_COOLDOWN_MESSAGE    = "Password reset failed, password reset cooldown in effect";
        private const string TOKEN_NOT_FOUND_MESSAGE            = "Password reset failed, recovery token not found.";
        private const string TOKEN_NOT_PROVIDED_MESSAGE         = "Password reset failed, token not provided";
        private const string USER_DOES_NOT_EXIST_MESSAGE        = "Password reset failed, the user does not exist.";
        private const string USER_IS_DISABLED_MESSAGE           = "Password reset failed, the login for this user is disabled.";

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

            user.Password = AdminStoreHelper.GenerateValidPassword();

            RestResponse response = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                response = Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, user.Password);
            }, "'POST {0}' should return 200 OK when passed a valid token and password.", REST_PATH);

            // Verify:
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user, force: true),
                "Couldn't login with the newly reset password!");

            TestHelper.AssertResponseBodyIsEmpty(response);
        }

        #endregion Positive tests

        #region Negative tests

        [TestCase(null)]
        [TestCase("")]
        [Description("Call Password Recovery Reset and send only a valid new password.  Verify 400 Bad Request is returned.")]
        [TestRail(266996)]
        public void PasswordRecoveryReset_MissingToken_400BadRequest(string recoveryToken)
        {
            // Setup:
            string newPassword = AdminStoreHelper.GenerateValidPassword();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken, newPassword: newPassword);
            }, "'POST {0}' should return 400 Bad Request when the token is missing.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordResetEmptyToken, TOKEN_NOT_PROVIDED_MESSAGE);
        }

        [TestCase]
        [Description("Create a user and get a Password Recovery Token.  Call Password Recovery Reset and send only the recovery token.  " +
                     "Verify 400 Bad Request is returned.")]
        [TestRail(266997)]
        public void PasswordRecoveryReset_MissingPassword_400BadRequest()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, newPassword: null);
            }, "'POST {0}' should return 400 Bad Request when the new password is missing.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.EmptyPassword, EMPTY_PASSWORD_MESSAGE);

            // Validate user's password wasn't changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
        }

        [TestCase("AAbb11$")]   // Too short.
        [TestCase("aaaa11$$")]  // Missing capital char.
        [TestCase("aaaa11BB")]  // Missing special char.
        [TestCase("AAAA$$$$")]  // Missing number.
        [Description("Call Password Recovery Reset and pass an invalid new password.  Verify 400 Bad Request is returned.")]
        [TestRail(267000)]
        public void PasswordRecoveryReset_InvalidPassword_400BadRequest(string invalidPassword)
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, invalidPassword);
            }, "'POST {0}' should return 400 Bad Request when passed an invalid new password.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.TooSimplePassword, INVALID_PASSWORD_MESSAGE);

            // Validate user's password wasn't changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
        }

        [TestCase]
        [Description("Call Password Recovery Reset and pass the same old password as your new password.  Verify 400 Bad Request is returned.")]
        [TestRail(267112)]
        public void PasswordRecoveryReset_SamePassword_400BadRequest()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, user.Password);
            }, "'POST {0}' should return 400 Bad Request when the new password is the same as the old password.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.TooSimplePassword, INVALID_PASSWORD_MESSAGE);

            // Validate user's password wasn't changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
        }

        [TestCase]
        [Description("Create a user and request a password reset for that user.  Delete the user, then try to reset their password.  " +
                     "Verify 409 Conflict is returned and the user is still deleted.")]
        [TestRail(266998)]
        public void PasswordRecoveryReset_DeletedUser_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);
            string newPassword = AdminStoreHelper.GenerateValidPassword();

            user.DeleteUser();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, newPassword);
            }, "'POST {0}' should return 409 Conflict when passed a valid token & password for a deleted user.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordResetTokenInvalid, USER_DOES_NOT_EXIST_MESSAGE);

            AdminStoreHelper.AssertUserNotFound(Helper, _adminUser, user.Id);
        }

        [TestCase(CommonConstants.InvalidToken)]
        [Description("Call Password Recovery Reset and pass an invalid recovery token.  Verify 409 Conflict is returned.")]
        [TestRail(266999)]
        public void PasswordRecoveryReset_InvalidToken_409Conflict(string invalidToken)
        {
            // Setup:
            string newPassword = AdminStoreHelper.GenerateValidPassword();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(invalidToken, newPassword);
            }, "'POST {0}' should return 409 Conflict when passed an invalid recovery token.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordResetTokenNotFound, TOKEN_NOT_FOUND_MESSAGE);
        }

        [TestCase]
        [Description("Create a user and request a password reset for that user.  Disable the user, then try to reset their password.  " +
                     "Verify 409 Conflict is returned and the user is still disabled.")]
        [TestRail(267001)]
        public void PasswordRecoveryReset_DisabledUser_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);
            string newPassword = AdminStoreHelper.GenerateValidPassword();

            user.Enabled = false;
            user.UpdateUser();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, newPassword);
            }, "'POST {0}' should return 409 Conflict when passed a valid token & password for a disabled user.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordResetUserDisabled, USER_IS_DISABLED_MESSAGE);

            AdminStoreHelper.AssertUserIsDisabled(Helper, user);
        }

        [TestCase]
        [Description("Create a user and get a password recovery token, then change the user's password.  Try to Reset the user's password.  " +
                     "Verify 409 Conflict is returned.")]
        [TestRail(267002)]
        public void PasswordRecoveryReset_PasswordRecentlyChanged_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);
            string newPassword = AdminStoreHelper.GenerateValidPassword();

            // Change the user's password the normal way.
            Helper.AdminStore.ChangePassword(user, newPassword);
            user.Password = newPassword;

            // Update the LastPasswordChangeTimestamp to 23h ago.
            user.ChangeLastPasswordChangeTimestamp(DateTime.UtcNow.AddHours(-23));

            newPassword = AdminStoreHelper.GenerateValidPassword();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, newPassword);
            }, "'POST {0}' should return 409 Conflict when the user's password was changed within the last 24h.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ChangePasswordCooldownInEffect, PASSWORD_RESET_COOLDOWN_MESSAGE);

            // Validate user's password wasn't changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
        }

        [TestCase]
        [Description("Create a user and get a password recovery token, then reset the user's password.  Try to Reset the user's password again " +
                     "with the same recovery token.  Verify 409 Conflict is returned.")]
        [TestRail(267003)]
        public void PasswordRecoveryReset_ResetTwiceWithSameToken_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);
            string newPassword = AdminStoreHelper.GenerateValidPassword();

            // Change the user's password the normal way.
            Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, newPassword);
            user.Password = newPassword;

            newPassword = AdminStoreHelper.GenerateValidPassword();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, newPassword);
            }, "'POST {0}' should return 409 Conflict when resetting a user's password twice with the same recovery token.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ChangePasswordCooldownInEffect, PASSWORD_RESET_COOLDOWN_MESSAGE);

            // Validate user's password wasn't changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
        }

        [TestCase]
        [Description("Create a user and request a password recovery token, then request another recovery token.  Try to Reset the user's password using " +
                     "the first recovery token.  Verify 409 Conflict is returned.")]
        [TestRail(267004)]
        public void PasswordRecoveryReset_GetTwoRecoveryTokens_ResetWithFirstToken_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var firstRecoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);
            string newPassword = AdminStoreHelper.GenerateValidPassword();

            // Request a 2nd recovery token.
            Helper.AdminStore.PasswordRecoveryRequest(user.Username);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(firstRecoveryToken.RecoveryToken, newPassword);
            }, "'POST {0}' should return 409 Conflict when using the first recovery token after you requested a second token.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordResetTokenNotLatest, NOT_LATEST_TOKEN_MESSAGE);

            // Validate user's password wasn't changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
        }

        #endregion Negative tests
    }
}
