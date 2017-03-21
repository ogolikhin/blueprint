using System;
using System.Threading;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
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

        private const string PasswordResetTokenExpirationInHours = "PasswordResetTokenExpirationInHours";

        private const string INVALID_PASSWORD_MESSAGE           = "Password reset failed, new password is invalid";
        private const string EMPTY_PASSWORD_MESSAGE             = "Password reset failed, new password cannot be empty";
        private const string PREVIOUS_PASSWORD_MESSAGE          = "The new password matches a previously used password.";
        private const string SAME_PASSWORD_MESSAGE              = "Password reset failed, new password cannot be equal to the old one";
        private const string NOT_LATEST_TOKEN_MESSAGE           = "Password reset failed, a more recent recovery token exists.";
        private const string PASSWORD_RESET_COOLDOWN_MESSAGE    = "Password reset failed, password reset cooldown in effect";
        private const string TOKEN_EXPIRED_MESSAGE              = "Password reset failed, recovery token expired.";
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
            _project = ProjectFactory.GetProject(_adminUser, shouldRetrieveArtifactTypes: false);
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

            Assert.DoesNotThrow(() => Helper.AdminStore.CheckSession(user),
                "User's session should be valid before a password reset!");

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
            TestHelper.AssertResponseBodyIsEmpty(response);

            // Verify that user's Nova session token is no longer valid.
            Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.CheckSession(user),
                "User's session should not be valid after a password reset!");

            // Verify the user's password was changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user, force: true),
                "Couldn't login with the newly reset password!");
        }

        [TestCase]
        [Description("Create a user and then request a password reset for that user; reset the recovery token's CreationTime to be 1 minute before it should expire; " +
                     "then reset the user's password with the recovery token.  Verify 200 OK is returned and the user's password was reset.")]
        [TestRail(267219)]
        public void PasswordRecoveryReset_TokenIsAlmostExpired_PasswordIsReset()
        {
            // Setup:
            var adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            Assert.DoesNotThrow(() => Helper.AdminStore.CheckSession(adminUser),
                "User's session should be valid before a password reset!");

            Helper.AdminStore.PasswordRecoveryRequest(adminUser.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(adminUser.Username);

            ChangeRecoveryTokenCreationTimeInDatabase(recoveryToken, addMinutesToCreationTime: 1); // Token is 1 minute before expiration date.

            adminUser.Password = AdminStoreHelper.GenerateValidPassword();

            RestResponse response = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                response = Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, adminUser.Password);
            }, "'POST {0}' should return 200 OK when passed a valid token and password.", REST_PATH);

            // Verify:
            TestHelper.AssertResponseBodyIsEmpty(response);

            // Verify that user's Nova session token is no longer valid.
            Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.CheckSession(adminUser),
                "User's session should not be valid after a password reset!");

            // Verify the user's password was changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(adminUser, force: true),
                "Couldn't login with the newly reset password!");
        }

        [Category(Categories.CannotRunInParallel)]  // Because it changes an ApplicationSetting in the database.
        [Category(Execute.Weekly)]  // Low priority test.
        [TestCase("")]
        [TestCase("0.5")]
        [TestCase("three")]
        [TestCase("999999999999")]
        [Description("Change the PasswordResetTokenExpirationInHours ApplicationSetting to a non-integer value; reset the recovery token's CreationTime " +
                     "to be 1 minute before it should expire; then call Password Recovery Reset and pass a valid recovery token & new password.  " +
                     "Verify 200 OK is returned and the user's password was reset.")]
        [TestRail(267227)]
        public void PasswordRecoveryReset_PasswordResetTokenExpirationInHoursSetToNonInteger_DefaultTokenExpirationTimeIsUsed(string badExpirationTime)
        {
            // Setup:
            var adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            Assert.DoesNotThrow(() => Helper.AdminStore.CheckSession(adminUser),
                "User's session should be valid before a password reset!");

            string oldValue = TestHelper.GetApplicationSetting(PasswordResetTokenExpirationInHours);

            try
            {
                TestHelper.UpdateApplicationSettings(PasswordResetTokenExpirationInHours, badExpirationTime);

                Helper.AdminStore.PasswordRecoveryRequest(adminUser.Username);
                var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(adminUser.Username);

                ChangeRecoveryTokenCreationTimeInDatabase(recoveryToken, addMinutesToCreationTime: 1, tokenLifespanInHours: 24); // Token is 1 minute before expiration date.

                adminUser.Password = AdminStoreHelper.GenerateValidPassword();

                RestResponse response = null;

                // Execute:
                Assert.DoesNotThrow(() =>
                {
                    response = Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, adminUser.Password);
                }, "'POST {0}' should return 200 OK when passed a valid token and password.", REST_PATH);

                // Verify:
                TestHelper.AssertResponseBodyIsEmpty(response);

                // Verify that user's Nova session token is no longer valid.
                Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.CheckSession(adminUser),
                    "User's session should not be valid after a password reset!");

                // Verify the user's password was changed.
                Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(adminUser, force: true),
                    "Couldn't login with the newly reset password!");
            }
            finally
            {
                TestHelper.UpdateApplicationSettings(PasswordResetTokenExpirationInHours, oldValue);
            }
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

        [TestCase(1, ErrorCodes.SamePassword, SAME_PASSWORD_MESSAGE)]
        [TestCase(3, ErrorCodes.PasswordAlreadyUsedPreviously, PREVIOUS_PASSWORD_MESSAGE)]
        [Description("Call Password Recovery Reset and pass a password that you used previously.  Verify 400 Bad Request is returned.")]
        [TestRail(267112)]
        public void PasswordRecoveryReset_PreviouslyUsedPassword_400BadRequest(int numberOfPreviousPasswords, int expectedErrorCode, string expectedErrorMessage)
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();
            var firstPassword = user.Password;

            for (int i = 1; i < numberOfPreviousPasswords; ++i)
            {
                string newPassword = AdminStoreHelper.GenerateValidPassword();

                Helper.AdminStore.ChangePassword(user, newPassword);

                user.Password = newPassword;
                user.ChangeLastPasswordChangeTimestamp(DateTime.UtcNow.AddHours(-25));
            }

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, firstPassword);
            }, "'POST {0}' should return 400 Bad Request when the new password is the same as the old password.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, expectedErrorCode, expectedErrorMessage);

            // Validate user's password wasn't changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
        }

        [TestCase]
        [Description("Call Password Recovery Reset and pass the Display Name as the new password.  Verify 400 Bad Request is returned.")]
        [TestRail(267113)]
        public void PasswordRecoveryReset_DisplayNameAsPassword_400BadRequest()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, user.DisplayName);
            }, "'POST {0}' should return 400 Bad Request when the Display Name is passed as the new password.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordSameAsDisplayName,
                "Password reset failed, new password cannot be equal to display name");

            // Validate user's password wasn't changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
        }

        [TestCase]
        [Description("Call Password Recovery Reset and pass the Username as the new password.  Verify 400 Bad Request is returned.")]
        [TestRail(267114)]
        public void PasswordRecoveryReset_UsernameAsPassword_400BadRequest()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, user.Username);
            }, "'POST {0}' should return 400 Bad Request when the Username is passed as the new password.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordSameAsLogin,
                "Password reset failed, new password cannot be equal to login name");

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
        [Description("Call Password Recovery Reset and pass an expired recovery token.  Verify 409 Conflict is returned.")]
        [TestRail(267116)]
        public void PasswordRecoveryReset_ExpiredToken_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();

            Helper.AdminStore.PasswordRecoveryRequest(user.Username);
            var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);
            string newPassword = AdminStoreHelper.GenerateValidPassword();

            ChangeRecoveryTokenCreationTimeInDatabase(recoveryToken, addMinutesToCreationTime: -1); // Token is 1 minute past expiration date.

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, newPassword);
            }, "'POST {0}' should return 409 Conflict when passed an invalid recovery token.", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordResetTokenExpired, TOKEN_EXPIRED_MESSAGE);

            // Validate user's password wasn't changed.
            Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
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

        [Category(Categories.CannotRunInParallel)]  // Because it changes an ApplicationSetting in the database.
        [Category(Execute.Weekly)]  // Low priority test.
        [TestCase("0")]
        [TestCase("-1")]
        [Description("Change the PasswordResetTokenExpirationInHours ApplicationSetting to non-positive integer; get a recovery token; " +
                     "then call Password Recovery Reset and pass a valid recovery token & new password.  Verify 409 Conflict is returned.")]
        [TestRail(267229)]
        public void PasswordRecoveryReset_PasswordResetTokenExpirationInHoursSetToNonPositiveInteger_409Conflict(string badExpirationTime)
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();
            string oldValue = TestHelper.GetApplicationSetting(PasswordResetTokenExpirationInHours);

            try
            {
                TestHelper.UpdateApplicationSettings(PasswordResetTokenExpirationInHours, badExpirationTime);

                Helper.AdminStore.PasswordRecoveryRequest(user.Username);
                var recoveryToken = AdminStoreHelper.GetRecoveryTokenFromDatabase(user.Username);

                string newPassword = AdminStoreHelper.GenerateValidPassword();

                RestResponse response = null;

                // Execute:
                var ex = Assert.Throws<Http409ConflictException>(() =>
                {
                    response = Helper.AdminStore.PasswordRecoveryReset(recoveryToken.RecoveryToken, newPassword);
                }, "'POST {0}' should return 409 Conflict when the {1} ApplicationSetting has a non-positive integer.",
                REST_PATH, PasswordResetTokenExpirationInHours);

                // Verify:
                TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordResetTokenExpired, TOKEN_EXPIRED_MESSAGE);

                // Validate user's password wasn't changed.
                Assert.DoesNotThrow(() => Helper.AdminStore.AddSession(user), "Couldn't login with the user's old password!");
            }
            finally
            {
                TestHelper.UpdateApplicationSettings(PasswordResetTokenExpirationInHours, oldValue);
            }
        }

        #endregion Negative tests

        #region Private functions

        /// <summary>
        /// Changes the CreationTime of the specified recovery token in the database to be close to the expiration time.
        /// Add minutes to keep the token valid or subtract minutes to expire the token.
        /// </summary>
        /// <param name="recoveryToken">The recovery token to expire.</param>
        /// <param name="addMinutesToCreationTime">The number of minutes to add to the recovery token CreationTime after subtracting the token lifespan.
        ///     Set to a positive number to make the token not expired or a negative number to make it expired.</param>
        /// <param name="tokenLifespanInHours">(optional) To bypass getting the value from the database and specify the token lifespan manually, pass a positive integer.
        ///     By default the PasswordResetTokenExpirationInHours value from the ApplicationSettings table will be used.</param>
        private static void ChangeRecoveryTokenCreationTimeInDatabase(AdminStoreHelper.PasswordRecoveryToken recoveryToken,
            int addMinutesToCreationTime,
            int? tokenLifespanInHours = null)
        {
            ThrowIf.ArgumentNull(recoveryToken, nameof(recoveryToken));

            if (tokenLifespanInHours == null)
            {
                tokenLifespanInHours = TestHelper.GetApplicationSetting("PasswordResetTokenExpirationInHours").ToInt32Invariant();
            }

            var newCreationTime = recoveryToken.CreationTime.AddHours(0 - tokenLifespanInHours.Value).AddMinutes(addMinutesToCreationTime);

            string query = I18NHelper.FormatInvariant(
                "UPDATE [dbo].[PasswordRecoveryTokens] SET [CreationTime] = '{0}' WHERE [Login] = '{1}' AND [RecoveryToken] = '{2}'",
                newCreationTime, recoveryToken.Login, recoveryToken.RecoveryToken);

            try
            {
                int rowsAffected = DatabaseHelper.ExecuteUpdateSqlQuery(query, "AdminStore");
                Assert.AreEqual(1, rowsAffected, "There should've been 1 row updated when running '{0}'!", query);
            }
            catch (SqlQueryFailedException)
            {
                Assert.Fail("No rows were updated when running: {0}", query);
            }
        }

        #endregion Private functions
    }
}
