﻿using System;
using System.Collections.Generic;
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
        private const string USER_PATH_ID = RestPaths.Svc.AdminStore.Users.USERS_id_;

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
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase(InstanceAdminRole.AssignInstanceAdministrators)]
        [TestCase(InstanceAdminRole.DefaultInstanceAdministrator)]
        [TestCase(InstanceAdminRole.ProvisionUsers)]
        [Description("Create and add a default instance user. Get the added user using a user that has permissions to " +
                     "view users. Verify the same user that was created is returned.")]
        [TestRail(999999)]
        public void GetInstanceUsers_PermissionsToGetUser_ReturnsCorrectUser(InstanceAdminRole adminRole)
        {
            // Setup:
            var addedUsers = Helper.CreateAndAddInstanceUsers(_adminUser, 5);

            var userWithPermissionsToGetUsers = Helper.CreateUserAndAuthenticate(
                TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

            QueryResult<InstanceUser> queryResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                queryResult = Helper.AdminStore.GetUsers(userWithPermissionsToGetUsers, offset: 0, limit: 10);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

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
        [Description("Create and add an instance user. Try to get the user using an invalid token header. " +
                     "Verify that 401 Unauthorized is returned.")]
        [TestRail(999999)]
        public void GetInstanceUser_InvalidTokenHeader_401Unauthorized(string tokenString, string errorMessage)
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

            var userWithInvalidTokenHeader = Helper.CreateUserWithInvalidToken(
                TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.DefaultInstanceAdministrator,
                badToken: tokenString);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetUserById(userWithInvalidTokenHeader, createdUser.Id);
            }, "'PUT {0}' should return 401 Unauthorized with invalid token header!", USER_PATH_ID);

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
        [Description("Create and add an instance user.  Try to get the user with another user that does not have " +
                     "permission to view users. Verify that 401 Unauthorized is returned.")]
        [TestRail(999999)]
        public void GetInstanceUser_NoPermissionsToGetUsers_403Forbidden(InstanceAdminRole? adminRole)
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

            var userWithNoPermissionsToGetUsers = Helper.CreateUserAndAuthenticate(
                TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.AdminStore.GetUserById(userWithNoPermissionsToGetUsers, createdUser.Id);
            },
            "'PUT {0}' should return 403 Forbidden when the user updating the user has no permissions to get users!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "The user does not have permissions.");
        }

        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        [Description("Try to get the non-existing user. Verify that 404 Not Found is returned.")]
        [TestRail(999999)]
        public void GetInstanceUser_UserDoesntExist_404NotFound(int userId)
        {
            // Setup, Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.GetUserById(_adminUser, userId);
            }, "'PUT {0}' should return 404 Not Found for nonexistent user!", USER_PATH_ID);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create and add an instance user. Delete the user.  Try to get the deleted user. " +
                     "Verify that 404 Not Found is returned.")]
        [TestRail(303456)]
        public static void GetInstanceUser_UserDeleted_404NotFound()
        {
            throw new NotImplementedException();
        }

        #endregion 404 Not Found Tests

    }
}
