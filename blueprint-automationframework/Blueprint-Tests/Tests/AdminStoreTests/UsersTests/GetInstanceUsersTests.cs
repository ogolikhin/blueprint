using System;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Common.Enums;
using Model.Impl;
using Model.NovaModel.AdminStoreModel;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Factories;

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
        [TestCase]
        [Description("Create and add several default instance users. Get all users using a user that only has permissions to " +
                     "view users. Verify the users that were created contained in the returned user list.")]
        [TestRail(303740)]
        public void GetInstanceUsers_PermissionsToGetUsers_ReturnsCorrectUsers()
        {
            using (var adminStoreHelper = new AdminStoreHelper())
            {
                // Setup:
                var adminRole = adminStoreHelper.AddInstanceAdminRoleToDatabase(InstanceAdminPrivileges.ViewUsers);

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
                var returnedUsers = queryResult.Items;

                foreach (var user in expectedTotalUsers)
                {
                    var returnedUser = returnedUsers.Find(u => u.Id == user.Id);

                    Assert.IsNotNull(returnedUser, "Added user was not found in the list of returned users!");

                    AdminStoreHelper.AssertAreEqual(user, returnedUser);
                }
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
        [TestCase(10, -10, 0, Description = "Offset of total user count minus 10 and Limit of 0")]
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

        [Description("Create and add several default instance users. Get users with an offset of 0 and a limit of zero. " +
                     "Verify that no users are returned.")]
        [TestRail(303982)]
        public void GetInstanceUsers_OffsetAndLimitEqualsZero_ReturnsCorrectNumberOfUsers()
        {
            // Setup:
            Helper.CreateAndAddInstanceUsers(_adminUser, 5);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(_adminUser, offset: 0, limit: 0);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = queryResult.Items;

            Assert.AreEqual(0, returnedUsers.Count(), "Users were returned but non were expected!");
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
            var returnedUsers = queryResult.Items;

            Assert.IsNull(returnedUsers.Find(user => user.Id == addedUser.Id), "The deleted user was returned!");
        }

        [TestCase]
        [Description("Create and add an instance user with a specific login. Get all users with search parameter that contains a substring " + 
                     "of user's login. Verify that a single created user is returned in the result.")]
        [TestRail(308901)]
        public void GetInstanceUsers_SearchCreatedUserByLogin_ReturnsSingleUserResult()
        {
            // Setup:
            var login = RandomGenerator.RandomAlphaNumeric(25);
            var search = login.Substring(5, 20);
            Helper.CreateAndAddInstanceUser(_adminUser, login: login);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(_adminUser, offset: 0, limit: 20, search: search);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = queryResult.Items;
            var returnedUsersCount = returnedUsers.Count;

            Assert.AreEqual(1, returnedUsersCount, returnedUsersCount + " users were returned but a single user was expected!");
            Assert.AreEqual(login, returnedUsers[0].Login, "Expected user's login to be '" + login + "' but found '" + returnedUsers[0].Login + "'");
        }

        [TestCase]
        [Description("Create and add an instance user with a specific display name. Get all users with search parameter that contains a substring " +
                     "of user's display name. Verify that a single created user is returned in the result.")]
        [TestRail(308902)]
        public void GetInstanceUsers_SearchCreatedUserByDisplayName_ReturnsSingleUserResult()
        {
            // Setup:
            var displayName = RandomGenerator.RandomAlphaNumeric(25);
            var search = displayName.Substring(5, 20);
            Helper.CreateAndAddInstanceUser(_adminUser, displayname: displayName);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(_adminUser, offset: 0, limit: 20, search: search);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = queryResult.Items;
            var returnedUsersCount = returnedUsers.Count;

            Assert.AreEqual(1, returnedUsersCount, returnedUsersCount + " users were returned but a single user was expected!");
            Assert.AreEqual(displayName, returnedUsers[0].DisplayName, "Expected user's display name to be '" + displayName + "' but found '" + returnedUsers[0].DisplayName + "'");
        }

        [TestCase]
        [Description("Create and add an instance user with a specific e-mail address. Get all users with search parameter that contains a substring " +
                     "of user's e-mail address. Verify that a single created user is returned in the result.")]
        [TestRail(308903)]
        public void GetInstanceUsers_SearchCreatedUserByEmail_ReturnsSingleUserResult()
        {
            // Setup:
            var domain = RandomGenerator.RandomAlphaNumeric(15) + ".com";
            var email = I18NHelper.FormatInvariant("{0}@{1}", RandomGenerator.RandomAlphaNumeric(5), domain);
            Helper.CreateAndAddInstanceUser(_adminUser, email: email);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(_adminUser, offset: 0, limit: 20, search: domain);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = queryResult.Items;
            var returnedUsersCount = returnedUsers.Count;

            Assert.AreEqual(1, returnedUsersCount, returnedUsersCount + " users were returned but a single user was expected!");
            Assert.AreEqual(email, returnedUsers[0].Email, "Expected user's e-mail name to be '" + email + "' but found '" + returnedUsers[0].Email + "'");
        }

        [TestCase]
        [Description("Create and add 5 instance users with specific e-mail addresses. Get all users with search parameter that contains a substring " +
                     "of users' e-mail address. Verify that all 5 matching users are returned in the result.")]
        [TestRail(308913)]
        public void GetInstanceUsers_SearchMultipleCreatedUsersByEmail_ReturnsAllMatchingUsersInResult()
        {
            // Setup:
            var domain = RandomGenerator.RandomAlphaNumeric(15) + ".com";
            var email1 = I18NHelper.FormatInvariant("{0}@{1}", RandomGenerator.RandomAlphaNumeric(5), domain);
            var email2 = I18NHelper.FormatInvariant("{0}@{1}", RandomGenerator.RandomAlphaNumeric(5), domain);
            var email3 = I18NHelper.FormatInvariant("{0}@{1}", RandomGenerator.RandomAlphaNumeric(5), domain);
            var email4 = I18NHelper.FormatInvariant("{0}@{1}", RandomGenerator.RandomAlphaNumeric(5), domain);
            var email5 = I18NHelper.FormatInvariant("{0}@{1}", RandomGenerator.RandomAlphaNumeric(5), domain);


            Helper.CreateAndAddInstanceUser(_adminUser, email: email1);
            Helper.CreateAndAddInstanceUser(_adminUser, email: email2);
            Helper.CreateAndAddInstanceUser(_adminUser, email: email3);
            Helper.CreateAndAddInstanceUser(_adminUser, email: email4);
            Helper.CreateAndAddInstanceUser(_adminUser, email: email5);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(_adminUser, offset: 0, limit: 20, search: domain);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = queryResult.Items;
            var returnedUsersCount = returnedUsers.Count;
            var expectedUsersCount = 5;

            Assert.AreEqual(expectedUsersCount, returnedUsersCount, returnedUsersCount + " users were returned but a " + expectedUsersCount + " users was expected!");
            Assert.IsTrue(returnedUsers.Any(user => user.Email == email1), "Expected user with e-mail '" + email1 + "' to be returned, but none were found");
            Assert.IsTrue(returnedUsers.Any(user => user.Email == email2), "Expected user with e-mail '" + email2 + "' to be returned, but none were found");
            Assert.IsTrue(returnedUsers.Any(user => user.Email == email3), "Expected user with e-mail '" + email3 + "' to be returned, but none were found");
            Assert.IsTrue(returnedUsers.Any(user => user.Email == email4), "Expected user with e-mail '" + email4 + "' to be returned, but none were found");
            Assert.IsTrue(returnedUsers.Any(user => user.Email == email5), "Expected user with e-mail '" + email5 + "' to be returned, but none were found");
        }

        [Category(Categories.CannotRunInParallel)]
        [TestCase]
        [Description("Get all users with search parameter that doesn't match any existing users. Verify that no users are returned as a result.")]
        [TestRail(308904)]
        public void GetInstanceUsers_SearchNonExistingUsers_ReturnsNoResults()
        {
            // Setup:
            var search = "jonnygreenwood";

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(_adminUser, offset: 0, limit: 20, search: search);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
            var returnedUsers = queryResult.Items;
            var returnedUsersCount = returnedUsers.Count;

            Assert.AreEqual(0, returnedUsersCount, returnedUsersCount + " users were returned but none was expected!");
        }

        #endregion 200 OK Tests

        #region 400 BadRequest

        [TestCase(null, null, InstanceAdminErrorMessages.InvalidPagination)]
        [TestCase(-1, null, InstanceAdminErrorMessages.IncorrectOffsetParameter)]
        [TestCase(0, null, InstanceAdminErrorMessages.IncorrectLimitParameter)]
        [TestCase(null, -1, InstanceAdminErrorMessages.IncorrectOffsetParameter)]
        [TestCase(-1, -1, InstanceAdminErrorMessages.IncorrectOffsetParameter)]
        [TestCase(0, -1, InstanceAdminErrorMessages.IncorrectLimitParameter)]
        [TestCase(null, 0, InstanceAdminErrorMessages.IncorrectOffsetParameter)]
        [TestCase(-1, 0, InstanceAdminErrorMessages.IncorrectOffsetParameter)]
        [TestCase(null, int.MaxValue, InstanceAdminErrorMessages.IncorrectOffsetParameter)]
        [TestCase(-1, int.MaxValue, InstanceAdminErrorMessages.IncorrectOffsetParameter)]
        [TestCase(int.MaxValue, null, InstanceAdminErrorMessages.IncorrectLimitParameter)]
        [TestCase(int.MaxValue, -1, InstanceAdminErrorMessages.IncorrectLimitParameter)]
        [Description("Get users using an invalid offset value and/or invalid limit value. Verify that 400 Bad Request is returned.")]
        [TestRail(303745)]
        public void GetInstanceUsers_InvalidOffsetAndOrLimit_400BadRequest(int? offset, int? limit, string errorMessage)
        {
            // Setup & Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.GetUsers(_adminUser, offset: offset, limit: limit);
            }, "'GET {0}' should return 400 Bad Request with invalid token header!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, errorMessage);
        }

        #endregion 400 Bad Request

        #region 401 Unauthorized Tests

        [TestCase(null, InstanceAdminErrorMessages.TokenMissingOrMalformed)]
        [TestCase("", InstanceAdminErrorMessages.TokenInvalid)]
        [TestCase(CommonConstants.InvalidToken, InstanceAdminErrorMessages.TokenInvalid)]
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

        [TestCase]
        [Description("Create and add several instance users.  Try to get the users with another user that has all permissions except " +
                     "permission to view users. Verify that 403 Forbidden is returned.")]
        [TestRail(303746)]
        public void GetInstanceUsers_NoPermissionsToGetUsers_403Forbidden()
        {
            using (var adminStoreHelper = new AdminStoreHelper())
            {
                // Setup:
                var allPrivilegesExceptViewUsers = (InstanceAdminPrivileges) int.MaxValue & ~InstanceAdminPrivileges.ViewUsers;
                var adminRole = adminStoreHelper.AddInstanceAdminRoleToDatabase(allPrivilegesExceptViewUsers);

                var userWithNoPermissionsToGetUsers = Helper.CreateUserAndAuthenticate(
                    TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

                // Execute:
                var ex = Assert.Throws<Http403ForbiddenException>(() =>
                {
                    Helper.AdminStore.GetUsers(userWithNoPermissionsToGetUsers, offset: 0, limit: 999);
                },
                    "'GET {0}' should return 403 Forbidden when the user updating the user has no permissions to get users!", USER_PATH);

                // Verify:
                TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.Forbidden, InstanceAdminErrorMessages.UserDoesNotHavePermissions);
            }
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
