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
using Utilities.Facades;
using Utilities.Factories;

namespace AdminStoreTests
{
    [Category(Categories.AdminStore)]
    [TestFixture]
    public class PasswordRecoveryTests : TestBase
    {
        private const string REST_PATH = RestPaths.Svc.AdminStore.Users.PasswordRecovery.REQUEST;

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
        [Description("Create a user and then request a password reset for that user.  Verify 200 OK is returned and a RecoveryToken for that user " +
                     "was added to the AdminStore database.")]
        [TestRail(266880)]
        public void RequestPasswordRecovery_ValidUsername_RecoveryTokenIsCreated(TestHelper.ProjectRole role)
        {
            // Setup:
            var user = Helper.CreateUserWithProjectRolePermissions(role, _project);
            RestResponse response = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                response = Helper.AdminStore.RequestPasswordRecovery(user.Username);
            }, "'POST {0}' should return 200 OK when passed a valid username.", REST_PATH);

            // Verify:
            ValidateRecoveryToken(user.Username);
            TestHelper.AssertResponseBodyIsEmpty(response);
        }

        #endregion Positive tests

        #region Negative tests

        [Explicit(IgnoreReasons.ProductBug)]    // Trello bug:  https://trello.com/c/ifyXsRc6  Gets a 500 error.
        [TestCase]
        [Description("Create and delete a user and then request a password reset for that user.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266892)]
        public void RequestPasswordRecovery_UsernameForDeletedUser_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAddToDatabase();
            user.DeleteUser();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordRecovery(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a deleted user.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a deleted user!");
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
        }

        [TestCase]
        [Description("Create and disable a user and then request a password reset for that user.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266893)]
        public void RequestPasswordRecovery_UsernameForDisabledUser_409Conflict()
        {
            // Setup:
            var user = UserFactory.CreateUserOnly();
            user.Enabled = false;
            user.CreateUser();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordRecovery(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a disabled user.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a disabled user!");
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
        }

        [TestCase]
        [Description("Create a user with no E-mail address and then request a password reset for that user.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266902)]
        public void RequestPasswordRecovery_UsernameForUserWithNoEmail_409Conflict()
        {
            // Setup:
            var user = UserFactory.CreateUserOnly();
            user.Email = null;
            user.CreateUser();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordRecovery(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a user with no E-mail address.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a user with no E-mail address!");
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
        }

        [Explicit(IgnoreReasons.ProductBug)]    // Trello bug:  https://trello.com/c/ifyXsRc6  Gets a 500 error.
        [TestCase]
        [Description("Request a password reset for that username that doesn't exist.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266894)]
        public void RequestPasswordRecovery_NonExistingUsername_409Conflict()
        {
            // Setup:
            string nonExistingUsername = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordRecovery(nonExistingUsername);
            }, "'POST {0}' should return 409 Conflict when passed a non-existing username.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(nonExistingUsername);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a non-existing user!");
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
        }

        [TestCase]
        [Description("Create a user and then request a password reset for that user several times in a row up to the maximum reset request limit.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266905)]
        public void RequestPasswordRecovery_ValidUsername_ExceedingMaxNumberOfRecoveryRequests_409Conflict()
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
                    Helper.AdminStore.RequestPasswordRecovery(user.Username);
                }, "'POST {0}' should return 200 OK when passed a valid username and the reset limit hasn't been exceeded.", REST_PATH);

                recoveryToken = GetRecoveryTokenFromDatabase(user.Username);
                recoveryTokens.Add(recoveryToken);
            }

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordRecovery(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a user that exceeded the maximum password reset request attempts.", REST_PATH);

            // Verify:
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);

            // The tokens in the list should be unique.
            recoveryToken = recoveryTokens[0];

            for (int i = 1; i < recoveryTokens.Count; ++i)
            {
                Assert.AreNotEqual(recoveryTokens[i].CreationTime, recoveryToken.CreationTime,
                    "The {0} of the tokens should be different!", nameof(PasswordRecoveryToken.CreationTime));
                Assert.AreNotEqual(recoveryTokens[i].RecoveryToken, recoveryToken.RecoveryToken,
                    "The {0} of the tokens should be different!", nameof(PasswordRecoveryToken.RecoveryToken));

                // Verify the RecoveryTokens aren't sequential.
                AssertGuidsAreNotSimilar(recoveryToken, recoveryTokens[i]);

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
        public void RequestPasswordRecovery_ValidUsername_UserRecentlyChangedTheirPassword_409Conflict()
        {
            // Setup:
            var user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            string newPassword = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8) + "Ab1$";
            Helper.AdminStore.ResetPassword(user, newPassword);
            user.Password = newPassword;

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.RequestPasswordRecovery(user.Username);
            }, "'POST {0}' should return 409 Conflict when passed a username for a user that changed their password within the past 24-hours.", REST_PATH);

            // Verify:
            var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.IsNull(recoveryToken, "No password recovery token should be created for a user who changed their password within the past 24-hours!");
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
        }

        [Category(Categories.CannotRunInParallel)]  // Since it changes a global config setting, it might interfere with other tests, especially in this class.
        [TestCase]
        [Description("Create a user and disable the 'IsPasswordRecoveryEnabled' setting in the database, then request a password reset for that user.  " +
                     "Verify 409 Conflict is returned and no RecoveryToken for that user was added to the AdminStore database.")]
        [TestRail(266954)]
        public void RequestPasswordRecovery_ValidUsername_PasswordRecoveryIsDisabled_409Conflict()
        {
            // Setup:
            const string IsPasswordRecoveryEnabled = "IsPasswordRecoveryEnabled";
            var user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            try
            {
                // Disable IsPasswordRecoveryEnabled in the database.
                TestHelper.UpdateApplicationSettings(IsPasswordRecoveryEnabled, "false");

                // Execute:
                var ex = Assert.Throws<Http409ConflictException>(() =>
                {
                    Helper.AdminStore.RequestPasswordRecovery(user.Username);
                }, "'POST {0}' should return 409 Conflict when the {1} Application Setting is disabled.",
                    REST_PATH, IsPasswordRecoveryEnabled);

                // Verify:
                var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

                Assert.IsNull(recoveryToken, "No password recovery token should be created for a user who changed their password within the past 24-hours!");
                TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
            }
            finally
            {
                // Re-enable IsPasswordRecoveryEnabled in the database.
                TestHelper.UpdateApplicationSettings(IsPasswordRecoveryEnabled, "true");
            }
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
        /// Asserts that the RecoveryToken GUIDs aren't too similar to each other.
        /// </summary>
        /// <param name="recoveryToken1">The first token to compare.</param>
        /// <param name="recoveryToken2">The second token to compare.</param>
        private static void AssertGuidsAreNotSimilar(PasswordRecoveryToken recoveryToken1, PasswordRecoveryToken recoveryToken2)
        {
            var firstGuid = Guid.Parse(recoveryToken1.RecoveryToken);
            var secondGuid = Guid.Parse(recoveryToken2.RecoveryToken);

            var firstTokenBytes = firstGuid.ToByteArray();
            var secondTokenBytes = secondGuid.ToByteArray();

            // Compare the GUIDs in 4 byte chunks for similarity.
            while (firstTokenBytes.Length > 0)
            {
                int firstNum = BitConverter.ToInt32(firstTokenBytes.Take(4).ToArray(), 0);
                int secondNum = BitConverter.ToInt32(secondTokenBytes.Take(4).ToArray(), 0);

                Assert.That(Math.Abs(firstNum - secondNum) > 2, "The GUIDs are too similar!  '{0}' vs '{1}'",
                    firstGuid.ToStringInvariant(), secondGuid.ToStringInvariant());

                firstTokenBytes = firstTokenBytes.Skip(4).ToArray();
                secondTokenBytes = secondTokenBytes.Skip(4).ToArray();
            }
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
            Assert.That(recoveryToken.CreationTime.CompareTimePlusOrMinusMilliseconds(DateTime.Now, 60000),
                "The CreationTime of the recovery token should be equal to the current time (+/- 60s)!");
            Assert.IsFalse(string.IsNullOrWhiteSpace(recoveryToken.RecoveryToken), "The recovery token shouldn't be blank!");
        }

        #endregion Private functions
    }
}
