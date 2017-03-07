using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

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
            var recoveryToken = GetRecoveryTokenFromDatabase(user.Username);

            Assert.NotNull(recoveryToken, "No password recovery token was found in the database for user: {0}", user.Username);
            Assert.AreEqual(user.Username, recoveryToken.Login,
                "The recovery token Login should equal the username of the user whose password is being reset!");
            Assert.That(recoveryToken.CreationTime.CompareTimePlusOrMinusMilliseconds(DateTime.UtcNow, 60000),
                "The CreationTime of the recovery token should be equal to the current time (+/- 60s)!");
            Assert.IsFalse(string.IsNullOrWhiteSpace(recoveryToken.RecoveryToken), "The recovery token shouldn't be blank!");
        }

        #endregion Positive tests

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

        #endregion Private functions
    }
}
