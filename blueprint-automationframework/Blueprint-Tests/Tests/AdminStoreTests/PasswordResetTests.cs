using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace AdminStoreTests
{
    [Explicit(IgnoreReasons.UnderDevelopmentDev)]   // US 5412 still in development
    [TestFixture]
    public class PasswordResetTests : TestBase
    {
        private const string REST_PATH = RestPaths.Svc.AdminStore.Users.RESET;

        private IUser _adminUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
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
        [Description("Create a user and then request a password reset for that user.  Verify 200 OK is returned and a RecoveryToken for that user " +
                     "was added to the AdminStore database.")]
        [TestRail(266880)]
        public void RequestPasswordReset_ValidUsername_RecoveryTokenIsCreated(TestHelper.ProjectRole role)
        {
            // Setup:
            var user = Helper.CreateUserWithProjectRolePermissions(role, _project);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.RequestPasswordReset(user.Username);
            }, "'POST {0}' should return 200 OK when passed a valid username.", REST_PATH);

            // Verify:
            ValidateRecoveryToken(user.Username);
        }

        #endregion Positive tests

        #region Negative tests

        [TestCase]
        [Description("Create and delete a user and then request a password reset for that user.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266892)]
        public void RequestPasswordReset_UsernameForDeletedUser_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();
            user.DeleteUser();

            // Execute:
            Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordReset(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a deleted user.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a deleted user!");
        }

        [TestCase]
        [Description("Create and disable a user and then request a password reset for that user.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266893)]
        public void RequestPasswordReset_UsernameForDisabledUser_409Conflict()
        {
            // Setup:
            var user = UserFactory.CreateUserOnly();
            user.Enabled = false;
            user.CreateUser();

            // Execute:
            Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordReset(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a disabled user.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a disabled user!");
        }

        [TestCase]
        [Description("Create a user with no E-mail address and then request a password reset for that user.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266902)]
        public void RequestPasswordReset_UsernameForUserWithNoEmail_409Conflict()
        {
            // Setup:
            var user = UserFactory.CreateUserOnly();
            user.Email = null;
            user.CreateUser();

            // Execute:
            Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordReset(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a user with no E-mail address.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a user with no E-mail address!");
        }

        [TestCase]
        [Description("Request a password reset for that username that doesn't exist.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266894)]
        public void RequestPasswordReset_NonExistingUsername_409Conflict()
        {
            // Setup:
            string nonExistingUsername = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordReset(nonExistingUsername);
            }, "'POST {0}' should return 409 Conflict when passed a non-existing username.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(nonExistingUsername);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a non-existing user!");
        }

        [TestCase]
        [Description("Create a user and then request a password reset for that user several times in a row up to the maximum reset request limit.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266905)]
        public void RequestPasswordReset_ValidUsername_ExceedingMaxNumberOfRecoveryRequests_409Conflict()
        {
            // Setup:
            const int RESET_REQUEST_LIMIT = 3;

            var user = Helper.CreateUserAndAddToDatabase();
            var recoveryTokens = new List<PasswordRecoveryToken>();
            PasswordRecoveryToken recoveryToken = null;

            for (int i = 0; i < RESET_REQUEST_LIMIT; ++i)
            {
                Assert.DoesNotThrow(() =>
                {
                    Helper.AdminStore.RequestPasswordReset(user.Username);
                }, "'POST {0}' should return 200 OK when passed a valid username and the reset limit hasn't been exceeded.", REST_PATH);

                recoveryToken = GetRecoveryTokenFromDatabase(user.Username);
                recoveryTokens.Add(recoveryToken);
            }

            // Execute:
            Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordReset(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a user that exceeded the maximum password reset request attempts.", REST_PATH);

            // Verify:  The tokens in the list should be unique.
            recoveryToken = recoveryTokens[0];

            for (int i = 1; i < recoveryTokens.Count; ++i)
            {
                Assert.AreNotEqual(recoveryTokens[i].CreationTime, recoveryToken.CreationTime,
                    "The {0} of the tokens should be different!", nameof(PasswordRecoveryToken.CreationTime));
                Assert.AreNotEqual(recoveryTokens[i].RecoveryToken, recoveryToken.RecoveryToken,
                    "The {0} of the tokens should be different!", nameof(PasswordRecoveryToken.RecoveryToken));

                // TODO: Verify the RecoveryTokens aren't sequential.

                recoveryToken = recoveryTokens[i];
            }

            // No new recovery tokens should be added after the recovery limit was exceeded.
            var latestRecoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.AreEqual(recoveryTokens.Last().CreationTime, latestRecoveryToken.CreationTime, "The {0} of the tokens should be the same!",
                nameof(PasswordRecoveryToken.CreationTime));
            Assert.AreEqual(recoveryTokens.Last().RecoveryToken, latestRecoveryToken.RecoveryToken, "The {0} of the tokens should be the same!",
                nameof(PasswordRecoveryToken.RecoveryToken));
        }

        [TestCase]
        [Description("Create a user and change their password, then request a password reset for that user.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266903)]
        public void RequestPasswordReset_ValidUsername_UserRecentlyChangedTheirPassword_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            string newPassword = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8) + "Ab1$";
            Helper.AdminStore.ResetPassword(user, newPassword);
            user.Password = newPassword;

            // Execute:
            Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordReset(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a user that changed their password within the past 24-hours.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a user who changed their password within the past 24-hours!");
        }

        #endregion Negative tests

        #region Private functions

        /// <summary>
        /// A class to represent a row in the PasswordRecoveryTokens database table.
        /// </summary>
        private class PasswordRecoveryToken
        {
            public string Login { get; set; }
            public DateTime CreationTime { get; set; }
            public string RecoveryToken { get; set; }
        }

        /// <summary>
        /// Gets the latest PasswordRecoveryToken from the AdminStore database for the specified username.
        /// </summary>
        /// <param name="username">The username whose recovery token you want to get.</param>
        /// <returns>The latest PasswordRecoveryToken for the specified user, or null if no token was found for that user.</returns>
        private static PasswordRecoveryToken GetRecoveryTokenFromDatabase(string username)
        {
            string query = I18NHelper.FormatInvariant(
                "SELECT * FROM [dbo].[PasswordRecoveryTokens] WHERE [Login] = '{0}' ORDER BY [CreationTime] DESC", username);

            var columnNames = new List<string> { "Login", "CreationTime", "RecoveryToken" };

            try
            {
                var results = DatabaseHelper.ExecuteMultipleValueSqlQuery(query, columnNames, "AdminStore");
                string createTime = results["CreationTime"];

                return new PasswordRecoveryToken
                {
                    Login = results["Login"],
                    CreationTime = DateTime.Parse(createTime, CultureInfo.InvariantCulture),
                    RecoveryToken = results["RecoveryToken"]
                };
            }
            catch (SqlQueryFailedException)
            {
                Logger.WriteDebug("No PasswordRecoveryToken was found for user: {0}", username);
            }

            return null;
        }

        /// <summary>
        /// Verifies that a valid password recovery token was found in the database for the specified user.
        /// </summary>
        /// <param name="username">The username of the user whose password was attempted to be reset.</param>
        private static void ValidateRecoveryToken(string username)
        {
            var recoveryToken = GetRecoveryTokenFromDatabase(username);

            Assert.NotNull(recoveryToken, "No password recovery token was found in the database for user: {0}", username);
            Assert.AreEqual(username, recoveryToken.Login,
                "The recovery token Login should be equal to the username of the user whose password is being reset!");
            Assert.That(recoveryToken.CreationTime.CompareTimePlusOrMinusMilliseconds(DateTime.UtcNow, 60000),
                "The CreationTime of the recovery token should be equal to the current time (+/- 60s)!");
            Assert.IsFalse(string.IsNullOrWhiteSpace(recoveryToken.RecoveryToken), "The recovery token shouldn't be blank!");
        }

        #endregion Private functions
    }
}
