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

        [TestFixtureSetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Helper.AdminStore.DeleteUsers(_adminUser, Helper.InstanceUsers.Select(user => user.Id).ToList());

            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase(InstanceAdminRole.AssignInstanceAdministrators)]
        [TestCase(InstanceAdminRole.DefaultInstanceAdministrator)]
        [TestCase(InstanceAdminRole.ProvisionUsers)]
        [Description("Create and add several default instance users. Get the added users using a user that has permissions to " +
                     "view users. Verify the users that were created contained in the returned user list.")]
        [TestRail(303740)]
        public void GetInstanceUsers_PermissionsToGetUsers_ReturnsCorrectUsers(InstanceAdminRole adminRole)
        {
            // Setup:
            var addedUsers = Helper.CreateAndAddInstanceUsers(_adminUser, 5);

            var userWithPermissionsToGetUsers = Helper.CreateUserAndAuthenticate(
                TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(userWithPermissionsToGetUsers, offset: 0, limit: 999);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:

            var returnedUsers = (List<InstanceUser>)queryResult.Items;

            foreach (var createdUser in addedUsers)
            {
                var returnedUser = returnedUsers.Find(u => u.Id == createdUser.Id);

                Assert.IsNotNull(returnedUser, "Added user was not found in the list of returned users!");

                AdminStoreHelper.AssertAreEqual(createdUser, returnedUser);
            }
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
            Helper.CreateAndAddInstanceUsers(_adminUser, 5);

            var userWithInvalidTokenHeader = Helper.CreateUserWithInvalidToken(
                TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.DefaultInstanceAdministrator,
                badToken: tokenString);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetUsers(userWithInvalidTokenHeader, offset: 0, limit: 999);
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
            Helper.CreateAndAddInstanceUsers(_adminUser, 5);

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
    }
}
