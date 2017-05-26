using System;
using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Helper;
using Model;
using Model.Common.Enums;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]
    public class GetInstanceUsersTests : TestBase
    {
        private const string USER_PATH = RestPaths.Svc.AdminStore.Users.USERS;

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

        [Category(Categories.CannotRunInParallel)]
        [TestCase(InstanceAdminRole.AssignInstanceAdministrators)]
        [TestCase(InstanceAdminRole.DefaultInstanceAdministrator)]
        [TestCase(InstanceAdminRole.ProvisionUsers)]
        [Description("Create and add several default instance users. Get all users using a user that has permissions to " +
                     "view users. Verify the users that were created contained in the returned user list.")]
        [TestRail(303740)]
        public void GetInstanceUsers_PermissionsToGetUsers_ReturnsCorrectUsers(InstanceAdminRole adminRole)
        {
            // Setup:
            var userWithPermissionsToGetUsers = Helper.CreateUserAndAuthenticate(
                TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

            // Get list of current users immediately before adding new users
            var currentUsers = Helper.GetCurrentUsers(_adminUser);
            var addedUsers = Helper.CreateAndAddInstanceUsers(_adminUser, 5);

            // Expected users is all current users plus the newly created users
            var expectedTotalUsers = currentUsers.Concat(addedUsers);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(userWithPermissionsToGetUsers, offset: 0, limit: int.MaxValue);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = (List<InstanceUser>)queryResult.Items;

            foreach (var user in expectedTotalUsers)
            {
                var returnedUser = returnedUsers.Find(u => u.Id == user.Id);

                Assert.IsNotNull(returnedUser, "Added user was not found in the list of returned users!");

                AdminStoreHelper.AssertAreEqual(user, returnedUser);
            }
        }

        [Category(Categories.CannotRunInParallel)]
        [TestCase(10, -5, Description = "Offset of 0 and Limit 5 less that total user count")]
        [TestCase(10, 0, Description = "Offset of 0 and Limit equals total user count")]
        [TestCase(10, 5, Description = "Offset of 0 and Limit 5 more that total user count")]
        [Description("Create and add several default instance users. Get users with an offset of 0 and a modified limit. " +
                     "Verify that the correct number of users is returned.")]
        [TestRail(303978)]
        public void GetInstanceUsers_OffsetEqualsZeroLimitVaries_ReturnsCorrectNumberOfUsers(int numberOfUsersCreated, int limitModifier)
        {
            // Setup:
            var currentUsers = Helper.GetCurrentUsers(_adminUser);
            var addedUsers = Helper.CreateAndAddInstanceUsers(_adminUser, numberOfUsersCreated);

            // Expected users is all current users plus the newly created users
            var expectedTotalUsers = currentUsers.Concat(addedUsers).ToList();

            var expectedNumberOfReturnedUsers = CalculateExpectedNumberOfReturnedUsers(
                totalUsers: expectedTotalUsers.Count,
                offset: 0,
                limit: expectedTotalUsers.Count + limitModifier);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                // Dynamically adjust the limit using limit modifier
                queryResult = Helper.AdminStore.GetUsers(_adminUser, offset: 0, limit: expectedTotalUsers.Count + limitModifier);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = queryResult.Items;

            Assert.AreEqual(expectedNumberOfReturnedUsers, returnedUsers.Count(), "The number of users returned was not expected!");
        }

        [Category(Categories.CannotRunInParallel)]
        [TestCase(10, -10, 1, Description = "Offset of total user count minus 10 and Limit of 1")]
        [TestCase(10, -10, 5, Description = "Offset of total user count minus 10 and Limit of 5")]
        [TestCase(10, -10, 10, Description = "Offset of total user count minus 10 and Limit of 10")]
        [TestCase(10, -10, 20, Description = "Offset of total user count minus 10 and Limit of 20")]
        [TestCase(10, 1, 20, Description = "Offset of total user count plus 1 and Limit of 20")]
        [Description("Create and add several default instance users. Modify both the offset and the limit. " +
                     "Verify that the correct number of users is returned.")]
        [TestRail(303977)]
        public void GetInstanceUsers_OffsetGreaterThanZeroLimitVaries_ReturnsCorrectNumberOfUsers(int numberOfUsersCreated, int offsetModifier, int limit)
        {
            // Setup:
            var currentUsers = Helper.GetCurrentUsers(_adminUser);
            var addedUsers = Helper.CreateAndAddInstanceUsers(_adminUser, numberOfUsersCreated);

            // Expected users is all current users plus the newly created users
            var expectedTotalUsers = currentUsers.Concat(addedUsers).ToList();

            var expectedNumberOfReturnedUsers = CalculateExpectedNumberOfReturnedUsers(
                totalUsers: expectedTotalUsers.Count,
                offset: expectedTotalUsers.Count + offsetModifier,
                limit: limit);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                // Dynaically modify the offset using an offset modifier
                queryResult = Helper.AdminStore.GetUsers(_adminUser, 
                    offset: expectedTotalUsers.Count + offsetModifier, 
                    limit: limit);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = queryResult.Items;

            Assert.AreEqual(expectedNumberOfReturnedUsers, returnedUsers.Count(), "The number of users returned was not expected!");
        }

        [Category(Categories.CannotRunInParallel)]
        [TestCase]
        [Description("Create and add a default instance users. Get all users. Verify the user that was created contained in the " +
                     "returned user list. Delete the user. Get all users again. Verify the user is not in the returned user list.")]
        [TestRail(303979)]
        public void GetInstanceUsers_DeleteUser_DeletedUserNotReturned()
        {
            // Setup:
            // Get list of current users immediately before adding new users
            var currentUsers = Helper.GetCurrentUsers(_adminUser);
            var addedUser = Helper.CreateAndAddInstanceUser(_adminUser);

            currentUsers.Add(addedUser);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Helper.AdminStore.DeleteUser(_adminUser, (int)addedUser.Id);

            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(_adminUser, offset: 0, limit: int.MaxValue);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = (List<InstanceUser>)queryResult.Items;

            Assert.IsNull(returnedUsers.Find(user => user.Id == addedUser.Id), "The deleted user was returned!");
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase(null, "Token is missing or malformed.")]
        [TestCase("", "Token is invalid.")]
        [TestCase(CommonConstants.InvalidToken, "Token is invalid.")]
        [Description("Create and add several default instance users. Try to get the users using an invalid token header. " +
                     "Verify that 401 Unauthorized is returned.")]
        [TestRail(303745)]
        public void GetInstanceUsers_InvalidTokenHeader_401Unauthorized(string tokenString, string errorMessage)
        {
            // Setup:
            var userWithInvalidTokenHeader = Helper.CreateUserWithInvalidToken(
                TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.DefaultInstanceAdministrator,
                badToken: tokenString);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetUsers(userWithInvalidTokenHeader, offset: 0, limit: int.MaxValue);
            }, "'GET {0}' should return 401 Unauthorized with invalid token header!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, errorMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase(null)]
        [TestCase(InstanceAdminRole.AdministerALLProjects)]
        [TestCase(InstanceAdminRole.BlueprintAnalytics)]
        [TestCase(InstanceAdminRole.Email_ActiveDirectory_SAMLSettings)]
        [TestCase(InstanceAdminRole.InstanceStandardsManager)]
        [TestCase(InstanceAdminRole.LogGatheringAndLicenseReporting)]
        [TestCase(InstanceAdminRole.ManageAdministratorRoles)]
        [TestCase(InstanceAdminRole.ProvisionProjects)]
        [Description("Create and add several instance users.  Try to get the users with another user that does not have " +
                     "permission to view users. Verify that 401 Unauthorized is returned.")]
        [TestRail(303746)]
        public void GetInstanceUsers_NoPermissionsToGetUsers_403Forbidden(InstanceAdminRole? adminRole)
        {
            // Setup:
            var userWithNoPermissionsToGetUsers = Helper.CreateUserAndAuthenticate(
                TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.AdminStore.GetUsers(userWithNoPermissionsToGetUsers, offset: 0, limit: 999);
            },
            "'PUT {0}' should return 403 Forbidden when the user updating the user has no permissions to get users!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "The user does not have permissions.");
        }

        #endregion 403 Forbidden Tests

        #region Private Methods

        /// <summary>
        /// Calculates the number of users expected to be returned from the GetUsers API call
        /// </summary>
        /// <param name="totalUsers">The total number of existing users.</param>
        /// <param name="offset">The offset applied to the API call.</param>
        /// <param name="limit">The limit applied to the API call.</param>
        /// <returns>The number of users expected in the API call response.</returns>
        private static int CalculateExpectedNumberOfReturnedUsers(int totalUsers, int offset, int limit)
        {
            return offset >= totalUsers ? 0 : Math.Min(totalUsers - offset, limit);
        }

        #endregion Private Methods
    }
}
